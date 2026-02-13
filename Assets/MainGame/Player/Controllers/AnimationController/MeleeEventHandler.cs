using UnityEngine;

public class MeleeEventHandler : StateMachineBehaviour
{
    CombatController controller;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = animator.GetComponent<CombatController>();
        animator.SetBool("Attacking", true);

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > 0.35f)
            controller.OnAttackAnimationStarted();
        if (stateInfo.normalizedTime > 0.70f)
            controller.OnAttackAnimationFinished();

    }
    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex
    )
    {
        animator.SetBool("Attacking", false);
        if (controller != null)
        {
            controller.OnAttackAnimationFinished();
        }
    }
}
