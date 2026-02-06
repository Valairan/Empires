using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "Base Weapon", menuName = "Items/Weapons/New Weapon")]
public class Weapon : Item
{
    public WeaponType WeaponType;
    public float treeDamage;
    public float oreDamage;
    public float playerDamage;
    public GameObject weaponPrefab_rb;
    public GameObject weaponPrefab_onplayer;
    [Header("Scope In Settings")]
    public bool canAim;
    public Sprite ScopedInTexture;
    [Range(0, 1)]
    public float scopeZoom;
    [Header("While attached to player")]
    [Header("Parented Position")]
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    void Awake()
    {
        stack = false;
    }

    public override void OnPickup(InventoryHandler player, NetworkBehaviour inworld)
    {
        inworld.NetworkObject.Despawn(true);
        player.EquipWeapon(this, inworld);
    }

    public override void OnBuy(InventoryHandler player)
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

}