using UnityEngine;

public class RifleEventHandler : StateMachineBehaviour
{
    CombatController controller;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        controller = animator.GetComponent<CombatController>();
        animator.SetBool("Attacking", true);

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {


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
