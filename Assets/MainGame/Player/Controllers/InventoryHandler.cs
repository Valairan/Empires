using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryHandler : NetworkBehaviour, IInventory
{

    public List<WeaponStorageSlot> weaponStorage = new();
    [SerializeField]
    public int currentWeaponIndex = -1;
    public int coins, timber, iron, stone;

    public Action InventoryUpdated;
    public bool HasWeaponEquipped => currentWeaponIndex >= 0;
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


    public void PickupWeapon(Weapon weapon, NetworkBehaviour inworld)
    {
        if (!IsServer) return;
        NetworkObject nettemp = Instantiate(weapon.weaponPrefab_onplayer, inWorldStorageForDiscarding.transform.position, quaternion.identity).GetComponent<NetworkObject>();
        nettemp.Spawn();
        nettemp.ChangeOwnership(OwnerClientId);
        NetworkObjectReference netref = nettemp;

        int index = 0;
        foreach (WeaponStorageSlot item in weaponStorage)
        {
            if (item.weapon.WeaponType == weapon.WeaponType)
            {
                dropWeapon(index);
                break;
            }
            index++;
        }

        WeaponStorageSlot slot = new WeaponStorageSlot(weapon, inworld.gameObject, nettemp.gameObject);

        AddToInventory(slot);

        switch (weapon.WeaponType)
        {
            case WeaponType.melee:
                {
                    networkObjectRoot.TryToParentNetworkObject(netref, meleeStorageParent);
                    break;
                }
            case WeaponType.rifle:
                {
                    networkObjectRoot.TryToParentNetworkObject(netref, primaryStorageParent);
                    break;
                }
            case WeaponType.sidearm:
                {
                    networkObjectRoot.TryToParentNetworkObject(netref, sideArmStorageParent);
                    break;
                }
        }
        inworld.NetworkObject.TrySetParent(transform);
        inworld.transform.position = Vector3.zero;
        StashWeaponOnAllPlayersVisually_ClientRpc(slot.onplayer_instance);
        toggleInWorldWeaponOnAllPlayers_ClientRpc(inworld.NetworkObject);
        updateWeaponOn_ClientRpc(currentWeaponIndex, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });

    }
    [ServerRpc]
    public void NextWeapon_ServerRpc()
    {

        int nextIndex = currentWeaponIndex - 1;

        if (nextIndex < 0)
            nextIndex = weaponStorage.Count - 1;

        StashCurrentWeapon();
        EquipWeapon(nextIndex);
        currentWeaponIndex = nextIndex;
        updateWeaponOn_ClientRpc(nextIndex, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });

    }

    [ServerRpc]
    public void PreviousWeapon_ServerRpc()
    {

        int prevIndex = currentWeaponIndex + 1;

        if (prevIndex > weaponStorage.Count)
            prevIndex = 0;

        StashCurrentWeapon();
        EquipWeapon(prevIndex);
        currentWeaponIndex = prevIndex;
        updateWeaponOn_ClientRpc(prevIndex, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });

    }


    public void DropCurrentWeapon()
    {
        if (currentWeaponIndex < 0) return;
        DropWeapon_ServerRpc(currentWeaponIndex);
    }
    public void dropWeapon(int weaponIndex)
    {
        if (weaponIndex >= weaponStorage.Count || weaponIndex < 0) return;
        if (weaponStorage.Count < 1) return;
        weaponStorage[weaponIndex].onplayer_instance.GetComponent<WeaponBehaviour>().NetworkObject.Despawn(true);
        weaponStorage[weaponIndex].rb_instance.GetComponent<NetworkObject>().TrySetParent((Transform)null);
        weaponStorage[weaponIndex].rb_instance.transform.position = inWorldStorageForDiscarding.transform.position;
        toggleInWorldWeaponOnAllPlayers_ClientRpc((NetworkObjectReference)weaponStorage[weaponIndex].rb_instance.GetComponent<NetworkObject>());
        weaponStorage.RemoveAt(weaponIndex);
    }

    [ServerRpc]
    public void DropWeapon_ServerRpc(int weaponIndex)
    {
        int temp = (int)weaponStorage[weaponIndex].weapon.WeaponType;
        dropWeapon(weaponIndex);
        RemoveFromInventory_ClientRpc(temp, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });
        currentWeaponIndex = -1;
        updateWeaponOn_ClientRpc(currentWeaponIndex, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });
    }

    [ServerRpc]
    public void EquipWeapon_ServerRpc(int index)
    {
        StashCurrentWeapon();
        EquipWeapon(index);
        updateWeaponOn_ClientRpc(currentWeaponIndex, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });

    }
    public void EquipWeapon(int index)
    {
        //if (!IsServer) return;
        if (index < 0 || index >= weaponStorage.Count) return;
        networkObjectRoot.TryToParentNetworkObject((NetworkObjectReference)weaponStorage[index].onplayer_instance, handParent);
        equipWeaponOnAllPlayersVisually_ClientRpc((NetworkObjectReference)weaponStorage[index].onplayer_instance);
        //StashCurrentWeapon();
    }
    [ClientRpc]
    public void equipWeaponOnAllPlayersVisually_ClientRpc(NetworkObjectReference networkObjectReference)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectReference.NetworkObjectId];
        networkObject.TryGetComponent(out WeaponBehaviour wb);
        if (wb) wb.equip(networkObject.OwnerClientId);
    }


    [ServerRpc]
    public void StashCurrentWeapon_ServerRpc()
    {
        if (currentWeaponIndex < 0) return;

        StashCurrentWeapon();
        currentWeaponIndex = -1;

        updateWeaponOn_ClientRpc(currentWeaponIndex, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { OwnerClientId }
            }
        });
    }
    public void StashCurrentWeapon()
    {
        if (weaponStorage.Count < 1) return;
        if (currentWeaponIndex < 0) return;
        Debug.Log($"{currentWeaponIndex} is index");
        switch (weaponStorage[currentWeaponIndex].weapon.WeaponType)
        {
            case WeaponType.melee:
                {
                    networkObjectRoot.TryToParentNetworkObject(weaponStorage[currentWeaponIndex].onplayer_instance, meleeStorageParent);
                    break;
                }
            case WeaponType.rifle:
                {
                    networkObjectRoot.TryToParentNetworkObject(weaponStorage[currentWeaponIndex].onplayer_instance, primaryStorageParent);
                    break;
                }
            case WeaponType.sidearm:
                {
                    networkObjectRoot.TryToParentNetworkObject(weaponStorage[currentWeaponIndex].onplayer_instance, sideArmStorageParent);
                    break;
                }
        }
        StashWeaponOnAllPlayersVisually_ClientRpc(weaponStorage[currentWeaponIndex].onplayer_instance);

    }


    [ClientRpc]
    public void StashWeaponOnAllPlayersVisually_ClientRpc(NetworkObjectReference networkObjectReference)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectReference.NetworkObjectId];
        networkObject.TryGetComponent(out WeaponBehaviour wb);
        if (wb) wb.store();
    }

    [ClientRpc]
    public void toggleInWorldWeaponOnAllPlayers_ClientRpc(NetworkObjectReference networkObjectReference)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectReference.NetworkObjectId];
        networkObject.TryGetComponent(out WeaponBehaviour wb);
        if (wb) wb.gameObject.SetActive(!wb.gameObject.activeSelf);
    }


    [ClientRpc]
    public void RemoveFromInventory_ClientRpc(int type, ClientRpcParams _ = default)
    {
        if (IsHost) return;
        int index = weaponStorage.FindIndex(
            x => (int)x.weapon.WeaponType == type
        );

        if (index >= 0)
            weaponStorage.RemoveAt(index);
    }
    public void AddToInventory(WeaponStorageSlot slot)
    {
        if (!IsServer) return;
        weaponStorage.Add(slot);

        AddToInventoryOn_ClientRpc(slot.rb_instance.GetComponent<NetworkObject>(), slot.onplayer_instance.GetComponent<NetworkObject>(), new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });
    }
    [ClientRpc]
    public void AddToInventoryOn_ClientRpc(NetworkObjectReference inworld, NetworkObjectReference onplayer, ClientRpcParams _ = default)
    {
        if (IsHost) return;
        NetworkObject op = NetworkManager.Singleton.SpawnManager.SpawnedObjects[onplayer.NetworkObjectId];
        NetworkObject rb = NetworkManager.Singleton.SpawnManager.SpawnedObjects[inworld.NetworkObjectId];

        op.TryGetComponent(out WeaponBehaviour wb);

        weaponStorage.Add(new WeaponStorageSlot(wb.baseitem, rb.gameObject, op.gameObject));

    }

    [ClientRpc]
    public void updateWeaponOn_ClientRpc(int currentWeaponIndex, ClientRpcParams _ = default)
    {
        this.currentWeaponIndex = currentWeaponIndex;
        InventoryUpdated?.Invoke();
    }
    public void BuyItem(Weapon weapon)
    {
    }

    public void AddItem()
    {
        throw new NotImplementedException();
    }
}

[Serializable]
public class WeaponStorageSlot
{
    public Weapon weapon;
    public GameObject rb_instance;
    public GameObject onplayer_instance;
    public WeaponBehaviour onplayer_behaviour => onplayer_instance.GetComponent<WeaponBehaviour>();

    public WeaponStorageSlot(Weapon weapon, GameObject rb_instance, GameObject onplayer_instance)
    {
        this.weapon = weapon;
        this.rb_instance = rb_instance;
        this.onplayer_instance = onplayer_instance;
    }
}