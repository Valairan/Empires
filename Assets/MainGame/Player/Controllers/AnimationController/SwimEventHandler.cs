using UnityEngine;

public class SwimEventHandler : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.transform.parent.TryGetComponent(out PlayerController player)) return;
        player.playerAnimationController.meleeRig.weight = 0f;
        player.playerAnimationController.rifleRig.weight = 0f;
        player.playerAnimationController.pistolRig.weight = 0f;
        player.playerAnimationController.throwableRig.weight = 0f;
        player.playerAnimationController.leftHandRig.weight = 1f;

    }
}
