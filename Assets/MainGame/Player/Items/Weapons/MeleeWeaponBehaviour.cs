using Unity.VisualScripting;
using UnityEngine;

public class MeleeWeaponBehaviour : WeaponBehaviour, IRaycastResponder, IInteractable
{

    public Item interact(GameObject interactor)
    {
        if (!IsLocalPlayer) return null;
        if (interactor.TryGetComponent(out PlayerController interactingPlayer))
        {
            //  interactingPlayer.PickUpItem(this.gameObject);
        }
        return baseitem;
    }

    public Item respondToRaycast()
    {
        return baseitem;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.TryGetComponent(out IDamageable damageable))
        {
            damageable.takeDamage((MeleeWeapon)baseitem);
        }
    }
}
