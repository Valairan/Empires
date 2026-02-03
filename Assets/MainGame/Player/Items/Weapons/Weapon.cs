using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Base Weapon", menuName = "Items/Weapons/New Weapon")]
public class Weapon : Item
{
    public float treeDamage;
    public float oreDamage;
    public float playerDamage;
    public GameObject weaponPrefab_rb;
    public GameObject weaponPrefab_onplayer;
    [Header("While attached to player")]
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;
    public WeaponType WeaponType;
    void Awake()
    {
        stack = false;
    }

    public override void OnPickup(ulong parentID, ulong objectID)
    {
        Debug.Log("Something before onpickup");
        InventoryHandler handler = NetworkManager.Singleton.ConnectedClients[parentID].PlayerObject.GetComponent<InventoryHandler>();
        handler.current = this;
        handler.EquipItem(parentID, objectID, this);
        Debug.Log("Something after onpicup");

    }

    public override void OnBuy(ulong parentID, ulong objectID)
    {
        InventoryHandler handler = NetworkManager.Singleton.ConnectedClients[parentID].PlayerObject.GetComponent<InventoryHandler>();
        handler.BuyItem(this);
    }
}

public enum WeaponType
{
    melee,
    sidearm,
    rifle,

}