using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

public partial class BuildHandler : MonoBehaviour, IInteractable, IRaycastResponder
{
    [Header("Available Machines")]
    public Machine[] allAvailableMachines;
    Machine currentMachine;

    public bool IsValidPlacement { get; private set; }
    public Quaternion ValidRotation { get; private set; }

    public float InteractionDuration => throw new NotImplementedException();

    public List<MachineBehaviour> ThingsIveBuilt = new();

    public Action<bool> locationValidityChange;

    public void setCurrentMachine(Machine machine)
    {
        currentMachine = machine;
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

    [ServerRpc]
    bool Place_ServerRpc(Vector3 pos, Quaternion rot)
    {
        if (!IsValidPlacement) return false;

        TryGetComponent(out InventoryHandler handler);
        if (!currentMachine.cost.CanBeCrafted(handler.coins, handler.timber, handler.iron, handler.stone))
            return false;

        currentMachine.cost.SubtractFromInventory(ref handler.coins, ref handler.timber, ref handler.iron, ref handler.stone);

        GameObject placedBuildableGO = Instantiate(currentMachine.machinePrefab.gameObject);
        placedBuildableGO.transform.SetPositionAndRotation(
            new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z)),
            rot);

        if (!placedBuildableGO.GetComponent<NetworkObject>().IsSpawned)
            placedBuildableGO.GetComponent<NetworkObject>().Spawn();
        ThingsIveBuilt.Add(placedBuildableGO.GetComponent<MachineBehaviour>());
        return true;
    }

    public Item Interact(ulong interactor)
    {
        throw new NotImplementedException();
    }

    public Item respondToRaycast(ulong interactor)
    {
        throw new NotImplementedException();
    }
}