
using UnityEngine;
public class TreeResourceBehaviour : BaseResourceBehaviour
{
    [SerializeField] ParticleSystem woodImpact;

    public override void takeDamage(DamageContext ctx)
    {
        Debug.Log("The tree is being hit");
        float damage = ctx.damager.treeDamage;
        ResourcesManager.Singleton.DamageTree_ServerRpc(damage, ctx.damager.WeaponType, ctx.hitpoint, ctx.hitnormal, xCoordinate, yCoordinate);
    }


    public override void playEffect(Vector3 hitpoint, Vector3 hitnormal, WeaponType type)
    {
        woodImpact.transform.position = hitpoint;
        woodImpact.transform.rotation = Quaternion.LookRotation(hitnormal);
        woodImpact.Play();
    }

}
