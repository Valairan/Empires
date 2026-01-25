using UnityEngine;

public class MachineBehaviour : ItemBehaviour, IRaycastResponder
{
    public Health health;
    public Item respondToRaycast()
    {
        return baseitem;
    }
}
