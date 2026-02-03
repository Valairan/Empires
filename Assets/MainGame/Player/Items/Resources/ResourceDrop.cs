using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu(fileName = "New Drop", menuName = "Items/New Resource Drop")]
public class ResourceDrop : Item
{
    public GameObject drop;
    public ResourceDropType type;
    public override void OnPickup(ulong parentID, ulong objectID)
    {
        InventoryHandler handler = NetworkManager.Singleton.ConnectedClients[parentID].PlayerObject.GetComponent<InventoryHandler>();
        switch (type)
        {
            case ResourceDropType.Coin: handler.coins++; break;
            case ResourceDropType.Stone: handler.coins++; break;
            case ResourceDropType.Iron: handler.coins++; break;
            case ResourceDropType.Timber: handler.coins++; break;
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
