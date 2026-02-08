using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Drop", menuName = "Items/New Resource Drop")]
public class ResourceDrop : Item
{
    public GameObject drop;
    public ResourceDropType type;
    public override void OnPickup(ItemPickupContext ctx)
    {
        ctx.inventory.AddItem();
    }
}

public enum ResourceDropType
{
    Coin,
    Stone,
    Iron,
    Timber,
}
