using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/New Item")]
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

[Serializable]
public struct ManufacturingCost : IEquatable<ManufacturingCost>
{
    public ResourceDrop[] itemsNeededToManufacture;
    public int[] howManyItemsNeededToManufacture;

    public bool IsValid =>
        itemsNeededToManufacture != null &&
        howManyItemsNeededToManufacture != null &&
        itemsNeededToManufacture.Length == howManyItemsNeededToManufacture.Length;

    private int TotalQuantity =>
        howManyItemsNeededToManufacture?.Sum() ?? 0;

    public bool Equals(ManufacturingCost other)
    {
        if (!IsValid || !other.IsValid)
            return false;

        if (itemsNeededToManufacture.Length != other.itemsNeededToManufacture.Length)
            return false;

        for (int i = 0; i < itemsNeededToManufacture.Length; i++)
        {
            if (itemsNeededToManufacture[i] != other.itemsNeededToManufacture[i])
                return false;

            if (howManyItemsNeededToManufacture[i] != other.howManyItemsNeededToManufacture[i])
                return false;
        }

        return true;
    }
    public bool CanBeCrafted(int coinsAvailable, int timberAvailable, int ironAvailable, int stoneAvailable)
    {
        if (!IsValid) return false;

        for (int i = 0; i < itemsNeededToManufacture.Length; i++)
        {
            int required = howManyItemsNeededToManufacture[i];
            switch (itemsNeededToManufacture[i].type)
            {
                case ResourceDropType.Coin:
                    if (coinsAvailable < required) return false;
                    break;
                case ResourceDropType.Timber:
                    if (timberAvailable < required) return false;
                    break;
                case ResourceDropType.Iron:
                    if (ironAvailable < required) return false;
                    break;
                case ResourceDropType.Stone:
                    if (stoneAvailable < required) return false;
                    break;
                default:
                    return false; // unknown resource
            }
        }

        return true;
    }
    public void SubtractFromInventory(ref int coins, ref int timber, ref int iron, ref int stone)
    {
        if (!IsValid) return;

        for (int i = 0; i < itemsNeededToManufacture.Length; i++)
        {
            int amount = howManyItemsNeededToManufacture[i];
            switch (itemsNeededToManufacture[i].type)
            {
                case ResourceDropType.Coin:
                    coins -= amount;
                    break;
                case ResourceDropType.Timber:
                    timber -= amount;
                    break;
                case ResourceDropType.Iron:
                    iron -= amount;
                    break;
                case ResourceDropType.Stone:
                    stone -= amount;
                    break;
            }
        }
    }
    public override bool Equals(object obj) =>
        obj is ManufacturingCost other && Equals(other);

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();

        if (IsValid)
        {
            for (int i = 0; i < itemsNeededToManufacture.Length; i++)
            {
                hash.Add(itemsNeededToManufacture[i]);
                hash.Add(howManyItemsNeededToManufacture[i]);
            }
        }

        return hash.ToHashCode();
    }

    public static bool operator ==(ManufacturingCost left, ManufacturingCost right) =>
        left.Equals(right);

    public static bool operator !=(ManufacturingCost left, ManufacturingCost right) =>
        !left.Equals(right);

    // --------------------
    // Ordering
    // --------------------

    public static bool operator <(ManufacturingCost left, ManufacturingCost right) =>
        left.TotalQuantity < right.TotalQuantity;

    public static bool operator >(ManufacturingCost left, ManufacturingCost right) =>
        left.TotalQuantity > right.TotalQuantity;
}

