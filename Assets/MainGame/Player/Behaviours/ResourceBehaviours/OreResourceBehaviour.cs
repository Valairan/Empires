using System;
using System.Data.Common;
using Unity.Netcode;
using UnityEngine;

public class OreResourceBehaviour : BaseResourceBehaviour
{
    [SerializeField] ParticleSystem impact;
    public override void takeDamage(DamageContext ctx)
    {
        float damage = ctx.damager.treeDamage;
        ResourcesManager.Singleton.DamageOre_ServerRpc(damage, ctx.damager.WeaponType, ctx.hitpoint, ctx.hitnormal, xCoordinate, yCoordinate);
    }
    public override void playEffect(Vector3 hitpoint, Vector3 hitnormal, WeaponType type)
    {
        impact.transform.position = hitpoint;
        impact.transform.rotation = Quaternion.Euler(hitnormal);
        impact.Play();
    }

}
