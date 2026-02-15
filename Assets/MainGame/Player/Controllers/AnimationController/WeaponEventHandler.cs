using UnityEngine;

public class WeaponEventHandler : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.transform.parent.TryGetComponent(out PlayerController player)) return;
        switch (player.playerCombatController.currentWeapon.baseitem.WeaponType)
        {
            case WeaponType.melee: player.playerAnimationController.meleeRig.weight = 1f; break;
            case WeaponType.rifle: player.playerAnimationController.rifleRig.weight = 1f; break;
            case WeaponType.sidearm: player.playerAnimationController.pistolRig.weight = 1f; break;
            case WeaponType.throwable: player.playerAnimationController.throwableRig.weight = 1f; break;


        }
        player.playerAnimationController.leftHandRig.weight = 1f;
    }

    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }


}
