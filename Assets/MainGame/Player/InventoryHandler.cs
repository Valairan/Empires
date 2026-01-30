using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InventoryHandler : NetworkBehaviour
{
    public Item current;
    public Item primary;
    public Item sidearm;
    public Item melee;

    public int coins;
    public int timber;
    public int iron;
    public int stone;
    public WeaponBehaviour currentBehaviour;
    public WeaponBehaviour primaryBehaviour;
    public WeaponBehaviour sidearmBehaviour;
    public WeaponBehaviour meleeBehaviour;
    [SerializeField] public Transform equipped;
    [SerializeField] public Transform meleeStorage;
    [SerializeField] public Transform primaryStorage;
    [SerializeField] public Transform sideArmStorage;
    [SerializeField] public GameObject currentGO;
    [SerializeField] public GameObject primaryGO;
    [SerializeField] public GameObject sidearamGO;
    [SerializeField] public GameObject meleeGO;
    [SerializeField] public NetworkParentCentre parentGameObject;
    [SerializeField] public NetworkParent handGameObject;

    public List<Item> storage;


    public void init()
    {
        handGameObject.NetworkParentId = "Hand";
    }
    public void drop()
    {

    }

    public void equip()
    {

    }


    public void EquipItem(Item item)
    {
        if (!IsLocalPlayer) return;
        //EquipItem_ServerRpc(item);
    }


    [ServerRpc]
    public void EquipItem_ServerRpc()
    {


    }


    [ServerRpc]
    public void PickUpItem_ServerRpc(ulong itemGO, ulong clientID)
    {
        if (!IsServer) return;
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(itemGO, out NetworkObject obj))
            return;
        if (!(Vector3.Distance(NetworkGamePropertiesStorage.Singleton.spawnedPlayers[clientID].transform.position, obj.transform.position) < 2f))
            return;

        Weapon item = (Weapon)obj.GetComponent<ItemBehaviour>().baseitem;

        GameObject temp = Instantiate(item.weaponPrefab_onplayer);
        NetworkObject nettemp = temp.GetComponent<NetworkObject>();
        currentGO = temp;
        nettemp.Spawn();
        NetworkObjectReference netref = nettemp;

        parentGameObject.TryToParentNetworkObject(netref, "Hand");
        temp.transform.localScale = item.scale;
        temp.transform.localPosition = item.position;
        temp.transform.localRotation = Quaternion.Euler(item.rotation);
        obj.Despawn();
    }

    [ClientRpc]
    public void setTransformsWhenPickingUp_ClientRpc()
    {

    }

}
