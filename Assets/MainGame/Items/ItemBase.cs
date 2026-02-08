using UnityEngine;

public abstract class ItemBase : ScriptableObject
{
    public abstract void OnPickup(ItemPickupContext context);
    public abstract void OnBuy(ItemPickupContext context);
}