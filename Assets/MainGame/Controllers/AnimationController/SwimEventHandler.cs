using UnityEngine;

public class SwimEventHandler : StateMachineBehaviour
{
    float cache_meleeRigWeight;
    float cache_rifleRigWeight;
    float cache_pistolRigWeight;
    float cache_throwableRigWeight;
    float cache_leftHandRigWeight;
    float cache_leftHandConstraintWeight;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.transform.parent.TryGetComponent(out PlayerController player)) return;
        cache_meleeRigWeight = player.playerAnimationController.meleeRigWeight;
        cache_rifleRigWeight = player.playerAnimationController.rifleRigWeight;
        cache_pistolRigWeight = player.playerAnimationController.pistolRigWeight;
        cache_throwableRigWeight = player.playerAnimationController.throwableRigWeight;
        cache_leftHandRigWeight = player.playerAnimationController.leftHandRigWeight;
        cache_leftHandConstraintWeight = player.playerAnimationController.leftHandConstraintWeight;

        player.playerAnimationController.meleeRigWeight = 0f;
        player.playerAnimationController.rifleRigWeight = 0f;
        player.playerAnimationController.pistolRigWeight = 0f;
        player.playerAnimationController.throwableRigWeight = 0f;
        player.playerAnimationController.leftHandRigWeight = 0f;
        player.playerAnimationController.leftHandConstraintWeight = 0f;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.transform.parent.TryGetComponent(out PlayerController player)) return;
        player.playerAnimationController.meleeRigWeight = cache_meleeRigWeight;
        player.playerAnimationController.rifleRigWeight = cache_rifleRigWeight;
        player.playerAnimationController.pistolRigWeight = cache_pistolRigWeight;
        player.playerAnimationController.throwableRigWeight = cache_throwableRigWeight;
        player.playerAnimationController.leftHandRigWeight = cache_leftHandRigWeight;
        player.playerAnimationController.leftHandConstraintWeight = cache_leftHandConstraintWeight;


        cache_meleeRigWeight = 0f;
        cache_rifleRigWeight = 0f;
        cache_pistolRigWeight = 0f;
        cache_throwableRigWeight = 0f;
        cache_leftHandRigWeight = 0f;
        cache_leftHandConstraintWeight = 0f;
    }
}
