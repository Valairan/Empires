using Unity.Netcode;
using UnityEditor.Build.Content;
using UnityEngine;

public class MachineBehaviour : ItemBehaviour<Machine>, IRaycastResponder
{
    public Health health;
    public MachineState state;
    public Transform[] placementPoints;
    public NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>(ulong.MaxValue);

    public virtual Item respondToRaycast(ulong interactor)
    {
        
        return baseitem;
    }



}

public enum MachineState
{
    preview,
    placed
}

