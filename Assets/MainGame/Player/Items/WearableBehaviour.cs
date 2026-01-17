using UnityEngine;

public class WearableBehaviour : ItemBehaviour, IRaycastResponder, IDamageable
{
    public Item respondToRaycast()
    {
        return baseitem;
    }

    public void takeDamage(float damage)
    {
        throw new System.NotImplementedException();
    }
}
