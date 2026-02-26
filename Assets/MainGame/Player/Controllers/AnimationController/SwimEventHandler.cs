using UnityEngine;

public class SwimEventHandler : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.transform.parent.TryGetComponent(out PlayerController player)) return;
        player.playerAnimationController.meleeRigWeight = 0f;
        player.playerAnimationController.rifleRigWeight = 0f;
        player.playerAnimationController.pistolRigWeight = 0f;
        player.playerAnimationController.throwableRigWeight = 0f;
        player.playerAnimationController.leftHandRigWeight = 0f;
        player.playerAnimationController.leftHandConstraintWeight = 0f;
    }
}
