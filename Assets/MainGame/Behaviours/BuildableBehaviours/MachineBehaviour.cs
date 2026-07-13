using UnityEngine;

public class MachineBehaviour : ItemBehaviour<Machine>, IRaycastResponder
{
    public Health health;
    public MachineState state;
    public Transform[] placementPoints;

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
