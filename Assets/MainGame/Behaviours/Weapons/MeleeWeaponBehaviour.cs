using Unity.Netcode;
using UnityEngine;

public class MeleeWeaponBehaviour : WeaponBehaviour<MeleeWeapon>, IWeaponTriggerable
{

    [ServerRpc]
    public override void Attack_ServerRpc(Vector3 point)
    {

    }

    public bool CanFire()
    {
        //throw new System.NotImplementedException();
        return false;
    }

    public void TriggerPressed(Vector3 aimPoint)
    {
        onAttack?.Invoke();
    }

    public void TriggerReleased()
    {
        //throw new System.NotImplementedException();
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
                damage = calculateDamage(),
                damagingPlayerID = NetworkManager.Singleton.LocalClientId,
                hitpoint = hitpoint,
                hitnormal = hitnormal
            };
            damageable.takeDamage(ctx);
        }
    }


    public float calculateDamage()
    {
        return 25f;
    }
}
