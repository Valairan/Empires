using UnityEngine;

public class BaseResource : ItemBehaviour, IDamageable, IRaycastResponder
{
    public int xCoordinate;
    public int yCoordinate;

    public virtual Item respondToRaycast()
    {
        return baseitem;
    }

    public virtual void takeDamage(float damage)
    {

    }

    public virtual void dropItems()
    {

    }
    public virtual void deactivateSelf()
    {

    }
}
