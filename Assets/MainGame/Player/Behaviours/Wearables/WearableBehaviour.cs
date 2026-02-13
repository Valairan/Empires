using UnityEngine;

public class WearableBehaviour : ItemBehaviour<Wearable>, IRaycastResponder, IDamageable
{
    public Item respondToRaycast()
    {
        return baseitem;
    }



    public void takeDamage(DamageContext ctx)
    {
        throw new System.NotImplementedException();
    }
}
