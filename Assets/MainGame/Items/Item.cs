using System;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Empires/Items/New Item")]
public class Item : ItemBase
{
    public Sprite ItemIcon;
    public String ItemName;
    public String ItemDescription;
    public bool stack;
    public ItemType Type;
    [Header("Manufacturing cost")]
    public ManufacturingCost cost;


    private void OnValidate()
    {

    }

    public override void OnPickup(ItemPickupContext context)
    {
        throw new NotImplementedException();
    }

    public override void OnBuy(ItemPickupContext context)
    {
        throw new NotImplementedException();
    }
}



[Serializable]
public struct Stats
{
    public int shieldTotal;
    public int healthOnGround;
}


public enum ItemType
{
    resource,
    machine,
    weapon
}

