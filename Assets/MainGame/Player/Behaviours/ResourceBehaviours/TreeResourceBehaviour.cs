
public class TreeResourceBehaviour : BaseResourceBehaviour
{
    public override void takeDamage(Weapon damager)
    {

        float damage = damager.treeDamage;
        ResourcesManager.Singleton.DamageTree_ServerRpc(damage, xCoordinate, yCoordinate);
    }

}
