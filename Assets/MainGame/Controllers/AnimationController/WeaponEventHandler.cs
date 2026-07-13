using UnityEngine;

public class WeaponEventHandler : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.transform.parent.TryGetComponent(out PlayerController player)) return;
        switch (player.playerCombatController.currentWeapon.baseitem.WeaponType)
        {

            case WeaponType.melee:
                {
                    player.playerAnimationController.meleeRigWeight = 0f;
                    player.playerAnimationController.rifleRigWeight = 0f;
                    player.playerAnimationController.pistolRigWeight = 0f;
                    player.playerAnimationController.throwableRigWeight = 0f;
                    player.playerAnimationController.leftHandConstraintWeight = 0f;
                    player.playerAnimationController.leftHandRigWeight = 0f;
                    break;
                }
            case WeaponType.rifle:
                {
                    player.playerAnimationController.meleeRigWeight = 0f;
                    player.playerAnimationController.rifleRigWeight = 1f;
                    player.playerAnimationController.pistolRigWeight = 0f;
                    player.playerAnimationController.throwableRigWeight = 0f;
                    player.playerAnimationController.leftHandConstraintWeight = 1f;
                    player.playerAnimationController.leftHandRigWeight = 1f;
                    break;
                }
            case WeaponType.sidearm:
                {
                    player.playerAnimationController.meleeRigWeight = 0f;
                    player.playerAnimationController.rifleRigWeight = 0f;
                    player.playerAnimationController.pistolRigWeight = 1f;
                    player.playerAnimationController.throwableRigWeight = 0f;
                    player.playerAnimationController.leftHandConstraintWeight = 1f;
                    player.playerAnimationController.leftHandRigWeight = 1f;
                    break;
                }
            case WeaponType.throwable:
                {
                    player.playerAnimationController.meleeRigWeight = 0f;
                    player.playerAnimationController.rifleRigWeight = 0f;
                    player.playerAnimationController.pistolRigWeight = 0f;
                    player.playerAnimationController.throwableRigWeight = 1f;
                    player.playerAnimationController.leftHandConstraintWeight = 0f;
                    player.playerAnimationController.leftHandRigWeight = 0f;
                    break;
                }
        }
    }

    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }


}
