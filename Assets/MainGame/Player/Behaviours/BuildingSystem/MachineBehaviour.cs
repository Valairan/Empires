using UnityEngine;

public class MachineBehaviour : ItemBehaviour<Machine>, IRaycastResponder
{
    public Health health;
    public Item respondToRaycast()
    {
        return baseitem;
    }
}
