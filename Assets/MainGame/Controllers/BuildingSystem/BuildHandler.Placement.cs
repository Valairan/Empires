using UnityEngine;

public partial class BuildHandler
{
    private GameObject previewGO;
    [SerializeField] private float maxBuildDistance = 8f;

    private bool inPreview;
    private Transform[] placementPoints;

    private Vector3 snapToGrid(Vector3 rawPosition)
    {
        return new Vector3(
            Mathf.RoundToInt(rawPosition.x),
            Mathf.RoundToInt(rawPosition.y),
            Mathf.RoundToInt(rawPosition.z)
        );
    }

    private Vector3 calculatePreviewPosition(Vector3 startPosition, Vector3 forwardVec)
    {
        if (Physics.Raycast(startPosition, forwardVec, out RaycastHit hit, maxBuildDistance, currentMachine.buildableLayers))
        {
            Vector3 point = hit.point;
            point.y = transform.position.y;
            return snapToGrid(point);
        }

        Vector3 temp = startPosition + (forwardVec * 10f);
        temp.y = transform.position.y;
        return snapToGrid(temp);
    }

    public bool CheckPlacements(Transform[] points)
    {
        if (points == null || points.Length == 0 || currentMachine == null)
            return false;

        bool hasBuildableHit = false;

        foreach (Transform point in points)
        {
            if (point == null) continue;
            Vector3 pos = point.position;

            if (Physics.OverlapSphere(pos, 0.4f, currentMachine.blockingLayers).Length > 0)
                return false;

            if (Physics.OverlapSphere(pos, 0.4f, currentMachine.buildableLayers).Length > 0)
                hasBuildableHit = true;
        }

        return hasBuildableHit;
    }

    public void previewBuild(Vector3 startPosition, Vector3 forwardVec)
    {
        if (!inPreview || previewGO == null || currentMachine == null)
            return;

        // 1. Move preview to new snapped position FIRST
        previewGO.transform.position = calculatePreviewPosition(startPosition, forwardVec);

        // 2. Validate at updated position
        IsValidPlacement = true;// CheckPlacements(placementPoints);

    }

    public void rotateButtonPressed()
    {
        if (!inPreview || previewGO == null)
            return;

        ValidRotation *= Quaternion.Euler(0f, 45f, 0f);
        previewGO.transform.rotation = ValidRotation;
    }

    public void CancelButtonPressed()
    {
        if (previewGO)
            Destroy(previewGO);

        inPreview = false;
        previewGO = null;
        currentMachine = null;
        IsValidPlacement = false;
        ValidRotation = Quaternion.identity;
    }

    public bool startPreview()
    {
        if (currentMachine == null) return false;

        if (!currentMachine.cost.CanBeCrafted(inventoryHandler.coins, inventoryHandler.timber, inventoryHandler.iron, inventoryHandler.stone))
            return false;

        if (previewGO != null) Destroy(previewGO);

        previewGO = Instantiate(currentMachine.preview.gameObject);

        // Cache placement points safely without IndexOutOfRange exceptions
        if (previewGO.TryGetComponent(out BuildableBehaviour mb) && mb.placementPoints != null && mb.placementPoints.Length > 0)
        {
            placementPoints = mb.placementPoints;
        }
        else
        {
            int childCount = previewGO.transform.childCount;
            placementPoints = new Transform[childCount];
            for (int i = 0; i < childCount; i++)
            {
                placementPoints[i] = previewGO.transform.GetChild(i);
            }
        }

        inPreview = true;
        ValidRotation = Quaternion.identity;
        return inPreview;
    }
}