using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryHandler : NetworkBehaviour, IInventory
{
    public Weapon current;
    public Weapon primary;
    public Weapon sidearm;
    public Weapon melee;

    public int coins, timber, iron, stone;


    [Tooltip("Stores which in world prefabs are currently on the player")]
    [SerializeField] public GameObject inworldPrimary_prefab, inworldSidearm_prefab, inworldMelee_prefab;
    [Tooltip("Stores which equipable prefabs are currently on the player")]
    [SerializeField] public GameObject currentGO, primaryGO, sidearamGO, meleeGO;
    [SerializeField] public NetworkParentCentre networkObjectRoot;
    [SerializeField] public NetworkParent inWorldStorageForDiscarding; //attach the inworld prefab to this and set active to false, enable and unparent for discarding
    [SerializeField] public NetworkParent handParent; //equpped weapon parent
    [SerializeField] public NetworkParent meleeStorageParent;
    [SerializeField] public NetworkParent primaryStorageParent;
    [SerializeField] public NetworkParent sideArmStorageParent;

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

    [ServerRpc]
    public void DropWeapon_ServerRpc(WeaponType type)
    {
        if (!IsServer) return;

        Weapon weaponToDrop = null;
        GameObject weaponGO = null;
        GameObject inWorldPrefab = null;

        switch (type)
        {
            case WeaponType.melee:
                weaponToDrop = melee;
                weaponGO = meleeGO;
                inWorldPrefab = inworldMelee_prefab;
                break;

            case WeaponType.rifle:
                weaponToDrop = primary;
                weaponGO = primaryGO;
                inWorldPrefab = inworldPrimary_prefab;
                break;

            case WeaponType.sidearm:
                weaponToDrop = sidearm;
                weaponGO = sidearamGO;
                inWorldPrefab = inworldSidearm_prefab;
                break;
        }

        if (weaponToDrop == null) return;

        // Despawn equipped weapon
        weaponGO.GetComponent<NetworkObject>().Despawn(true);

        // Move in-world version to drop location
        NetworkObject inWorldNetObj = inWorldPrefab.GetComponent<NetworkObject>();
        inWorldNetObj.TrySetParent((Transform)null);
        inWorldPrefab.transform.position = inWorldStorageForDiscarding.transform.position;

        toggleInWorldWeaponOnAllPlayers_ClientRpc(inWorldNetObj);

        // Clear references
        switch (type)
        {
            case WeaponType.melee:
                melee = null;
                meleeGO = null;
                break;
            case WeaponType.rifle:
                primary = null;
                primaryGO = null;
                break;
            case WeaponType.sidearm:
                sidearm = null;
                sidearamGO = null;
                break;
        }
    }


    public void EquipWeapon(Weapon weapon, NetworkBehaviour inworld)
    {
        if (!IsServer) return;
        NetworkObject nettemp = Instantiate(weapon.weaponPrefab_onplayer).GetComponent<NetworkObject>();
        nettemp.Spawn();
        nettemp.ChangeOwnership(OwnerClientId);
        NetworkObjectReference netref = nettemp;

        switch (weapon.WeaponType)
        {
            case WeaponType.melee:
                if (melee)
                {
                    meleeGO.GetComponent<WeaponBehaviour>().NetworkObject.Despawn(true);
                    inworldMelee_prefab.GetComponent<NetworkObject>().TrySetParent((Transform)null);
                    inworldMelee_prefab.transform.position = inWorldStorageForDiscarding.transform.position;
                    toggleInWorldWeaponOnAllPlayers_ClientRpc(inworldMelee_prefab);

                }
                inworldMelee_prefab = inworld.gameObject;
                meleeGO = nettemp.gameObject;
                melee = weapon;
                break;
            case WeaponType.rifle:
                if (primary)
                {
                    primaryGO.GetComponent<WeaponBehaviour>().NetworkObject.Despawn(true);
                    inworldPrimary_prefab.GetComponent<NetworkObject>().TrySetParent((Transform)null);
                    inworldPrimary_prefab.transform.position = inWorldStorageForDiscarding.transform.position;
                    toggleInWorldWeaponOnAllPlayers_ClientRpc(inworldPrimary_prefab);
                }
                inworldPrimary_prefab = inworld.gameObject;
                primaryGO = nettemp.gameObject;
                primary = weapon;
                break;
            case WeaponType.sidearm:
                if (sidearm)
                {
                    sidearamGO.GetComponent<WeaponBehaviour>().NetworkObject.Despawn(true);
                    inworldSidearm_prefab.GetComponent<NetworkObject>().TrySetParent((Transform)null);
                    inworldSidearm_prefab.transform.position = inWorldStorageForDiscarding.transform.position;
                    toggleInWorldWeaponOnAllPlayers_ClientRpc(inworldSidearm_prefab);
                }
                inworldSidearm_prefab = inworld.gameObject;
                sidearamGO = nettemp.gameObject;
                sidearm = weapon;
                break;
        }

        if (current)
        {
            if (current.WeaponType != weapon.WeaponType)
            {
                NetworkObjectReference currentRef = currentGO.GetComponent<NetworkObject>();
                switch (current.WeaponType)
                {
                    case WeaponType.melee:
                        {
                            networkObjectRoot.TryToParentNetworkObject(currentRef, meleeStorageParent);
                            break;
                        }
                    case WeaponType.rifle:
                        {
                            networkObjectRoot.TryToParentNetworkObject(currentRef, primaryStorageParent);
                            break;
                        }
                    case WeaponType.sidearm:
                        {
                            networkObjectRoot.TryToParentNetworkObject(currentRef, sideArmStorageParent);
                            break;
                        }
                }

                storeWeaponOnAllPlayers_ClientRpc(currentRef);
            }
        }
        toggleInWorldWeaponOnAllPlayers_ClientRpc(inworld.NetworkObject);
        inworld.NetworkObject.TrySetParent(transform);
        inworld.transform.position = Vector3.zero;

        current = weapon;
        currentGO = nettemp.gameObject;

        networkObjectRoot.TryToParentNetworkObject(netref, handParent);
        equipWeaponOnAllPlayers_ClientRpc(netref);
        updateWeaponOn_ClientRpc(nettemp.NetworkObjectId, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });
    }

    [ClientRpc]
    public void storeWeaponOnAllPlayers_ClientRpc(NetworkObjectReference networkObjectReference)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectReference.NetworkObjectId];
        networkObject.TryGetComponent(out WeaponBehaviour wb);
        if (wb) wb.store();
    }
    [ClientRpc]
    public void equipWeaponOnAllPlayers_ClientRpc(NetworkObjectReference networkObjectReference)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectReference.NetworkObjectId];
        networkObject.TryGetComponent(out WeaponBehaviour wb);
        if (wb) wb.equip(networkObject.OwnerClientId);
    }
    [ClientRpc]
    public void toggleInWorldWeaponOnAllPlayers_ClientRpc(NetworkObjectReference networkObjectReference)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectReference.NetworkObjectId];
        networkObject.TryGetComponent(out WeaponBehaviour wb);
        if (wb) wb.gameObject.SetActive(!wb.gameObject.activeSelf);
    }


    [ClientRpc]
    public void updateWeaponOn_ClientRpc(ulong updatedweaponid, ClientRpcParams _ = default)
    {
        WeaponBehaviour incomingBehaviour = NetworkManager.Singleton.SpawnManager.SpawnedObjects[updatedweaponid].GetComponent<WeaponBehaviour>();
        current = incomingBehaviour.baseitem;
        currentGO = NetworkManager.Singleton.SpawnManager.SpawnedObjects[updatedweaponid].gameObject;

        switch (current.WeaponType)
        {
            case WeaponType.melee:
                {
                    meleeGO = currentGO;
                    melee = current;
                    break;
                }
            case WeaponType.rifle:
                {
                    primaryGO = currentGO;
                    primary = current;
                    break;
                }
            case WeaponType.sidearm:
                {
                    sidearamGO = currentGO;
                    sidearm = current;
                    break;
                }
        }
        TryGetComponent(out PlayerController controller);
        controller.playerCombatController.currentWeapon = currentGO.GetComponent<WeaponBehaviour>();
        controller.playerAnimationController.setTarget(incomingBehaviour.ik_target);
        controller.onWeaponChanged.Invoke(current);
    }
    public void BuyItem(Weapon weapon)
    {
    }

    [ClientRpc]
    public void activateInWorldPrefab_ClientRpc()
    {

    }


    [ServerRpc]
    public void EquipItem_ServerRpc(ulong itemGO, ulong clientID)
    {

    }

    public void AddItem()
    {
        throw new System.NotImplementedException();
    }
}
