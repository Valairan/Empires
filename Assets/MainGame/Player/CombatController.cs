using UnityEngine;

public class CombatController : MonoBehaviour
{
    public bool isAttacking;

    public virtual void OnAttackAnimationFinished()
    {
        isAttacking = false;
    }
    public virtual void OnAttackAnimationStarted()
    {
        isAttacking = true;
    }
    public void Attack()
    {

    }
}
