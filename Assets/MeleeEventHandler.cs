using UnityEngine;

public class MeleeEventHandler : StateMachineBehaviour
{
    CombatController controller;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = animator.GetComponent<CombatController>();
        controller.OnAttackAnimationStarted();

    }
    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex
    )
    {
        if (controller != null)
        {
            controller.OnAttackAnimationFinished();
        }
    }
}
