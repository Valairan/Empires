using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public partial class GameManager : NetworkBehaviour, IBuildContext, IBuildDatabaseContext
{
    [Header("Available Machines")]
    public List<Machine> allAvailableMachinesList;
    public Dictionary<string, Machine> allAvailableMachines;
    private Dictionary<ulong, List<ulong>> registry = new();

    public void buildMachineDatabase()
    {
        foreach (Machine m in allAvailableMachinesList)
        {
            allAvailableMachines.Add(m.ItemId, m);
        }
    }
    public GameObject GetPrefab(string prefabId)
    {
        return allAvailableMachines[prefabId].machinePrefab;
    }
    public int GetDatabaseCount()
    {
        return allAvailableMachines.Count;
    }

    public bool RegisterBuilding(ulong interactor, ulong building)
    {
        if (!registry.ContainsKey(interactor))
        {
            return false;
        }
        registry[interactor].Add(building);
        return true;
    }

    public bool ValidateOwnership(ulong interactor, ulong building)
    {
        if (!registry.ContainsKey(interactor))
        {
            return false;
        }
        if (registry[interactor].Contains(building)) return true; else return false;
    }
    public bool CanPlaceStructure(ulong interactor, ulong building)
    {
        if (!registry.ContainsKey(interactor))
        {
            return false;
        }
        if (registry[interactor].Contains(building)) return true; else return false;
    }

    public bool CanInteractWithStructure(ulong interactor, ulong building)
    {
        if (!registry.ContainsKey(interactor))
        {
            return false;
        }
        if (registry[interactor].Contains(building)) return true; else return false;
    }

    public bool ChangeOwnership(ulong source, ulong destination, ulong item)
    {
        if (!(registry.ContainsKey(source) || registry.ContainsKey(destination)))
        {
            return false;
        }
        if (!registry[source].Remove(item)) return false;
        registry[destination].Add(item);
        return true;
    }
}
