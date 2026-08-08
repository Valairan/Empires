using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.Build.Content;
using UnityEngine;

[RequireComponent(typeof(InventoryHandler))]
public partial class BuildHandler : NetworkBehaviour, IInteractable, IRaycastResponder
{
    private Machine currentMachine;

    [Header("Building Settings")]

    bool IsValidPlacement
    {
        get { return isValidPlacement; }
        set
        {
            if (value && !isValidPlacement) onBuildvalidityChange?.Invoke(value);

            isValidPlacement = value;
        }
    }

    public IBuildContext _buildContext;
    public IBuildDatabaseContext _buildDatabaseContext;


    bool isValidPlacement;
    public Quaternion ValidRotation { get; private set; } = Quaternion.identity;

    public float InteractionDuration => 0f;

    private InventoryHandler inventoryHandler;

    public void Init(IBuildContext ctx, IBuildDatabaseContext dbctx)
    {
        _buildContext = ctx;
        _buildDatabaseContext = dbctx;
    }

    private void Awake()
    {
        inventoryHandler = GetComponent<InventoryHandler>();
    }

    public void setCurrentMachine(Machine machine)
    {
        currentMachine = machine;
    }

    public void buildButtonPressed(Vector3 rayOrigin, Vector3 rayDirection)
    {
        if (!IsOwner) return;

        // Nothing selected -> open menu
        if (!inPreview && currentMachine == null)
        {
            UiController.Singleton.toggleBuildMenu();
            return;
        }

        // Start preview
        if (!inPreview)
        {
            startPreview();
            UiController.Singleton.toggleBuildMenu();
            return;
        }

        // Try place
        TryPlaceBuilding();
    }

    private void TryPlaceBuilding()
    {
        if (previewGO == null || !IsValidPlacement || currentMachine == null)
            return;

        if (Vector3.Distance(transform.position, previewGO.transform.position) > maxBuildDistance)
            return;

        string machineId = currentMachine.ItemId;
        if (machineId.Length < 1) return;
        Place_ServerRpc(machineId, previewGO.transform.position, ValidRotation);

        CancelButtonPressed();
    }

    [ServerRpc]
    private void Place_ServerRpc(string machineId, Vector3 pos, Quaternion rot)
    {
        if (machineId.Length < 1) return;
        GameObject targetMachinePrefab = _buildDatabaseContext.GetPrefab(machineId);

        BuildableBehaviour targetBehaviour = targetMachinePrefab.GetComponent<BuildableBehaviour>();

        if (targetBehaviour == null || targetMachinePrefab == null)
            return;

        if (Vector3.Distance(transform.position, pos) > maxBuildDistance + 1f)
            return;

        if (!targetBehaviour.baseitem.cost.CanBeCrafted(inventoryHandler.coins, inventoryHandler.timber, inventoryHandler.iron, inventoryHandler.stone))
            return;

        // Authoritative grid collision check on Server
        Vector3 snappedPos = snapToGrid(pos);
        if (Physics.OverlapSphere(snappedPos, 0.4f, targetBehaviour.baseitem.blockingLayers).Length > 0)
            return;

        targetBehaviour.baseitem.cost.SubtractFromInventory(ref inventoryHandler.coins, ref inventoryHandler.timber, ref inventoryHandler.iron, ref inventoryHandler.stone);

        GameObject placedBuildableGO = Instantiate(targetBehaviour.baseitem.machinePrefab.gameObject, snappedPos, rot);

        if (placedBuildableGO.TryGetComponent(out NetworkObject netObj))
        {
            netObj.SpawnWithOwnership(OwnerClientId);
        }

        targetBehaviour.interactWithMe += UiController.Singleton.toggleWeaponSelector;

    }

    public Item Interact(ulong interactor) => throw new NotImplementedException();
    public Item respondToRaycast(ulong interactor) => throw new NotImplementedException();
}