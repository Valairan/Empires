using UnityEngine;

public class WearableBehaviour : ItemBehaviour<Wearable>, IRaycastResponder, IDamageable
{
    public Item respondToRaycast(ulong interactor)
    {
        return baseitem;
    }



    public void takeDamage(DamageContext ctx)
    {
        throw new System.NotImplementedException();
    }
}
