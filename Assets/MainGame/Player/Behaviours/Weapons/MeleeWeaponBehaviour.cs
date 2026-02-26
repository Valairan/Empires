using Unity.Netcode;
using UnityEngine;

public class MeleeWeaponBehaviour : WeaponBehaviour<MeleeWeapon>
{

    [ServerRpc]
    public override void Attack_ServerRpc(Vector3 point)
    {

    }

    void OnTriggerEnter(Collider collision)
    {
        if (!isAttacking) return;
        IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            Vector3 hitpoint = collision.ClosestPoint(transform.position);
            Vector3 hitnormal = (transform.position - hitpoint).normalized;
            DamageContext ctx = new DamageContext
            {
                damager = baseitem,
                damagingPlayerID = NetworkManager.Singleton.LocalClientId,
                hitpoint = hitpoint,
                hitnormal = hitnormal
            };
            damageable.takeDamage(ctx);
        }
    }

}
