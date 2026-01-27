using UnityEngine;

public class MeleeEventHandler : StateMachineBehaviour
{
    WeaponBehaviour weapon;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        weapon = animator.GetComponent<PlayerController>().currentBehaviour;
        weapon.OnAttackAnimationStarted();

    }
    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex
    )
    {
        if (weapon != null)
        {
            weapon.OnAttackAnimationFinished();
        }
    }
}
