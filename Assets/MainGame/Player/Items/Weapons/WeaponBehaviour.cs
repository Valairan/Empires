using Unity.Netcode;
using UnityEngine;

public class WeaponBehaviour : ItemBehaviour, IRaycastResponder, IInteractable
{
    public bool isAttacking = false;
    public Weapon baseWeapon;
    public Transform ik_target;
    public float InteractionDuration => 1f;

    public override void OnNetworkSpawn()
    {
        baseitem = baseWeapon;
    }

    public void Init(ulong sender)
    {
        transform.localPosition = baseWeapon.position;
        transform.localRotation = Quaternion.Euler(baseWeapon.rotation);
        transform.localScale = baseWeapon.scale;
        if (!IsOwner)
            enabled = false;
    }

    public Item Interact(ulong interactor)
    {

        attemptToInteract_ServerRpc(interactor, NetworkObjectId);
        return baseitem;
    }

    public Item respondToRaycast()
    {
        return baseitem;
    }

    [ServerRpc(RequireOwnership = false)]
    void attemptToInteract_ServerRpc(ulong interactingPlayerId, ulong interactee)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(interactingPlayerId, out NetworkClient client))
            return;
        NetworkObject playerObject = client.PlayerObject;
        if (playerObject == null)
            return;
        if (!playerObject.TryGetComponent(out PlayerController player))
            return;

        if (baseitem == null)
            return;

        float distance = Vector3.Distance(
            player.transform.position,
            transform.position
        );
        if (distance > 2.5f) // interaction range
            return;

        baseitem.OnPickup(player.GetComponent<InventoryHandler>(), this);
    }
}