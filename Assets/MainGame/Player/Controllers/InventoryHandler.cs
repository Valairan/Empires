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
    public NetworkVariable<int> currentWeaponIndex = new NetworkVariable<int>(-1);
    public int coins, timber, iron, stone;

    public bool HasWeaponEquipped => currentWeaponIndex.Value >= 0;
    [SerializeField] public NetworkParentCentre networkObjectRoot;
    [SerializeField] public NetworkParent inWorldStorageForDiscarding; //attach the inworld prefab to this and set active to false, enable and unparent for discarding
    [SerializeField] public NetworkParent handParent; //equpped weapon parent
    [SerializeField] public NetworkParent meleeStorageParent;
    [SerializeField] public NetworkParent primaryStorageParent;
    [SerializeField] public NetworkParent sideArmStorageParent;

    public List<Item> storage;

    public void init()
    {
        currentWeaponIndex.OnValueChanged += OnWeaponChangedOnServer;
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
                    networkObjectRoot.TryToParentNetworgkObject(netref, meleeStorageParent);
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
        toggleInWorldWeaponOnAllPlayers_ClientRpc(inworld.NetworkObject);

    }
    [ServerRpc]
    public void NextWeapon_ServerRpc()
    {
        if (weaponStorage.Count == 0) return;
        int nextIndex = currentWeaponIndex.Value - 1;
        if (nextIndex < 0)
            nextIndex = weaponStorage.Count - 1;

        StashCurrentWeapon();
        EquipWeapon(nextIndex);
    }

    [ServerRpc]
    public void PreviousWeapon_ServerRpc()
    {
        if (weaponStorage.Count == 0) return;
        int prevIndex = currentWeaponIndex.Value + 1;
        if (prevIndex >= weaponStorage.Count)
            prevIndex = 0;

        StashCurrentWeapon();
        EquipWeapon(prevIndex);
    }



    public void dropWeapon(int weaponIndex)
    {
        if (weaponIndex >= weaponStorage.Count || weaponIndex < 0) return;
        if (weaponStorage.Count < 1) return;
        weaponStorage[weaponIndex].onplayer_instance.GetComponent<WeaponBehaviour>().NetworkObject.Despawn(true);
        weaponStorage[weaponIndex].rb_instance.GetComponent<NetworkObject>().TrySetParent((Transform)null);
        weaponStorage[weaponIndex].rb_instance.transform.position = inWorldStorageForDiscarding.transform.position;
        toggleInWorldWeaponOnAllPlayers_ClientRpc((NetworkObjectReference)weaponStorage[weaponIndex].rb_instance.GetComponent<NetworkObject>());
        RemoveFromInventory_ClientRpc((int)weaponStorage[weaponIndex].weapon.WeaponType, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });
        weaponStorage.RemoveAt(weaponIndex);
        currentWeaponIndex.Value = weaponIndex == currentWeaponIndex.Value ? -1 : currentWeaponIndex.Value;

    }

    [ServerRpc]
    public void DropCurrentWeapon_ServerRpc()
    {
        dropWeapon(currentWeaponIndex.Value);
    }
    [ServerRpc]
    public void DropWeapon_ServerRpc(int weaponIndex)
    {
        dropWeapon(weaponIndex);
    }

    [ServerRpc]
    public void EquipWeapon_ServerRpc(int index)
    {
        StashCurrentWeapon();
        EquipWeapon(index);

    }
    [ClientRpc]
    public void EquipWeapon_ClientRpc(int index)
    {
        if (index < 0 || index >= weaponStorage.Count) return;
        weaponStorage[index].onplayer_instance.transform.localPosition = weaponStorage[index].weapon.position;
        weaponStorage[index].onplayer_instance.transform.localRotation = Quaternion.Euler(weaponStorage[index].weapon.rotation);
        weaponStorage[index].onplayer_instance.transform.localScale = weaponStorage[index].weapon.scale;
    }
    public void EquipWeapon(int index)
    {
        //if (!IsServer) return;
        if (index < 0 || index >= weaponStorage.Count) return;
        if (!networkObjectRoot.TryToParentNetworkObject((NetworkObjectReference)weaponStorage[index].onplayer_instance, handParent)) return;
        setWeaponToEquippedPosition(index);
        EquipWeapon_ClientRpc(index);
        currentWeaponIndex.Value = index;
    }

    void setWeaponToEquippedPosition(int index)
    {
        weaponStorage[index].onplayer_instance.transform.localPosition = weaponStorage[index].weapon.position;
        weaponStorage[index].onplayer_instance.transform.localRotation = Quaternion.Euler(weaponStorage[index].weapon.rotation);
        weaponStorage[index].onplayer_instance.transform.localScale = weaponStorage[index].weapon.scale;
    }
    void setWeaponToStoredPosition(int index)
    {
        weaponStorage[index].onplayer_instance.transform.localPosition = weaponStorage[index].weapon.storedPosition;
        weaponStorage[index].onplayer_instance.transform.localRotation = Quaternion.Euler(weaponStorage[index].weapon.storedRotation);
        weaponStorage[index].onplayer_instance.transform.localScale = weaponStorage[index].weapon.storedScale;
    }


    [ServerRpc]
    public void StashCurrentWeapon_ServerRpc()
    {
        if (currentWeaponIndex.Value < 0) return;

        StashCurrentWeapon();

    }
    public void StashCurrentWeapon()
    {
        if (weaponStorage.Count < 1) return;
        if (currentWeaponIndex.Value < 0) return;

        int temp = currentWeaponIndex.Value;

        switch (weaponStorage[temp].weapon.WeaponType)
        {
            case WeaponType.melee:
                {
                    networkObjectRoot.TryToParentNetworkObject(weaponStorage[temp].onplayer_instance, meleeStorageParent);
                    break;
                }
            case WeaponType.rifle:
                {
                    networkObjectRoot.TryToParentNetworkObject(weaponStorage[temp].onplayer_instance, primaryStorageParent);
                    break;
                }
            case WeaponType.sidearm:
                {
                    networkObjectRoot.TryToParentNetworkObject(weaponStorage[temp].onplayer_instance, sideArmStorageParent);
                    break;
                }
        }
        setWeaponToStoredPosition(temp);
        currentWeaponIndex.Value = -1;
    }
    void OnWeaponChangedOnServer(int previous, int current)
    {
        if (!IsServer) return;
        OnWeaponChanged_ClientRpc(previous, current);
    }
    [ClientRpc]
    void OnWeaponChanged_ClientRpc(int previous, int current)
    {
        if (weaponStorage[previous].onplayer_behaviour.TryGetComponent(out WeaponBehaviour wb))
            wb.store();



    }
    /*
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
    */

    [ClientRpc]
    public void toggleInWorldWeaponOnAllPlayers_ClientRpc(NetworkObjectReference networkObjectReference)
    {
        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(networkObjectReference.NetworkObjectId, out var networkObject))
            return;
        networkObject.TryGetComponent(out WeaponBehaviour wb);
        if (wb) wb.gameObject.SetActive(!wb.gameObject.activeSelf);
    }


    [ClientRpc]
    public void RemoveFromInventory_ClientRpc(int type, ClientRpcParams _ = default)
    {
        if (IsServer) return;
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