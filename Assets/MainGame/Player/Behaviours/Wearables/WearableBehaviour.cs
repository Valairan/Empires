using UnityEngine;

public class WearableBehaviour : ItemBehaviour, IRaycastResponder, IDamageable
{
    public Item respondToRaycast()
    {
        return baseitem;
    }



    public void takeDamage(Weapon damager)
    {
        throw new System.NotImplementedException();
    }
}
