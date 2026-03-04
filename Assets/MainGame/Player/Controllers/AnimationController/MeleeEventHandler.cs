using System;
using UnityEngine;

public class MeleeEventHandler : StateMachineBehaviour
{
    CombatController controller;
    public float startAttackTime;
    public float endAttackTime;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = animator.transform.parent.GetComponent<CombatController>();
        animator.SetBool("Attacking", true);

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.normalizedTime > startAttackTime)
            controller.OnAttackAnimationStarted();
        if (stateInfo.normalizedTime > endAttackTime)
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
