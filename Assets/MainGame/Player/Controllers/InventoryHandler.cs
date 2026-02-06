using System.Collections.Generic;
using Unity.Netcode;
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

    [SerializeField] public NetworkParent meleeStorage;
    [SerializeField] public NetworkParent primaryStorage;
    [SerializeField] public NetworkParent sideArmStorage;
    [SerializeField] public NetworkParent inworldStorage;
    [SerializeField] public GameObject inworld_prefab;
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

    public void WearItem()
    {

    }

    public void EquipWeapon(Weapon weapon, NetworkBehaviour inworld)
    {
        if (!IsServer) return;
        GameObject temp = Instantiate(weapon.weaponPrefab_onplayer);
        NetworkObject nettemp = temp.GetComponent<NetworkObject>();
        nettemp.Spawn();
        nettemp.ChangeOwnership(OwnerClientId);
        NetworkObjectReference netref = nettemp;
        if (current)
        {
            NetworkObject inhandref = currentGO.GetComponent<NetworkBehaviour>().NetworkObject;
            if (current.WeaponType == weapon.WeaponType)
            {
                Vector3 discardPosition = transform.position;
                discardPosition.y += 1;
                inworld_prefab.GetComponent<WeaponBehaviour>().NetworkObject.TrySetParent((Transform)null);
                inworld_prefab.transform.position = discardPosition;
            }
            else
            {
                switch (current.WeaponType)
                {
                    case WeaponType.melee: networkObjectRoot.TryToParentNetworkObject(inhandref, meleeStorage); break;
                    case WeaponType.sidearm: networkObjectRoot.TryToParentNetworkObject(inhandref, sideArmStorage); break;
                    case WeaponType.rifle: networkObjectRoot.TryToParentNetworkObject(inhandref, primaryStorage); break;
                }
            }
        }
        if (!networkObjectRoot.TryToParentNetworkObject(netref, inworldStorage)) { return; }
        if (!networkObjectRoot.TryToParentNetworkObject(netref, handGameObject)) { return; }
        updateWeaponOn_ClientRpc(nettemp.NetworkObjectId, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });
    }


    [ClientRpc]
    public void updateWeaponOn_ClientRpc(ulong updatedweaponid, ClientRpcParams _ = default)
    {
        TryGetComponent(out PlayerController controller);
        WeaponBehaviour currentBehaviour = NetworkManager.Singleton.SpawnManager.SpawnedObjects[updatedweaponid].GetComponent<WeaponBehaviour>();
        if (current.WeaponType == currentBehaviour.baseWeapon.WeaponType)
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
