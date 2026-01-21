using UnityEngine;

public class BaseResourceBehaviour : ItemBehaviour, IDamageable, IRaycastResponder
{
    public int xCoordinate;
    public int yCoordinate;
    public Health health;

    public virtual Item respondToRaycast()
    {
        return baseitem;
    }

    public virtual void takeDamage(Weapon damager)
    {

    }

    public virtual void dropItems()
    {

    }
    public virtual void deactivateSelf()
    {

    }
}
