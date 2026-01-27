using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MeleeWeaponBehaviour : WeaponBehaviour, IRaycastResponder, IInteractable
{
    public float InteractionDuration => 1f;

    bool canAttack = true;

    public override void OnAttackAnimationFinished()
    {
        canAttack = true;
    }
    public override void OnAttackAnimationStarted()
    {
        canAttack = false;
    }

    
}
