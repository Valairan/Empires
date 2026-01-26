using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Buildable : NetworkBehaviour
{
    [Header("Placement")]
    [SerializeField] private Transform[] placementPoints; // your empty GameObjects
    [SerializeField] private LayerMask blockingLayers;
    [SerializeField] private LayerMask terrainLayer;
    [SerializeField] private float raycastDownDistance = 5f;
    [SerializeField] private float raycastUpDistance = 2f;

    public bool IsPlaced;
    public bool IsValidPlacement;
    public Quaternion ValidRotation;

    static readonly Quaternion[] CardinalRotations =
    {
        Quaternion.Euler(0, 0, 0),
        Quaternion.Euler(0, 90, 0),
        Quaternion.Euler(0, 180, 0),
        Quaternion.Euler(0, 270, 0)
    };

    void Update()
    {
        if (IsPlaced || !IsClient) return;

        IsValidPlacement = CheckAllCardinalPlacements(transform.position, out ValidRotation);
    }

    bool CheckAllCardinalPlacements(Vector3 pos, out Quaternion validRotation)
    {
        foreach (var rot in CardinalRotations)
        {
            if (CheckPlacementAt(pos, rot))
            {
                validRotation = rot;
                return true;
            }
        }

        validRotation = Quaternion.identity;
        return false;
    }

    bool CheckPlacementAt(Vector3 pos, Quaternion rot)
    {
        foreach (var point in placementPoints)
        {
            Vector3 worldPos = pos + rot * (point.localPosition);

            // 1️⃣ Horizontal overlap check (optional for irregular objects)
            Collider[] hits = Physics.OverlapSphere(worldPos, 0.1f, blockingLayers);
            foreach (var hit in hits)
            {
                if (hit.transform.IsChildOf(transform)) continue;
                return false; // something blocking horizontally
            }

            // 2️⃣ Downward raycast
            if (!Physics.Raycast(worldPos, Vector3.down, out RaycastHit hitDown, raycastDownDistance, terrainLayer))
                return false; // nothing to stand on

            // 3️⃣ Upward raycast
            if (Physics.Raycast(worldPos, Vector3.up, raycastUpDistance, blockingLayers))
                return false; // blocked above
        }

        return true; // all points valid
    }

    public bool TryPlace()
    {
        if (!IsValidPlacement) return false;
        TryPlace_ServerRpc(transform.position, ValidRotation);
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    void TryPlace_ServerRpc(Vector3 pos, Quaternion rot)
    {
        if (!CheckPlacementAt(pos, rot)) return;
        PlaceInternal(pos, rot);
    }

    void PlaceInternal(Vector3 pos, Quaternion rot)
    {
        transform.SetPositionAndRotation(new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z), rot);
        IsPlaced = true;
        if (!NetworkObject.IsSpawned)
            GetComponent<NetworkObject>().Spawn();
    }


}
