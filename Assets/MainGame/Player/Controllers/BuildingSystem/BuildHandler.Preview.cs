using UnityEngine;

public partial class BuildHandler
{
    GameObject previewGO;
    bool inPreview;
    Transform[] placementPoints;

    Vector3 calculatePreviewPosition(Vector3 startPosition, Vector3 forwardVec)
    {
        if (Physics.Raycast(startPosition, forwardVec, out RaycastHit hit, 100, currentMachine.buildableLayers))
        {
            if (hit.transform.position.y == transform.position.y)
            {
                return new Vector3Int(
                    Mathf.RoundToInt(hit.point.x),
                    Mathf.RoundToInt(transform.position.y),
                    Mathf.RoundToInt(hit.point.z));
            }
            else
            {
                Vector3 temp = startPosition + (forwardVec * 10);
                temp.y = transform.position.y;
                temp.x = Mathf.RoundToInt(temp.x);
                temp.y = Mathf.RoundToInt(temp.y);
                temp.z = Mathf.RoundToInt(temp.z);
                return temp;
            }
        }
        return Vector3.zero;
    }

    public bool CheckPlacements(Transform[] placementPoints)
    {
        if (placementPoints == null || placementPoints.Length == 0)
            return false;

        bool hasBuildableHit = false;

        foreach (Transform point in placementPoints)
        {
            Vector3 pos = point.position;

            if (Physics.OverlapSphere(pos, 0.5f, currentMachine.blockingLayers).Length > 0)
                return false;

            if (Physics.OverlapSphere(pos, 0.5f, currentMachine.buildableLayers).Length > 0)
                hasBuildableHit = true;
        }

        if (!hasBuildableHit)
            return false;

        ValidRotation = transform.rotation;
        return true;
    }

    public void previewBuild(Vector3 startPosition, Vector3 forwardVec)
    {
        if (!inPreview || previewGO == null)
            return;

        bool valid = CheckPlacements(placementPoints);

        if (valid != IsValidPlacement)
        {
            IsValidPlacement = valid;
            locationValidityChange?.Invoke(valid);
        }

        previewGO.transform.position = calculatePreviewPosition(startPosition, forwardVec);
    }

    public void rotateButtonPressed()
    {
        if (!inPreview || previewGO == null)
            return;

        Vector3 rot = previewGO.transform.eulerAngles;
        previewGO.transform.rotation = Quaternion.Euler(rot.x, rot.y + 45f, rot.z);
    }

    public void CancelButtonPressed()
    {
        if (previewGO)
            Destroy(previewGO);
        inPreview = false;
        previewGO = null;
        currentMachine = null;
    }

    public bool startPreview()
    {
        TryGetComponent(out InventoryHandler handler);
        if (!currentMachine.cost.CanBeCrafted(handler.coins, handler.timber, handler.iron, handler.stone))
            return false;

        previewGO = Instantiate(currentMachine.preview.gameObject);
        placementPoints = previewGO.GetComponent<MachineBehaviour>().placementPoints;
        int i = 0;
        foreach (Transform child in previewGO.transform)
        {
            placementPoints[i] = child;
            i++;
        }
        inPreview = true;
        return true;
    }
}