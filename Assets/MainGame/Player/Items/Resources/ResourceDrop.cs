using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Drop", menuName = "Items/New Resource Drop")]
public class ResourceDrop : Item
{
    public GameObject drop;
    public ResourceDropType type;
    public override void OnPickup(InventoryHandler player, NetworkBehaviour inworld)
    {
        inworld.NetworkObject.Despawn(true);
        switch (type)
        {
            case ResourceDropType.Coin: player.coins++; break;
            case ResourceDropType.Stone: player.coins++; break;
            case ResourceDropType.Iron: player.coins++; break;
            case ResourceDropType.Timber: player.coins++; break;
        }
    }
}

public enum ResourceDropType
{
    Coin,
    Stone,
    Iron,
    Timber,
}
