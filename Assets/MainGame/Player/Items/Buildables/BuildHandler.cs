using UnityEngine;
using Unity.Netcode;
using System;

public class BuildHandler : MonoBehaviour
{
    [Header("Available Machines")]
    public Machine[] allAvailableMachines;

    Machine currentMachine;
    GameObject previewGO;
    bool inPreview;
    public bool IsValidPlacement { get; private set; }
    public Quaternion ValidRotation { get; private set; }

    public Action<bool> locationValidityChange;
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
                return temp;
            }
        }
        return Vector3.zero;
    }
    public bool checkPlacements(Transform[] placementPoints)
    {
        if (!(placementPoints.Length > 0)) return false;
        foreach (Transform placementpoint in placementPoints)
        {
            if (Physics.OverlapSphere(placementpoint.position, .5f, currentMachine.blockingLayers).Length > 0)
                return false;
        }
        ValidRotation = transform.rotation;
        return true;
    }
    public void setCurrentMachine(Machine machine)
    {
        currentMachine = machine;
    }

    public void previewBuild(Vector3 startPosition, Vector3 forwardVec)
    {
        if (!inPreview || previewGO == null)
            return;
        if (!inPreview) return;

        bool valid = checkPlacements(placementPoints);

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
        previewGO.transform.rotation =
            Quaternion.Euler(rot.x, rot.y + 45f, rot.z);
    }
    public void CancelButtonPressed()
    {
        if (previewGO)
            Destroy(previewGO);
        inPreview = false;
        previewGO = null;
        currentMachine = null;

    }


    public void buildButtonPressed()
    {
        // Nothing selected → open menu
        if (!inPreview && currentMachine == null)
        {
            UiController.Singleton.toggleBuildMenu();
            return;
        }

        // Start preview
        if (!inPreview)
        {
            startPreview();
            return;
        }

        // Try place
        TryPlaceBuilding();
    }

    public void startPreview()
    {
        previewGO = Instantiate(currentMachine.preview.gameObject);
        placementPoints = new Transform[previewGO.transform.childCount];
        int i = 0;
        foreach (Transform child in previewGO.transform)
        {
            placementPoints[i] = child;
            i++;
        }
        inPreview = true;
    }

    void TryPlaceBuilding()
    {
        if (previewGO == null)
            return;

        if (!IsValidPlacement)
            return;

        if (!Place_ServerRpc(previewGO.transform.position, previewGO.transform.rotation)) return;

        Destroy(previewGO);

        previewGO = null;
        inPreview = false;
        currentMachine = null;
    }

    public void CancelPreview()
    {
        if (!inPreview)
            return;

        Destroy(previewGO);

        previewGO = null;
        inPreview = false;
    }

    [ServerRpc]
    bool Place_ServerRpc(Vector3 pos, Quaternion rot)
    {
        if (!IsValidPlacement) return false;
        GameObject placedBuildableGO = Instantiate(currentMachine.machinePrefab.gameObject);
        placedBuildableGO.transform.SetPositionAndRotation(new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z)), rot);

        if (!placedBuildableGO.GetComponent<NetworkObject>().IsSpawned)
            placedBuildableGO.GetComponent<NetworkObject>().Spawn();
        return true;
    }
}
