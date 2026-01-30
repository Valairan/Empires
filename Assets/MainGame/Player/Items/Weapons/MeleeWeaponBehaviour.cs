using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeWeaponBehaviour : WeaponBehaviour, IRaycastResponder, IInteractable
{
    public float InteractionDuration => 1f;

    void OnTriggerEnter(Collider collision)
    {
        IDamageable damageable = collision.gameObject.GetComponentInParent<IDamageable>();
        if (damageable != null)
            damageable.takeDamage((Weapon)baseitem);
    }

}
