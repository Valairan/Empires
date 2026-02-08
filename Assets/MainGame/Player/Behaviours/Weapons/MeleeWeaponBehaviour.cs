using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeWeaponBehaviour : WeaponBehaviour
{

    void OnTriggerEnter(Collider collision)
    {
        if (!isAttacking) return;
        IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();
        if (damageable != null)
            damageable.takeDamage((Weapon)baseitem);
    }

}
