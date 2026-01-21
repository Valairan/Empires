using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/New Item")]
public class Item : ScriptableObject
{
    public String ItemName;
    public String ItemDescription;
    public bool stack;
    public ItemType Type;

}

[Serializable]
public struct Stats
{
    public int shieldTotal;
    public int healthOnGround;
}

[Serializable]
public struct Loot
{
    public GameObject[] loot;
}

public enum ItemType
{
    resource,
    melee,
    primary,
    sidearm,
}