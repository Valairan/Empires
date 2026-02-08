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
    public int fireRate;                     // Shots per minute (or per second if you prefer)
    public int magSize;                      // Magazine size
    public int pelletCount;                  // For shotguns, number of pellets per shot
    public float accuracy = 1f;              // Optional: 0-1 scale, higher = more accurate
    public float recoil = 1f;                // Optional: 0-1 scale, higher = more recoil
    public float range = 50f;                // Optional: range in meters
    public bool isThrowable = false;
    public bool hasAreaEffect = false;
    public bool isAutomatic = true;
    public bool isStackable = false;
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
    void Awake()
    {
        stack = false;
    }

    public override void OnPickup(ItemPickupContext ctx)
    {
        ctx.inventory.EquipWeapon(this, ctx.inworld);
    }

    public override void OnBuy(ItemPickupContext ctx)
    {
        //InventoryHandler handler = NetworkManager.Singleton.ConnectedClients[parentID].PlayerObject.GetComponent<InventoryHandler>();
        //player.BuyItem(this);
    }


}

public enum WeaponType
{
    melee,
    sidearm,
    rifle,
    throwable,

}