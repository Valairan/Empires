using UnityEngine;

public class MachineBehaviour : ItemBehaviour<Machine>, IRaycastResponder
{
    public Health health;
    [SerializeField] Transform[] placementPoints;

    public Item respondToRaycast()
    {
        return baseitem;
    }


    
}
