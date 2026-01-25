using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/New Item")]
public class Item : ScriptableObject
{
    public Sprite ItemIcon;
    public String ItemName;
    public String ItemDescription;
    public bool stack;
    public ItemType Type;
    [Header("Manufacturing cost")]
    public ManufacturingCost cost;
    [Header("While attached to player")]
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

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
    melee,
    primary,
    sidearm,
}

[Serializable]
public struct ManufacturingCost
{
    public BaseResource[] itemsNeededToManufacture;
    public int[] howManyItemsNeededToManufacture;
}