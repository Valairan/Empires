using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public partial class GameManager : NetworkBehaviour
{
    // A registry to keep track of all placed buildings by owner
    public Dictionary<ulong, List<MachineBehaviour>> PlayerBuildingsRegistry = new();

    public void RegisterBuilding(ulong ownerClientId, MachineBehaviour building)
    {
        if (!PlayerBuildingsRegistry.ContainsKey(ownerClientId))
        {
            PlayerBuildingsRegistry[ownerClientId] = new List<MachineBehaviour>();
        }
        PlayerBuildingsRegistry[ownerClientId].Add(building);
    }

    public bool CanPlayerInteract(ulong clientId, MachineBehaviour building)
    {
        // Logic: Allow interaction if it's not private, or if the client is the owner
        //if (!building.machineData.isPrivate) return true;
        
        return building.ownerClientId.Value == clientId;
    }
}
