using UnityEngine;

public class RifleEventHandler : StateMachineBehaviour
{
    PlayerController player;
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!animator.transform.parent.TryGetComponent(out player)) return;
        animator.SetBool("Attacking", true);

    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {


    }
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Attacking", false);
        if (player != null)
        {
            player.playerCombatController.OnAttackAnimationFinished();
        }

    }
}
