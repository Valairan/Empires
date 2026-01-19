using Unity.VisualScripting;
using UnityEngine;

public class MeleeWeaponBehaviour : WeaponBehaviour, IRaycastResponder, IInteractable
{

    public Item interact(GameObject interactor)
    {
        if (interactor.TryGetComponent(out PlayerController interactingPlayer))
        {
            interactingPlayer.melee = baseitem;
            interactingPlayer.meleeGO = this.gameObject;
            this.gameObject.transform.SetParent(interactingPlayer.transform);
            this.gameObject.SetActive(false);
        }
        return baseitem;
    }

    public Item respondToRaycast()
    {
        return baseitem;
    }

}
