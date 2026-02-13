using Unity.Netcode;
using UnityEngine;

public class BaseResourceBehaviour : ItemBehaviour<BaseResource>, IDamageable, IRaycastResponder
{
    public int xCoordinate;
    public int yCoordinate;
    public Health health;

    public virtual Item respondToRaycast()
    {
        return baseitem;
    }

    public virtual void takeDamage(DamageContext ctx)
    {

    }
    public virtual void playEffect(Vector3 hitpoint, Vector3 hitnormal, WeaponType type)
    {

    }

}
