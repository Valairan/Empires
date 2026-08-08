using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class InventoryHandler : NetworkBehaviour, IInventory
{

    public WeaponStorageSlot[] weaponStorage;
    [SerializeField]
    public NetworkVariable<int> currentWeaponIndex = new NetworkVariable<int>(-1);
    public int coins, timber, iron, stone;

    public bool HasWeaponEquipped => currentWeaponIndex.Value >= 0;
    [SerializeField] public NetworkParentCentre networkObjectRoot;
    [SerializeField] public NetworkParent inWorldStorageForDiscarding; //attach the inworld prefab to this and set active to false, enable and unparent for discarding
    [SerializeField] public NetworkParent handParent; //equpped weapon parent
    [SerializeField] public Transform positionOffsetConstraint;
    [SerializeField] public NetworkParent meleeStorageParent;
    [SerializeField] public NetworkParent primaryStorageParent;
    [SerializeField] public NetworkParent sideArmStorageParent;

    public List<Item> storage;

    public void init()
    {
        weaponStorage = new WeaponStorageSlot[3];
        currentWeaponIndex.OnValueChanged += OnWeaponChangedOnServer;
    }


    public void PickupWeapon(Weapon weapon, NetworkBehaviour inworld)
    {
        if (!IsServer) return;

        NetworkObject nettemp = Instantiate(weapon.weaponPrefab_onplayer, inWorldStorageForDiscarding.transform.position, quaternion.identity).GetComponent<NetworkObject>();
        nettemp.Spawn();
        nettemp.ChangeOwnership(OwnerClientId);
        NetworkObjectReference netref = nettemp;

        int index = ((int)weapon.WeaponType == 3) ? 0 : (int)weapon.WeaponType;
        Debug.Log(index);
        if (weaponStorage[index] != null)
        {
            Debug.Log(weaponStorage[index]);
            dropWeapon(index);
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
        setWeaponToStoredPosition(weaponStorage.Length - 1);
        StashWeaponOnAll_ClientRpc(netref);
        toggleInWorldWeaponOnAllPlayers_ClientRpc(inworld.NetworkObject);
        ClientRpcParams rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };
        updateUIOnPickup_ClientRpc(currentWeaponIndex.Value, rpcParams);

    }

    [ClientRpc]
    private void updateUIOnPickup_ClientRpc(int current, ClientRpcParams clientRpcParams = default)
    {
        UiController.Singleton.updateInventoryDisplay(current);
    }


    [ServerRpc]
    public void NextWeapon_ServerRpc()
    {
        if (weaponStorage.Length == 0) return;
        int nextIndex = currentWeaponIndex.Value - 1;
        if (nextIndex < 0)
            nextIndex = weaponStorage.Length - 1;

        stashCurrentWeapon();
        EquipWeapon(nextIndex);
    }

    [ServerRpc]
    public void PreviousWeapon_ServerRpc()
    {
        if (weaponStorage.Length == 0) return;
        int prevIndex = currentWeaponIndex.Value + 1;
        if (prevIndex >= weaponStorage.Length)
            prevIndex = 0;

        stashCurrentWeapon();
        EquipWeapon(prevIndex);
    }



    public void dropWeapon(int weaponIndex)
    {
        if (weaponIndex >= weaponStorage.Length || weaponIndex < 0) return;
        if (weaponStorage.Length < 1) return;
        weaponStorage[weaponIndex].onplayer_instance.GetComponent<WeaponBehaviour>().NetworkObject.Despawn(true);
        weaponStorage[weaponIndex].rb_instance.GetComponent<NetworkObject>().TrySetParent((Transform)null);
        weaponStorage[weaponIndex].rb_instance.transform.position = inWorldStorageForDiscarding.transform.position;
        toggleInWorldWeaponOnAllPlayers_ClientRpc((NetworkObjectReference)weaponStorage[weaponIndex].rb_instance.GetComponent<NetworkObject>());
        RemoveFromInventory_ClientRpc((int)weaponStorage[weaponIndex].weapon.WeaponType, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });
        weaponStorage[weaponIndex] = null;
        currentWeaponIndex.Value = weaponIndex == currentWeaponIndex.Value ? -1 : currentWeaponIndex.Value;
        ClientRpcParams rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };
        updateUIOnPickup_ClientRpc(weaponIndex, rpcParams);

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
        if (currentWeaponIndex.Value == index) return;
        stashCurrentWeapon();
        EquipWeapon(index);

    }
    [ClientRpc]
    public void EquipWeaponOnAll_ClientRpc(NetworkObjectReference weapon)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[weapon.NetworkObjectId];
        if (networkObject == null) return;
        if (!networkObject.TryGetComponent(out WeaponBehaviour weaponBehaviour)) return;
        networkObject.transform.localPosition = weaponBehaviour.baseitem.position;
        networkObject.transform.localRotation = Quaternion.Euler(weaponBehaviour.baseitem.rotation);
        networkObject.transform.localScale = weaponBehaviour.baseitem.scale;

    }
    public void EquipWeapon(int index)
    {
        //if (!IsServer) return;
        if (index < 0 || index >= weaponStorage.Length) return;
        if (!weaponStorage[index]) return;
        if (!networkObjectRoot.TryToParentNetworkObject((NetworkObjectReference)weaponStorage[index].onplayer_instance, handParent)) return;
        setWeaponToEquippedPosition(index);
        EquipWeaponOnAll_ClientRpc((NetworkObjectReference)weaponStorage[index].onplayer_instance);
        currentWeaponIndex.Value = index;
    }

    void setWeaponToEquippedPosition(int index)
    {
        positionOffsetConstraint.localPosition = weaponStorage[index].weapon.forwardOffset;

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
        stashCurrentWeapon();

    }
    [ClientRpc]
    public void StashWeaponOnAll_ClientRpc(NetworkObjectReference weapon)
    {
        NetworkObject networkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[weapon.NetworkObjectId];
        if (networkObject == null) return;
        if (!networkObject.TryGetComponent(out WeaponBehaviour weaponBehaviour)) return;
        networkObject.transform.localPosition = weaponBehaviour.baseitem.storedPosition;
        networkObject.transform.localRotation = Quaternion.Euler(weaponBehaviour.baseitem.storedRotation);
        networkObject.transform.localScale = weaponBehaviour.baseitem.storedScale;
    }
    public void stashCurrentWeapon()
    {
        if (weaponStorage.Length < 1) return;
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
        StashWeaponOnAll_ClientRpc((NetworkObjectReference)weaponStorage[temp].onplayer_instance);
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
        if (previous >= 0)
        {
            if (weaponStorage[previous].onplayer_behaviour.TryGetComponent(out WeaponBehaviour wb))
                wb.store();
        }



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

        if (type >= 0 && type < 3)
            weaponStorage[type] = null;
    }
    public void AddToInventory(WeaponStorageSlot slot)
    {
        if (!IsServer) return;
        int index = ((int)slot.weapon.WeaponType == 3) ? 0 : ((int)slot.weapon.WeaponType == 4) ? 3 : (int)slot.weapon.WeaponType;
        Debug.Log(index);
        if (index >= weaponStorage.Length || index < 0) return;

        weaponStorage[index] = slot;

        AddToInventoryOn_ClientRpc(slot.rb_instance.GetComponent<NetworkObject>(), slot.onplayer_instance.GetComponent<NetworkObject>(), new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { this.NetworkObject.OwnerClientId } } });
    }
    [ClientRpc]
    public void AddToInventoryOn_ClientRpc(NetworkObjectReference inworld, NetworkObjectReference onplayer, ClientRpcParams _ = default)
    {
        if (IsHost) return;
        NetworkObject op = NetworkManager.Singleton.SpawnManager.SpawnedObjects[onplayer.NetworkObjectId];
        NetworkObject rb = NetworkManager.Singleton.SpawnManager.SpawnedObjects[inworld.NetworkObjectId];

        op.TryGetComponent(out WeaponBehaviour wb);
        int index = ((int)wb.baseitem.WeaponType == 4) ? 1 : ((int)wb.baseitem.WeaponType == 5) ? 4 : (int)wb.baseitem.WeaponType;

        weaponStorage[index] = new WeaponStorageSlot(wb.baseitem, rb.gameObject, op.gameObject);

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

    public static implicit operator bool(WeaponStorageSlot slot)
    {
        return slot.weapon != null;
    }
}
