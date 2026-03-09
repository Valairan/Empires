using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Base Weapon", menuName = "Items/Weapons/New Weapon")]
public class Weapon : Item
{
    public WeaponType WeaponType;
    public float treeDamage;
    public float oreDamage;
    public float headDamage;
    public float bodyDamage;
    public float legDamage;
    // Flags
    public bool canADS = false;
    public bool hasAreaEffect = false;
    public GameObject weaponPrefab_rb;
    public GameObject weaponPrefab_onplayer;
    [Header("Parented Position")]
    [Header("While attached to player")]
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale = Vector3.one;
    [Header("While stored on player")]
    public Vector3 storedPosition;
    public Vector3 storedRotation;
    public Vector3 storedScale = Vector3.one;
    protected virtual void Awake()
    {
        stack = false;
    }

    public override void OnPickup(ItemPickupContext ctx)
    {
        ctx.inventory.PickupWeapon(this, ctx.inworld);
    }

    public override void OnBuy(ItemPickupContext ctx)
    {
        //InventoryHandler handler = NetworkManager.Singleton.ConnectedClients[parentID].PlayerObject.GetComponent<InventoryHandler>();
        //player.BuyItem(this);
    }


}

public enum WeaponType
{
    rifle,
    sidearm,
    melee,
    throwable,

}