using System.Collections.Concurrent;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryHandler : NetworkBehaviour
{
    public Weapon current;
    public Weapon primary;
    public Weapon sidearm;
    public Weapon melee;

    public int coins;
    public int timber;
    public int iron;
    public int stone;

    [SerializeField] public Transform meleeStorage;
    [SerializeField] public Transform primaryStorage;
    [SerializeField] public Transform sideArmStorage;
    [SerializeField] public GameObject currentGO;
    [SerializeField] public GameObject primaryGO;
    [SerializeField] public GameObject sidearamGO;
    [SerializeField] public GameObject meleeGO;
    [SerializeField] public NetworkParentCentre networkObjectRoot;
    [SerializeField] public NetworkParent handGameObject;

    public List<Item> storage;


    public void init()
    {

    }
    public void drop()
    {

    }

    public void equip()
    {

    }


    public void EquipItem(ulong parentID, ulong objectID, Weapon weapon)
    {
        if (!IsServer) return;
        GameObject temp = Instantiate(weapon.weaponPrefab_onplayer);
        NetworkObject nettemp = temp.GetComponent<NetworkObject>();
        nettemp.Spawn();
        nettemp.ChangeOwnership(parentID);
        NetworkObjectReference netref = nettemp;
        if (networkObjectRoot.TryToParentNetworkObject(netref, handGameObject)) ;
        NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectID].Despawn(true);
        updateWeaponOn_ClientRpc(nettemp.NetworkObjectId, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });
    }


    [ClientRpc]
    public void updateWeaponOn_ClientRpc(ulong updatedweaponid, ClientRpcParams _ = default)
    {
        TryGetComponent(out PlayerController controller);

        WeaponBehaviour currentBehaviour = NetworkManager.Singleton.SpawnManager.SpawnedObjects[updatedweaponid].GetComponent<WeaponBehaviour>();
        current = currentBehaviour.baseWeapon;
        currentGO = NetworkManager.Singleton.SpawnManager.SpawnedObjects[updatedweaponid].gameObject;
        controller.playerCombatController.currentWeapon = currentGO.GetComponent<WeaponBehaviour>();
        controller.playerAnimationController.setTarget(currentBehaviour.ik_target);
        controller.onWeaponChanged.Invoke(current);
    }
    public void BuyItem(Weapon weapon)
    {
    }





    [ServerRpc]
    public void EquipItem_ServerRpc(ulong itemGO, ulong clientID)
    {

    }


}
