using UnityEngine;

public class CombatController : MonoBehaviour
{
    public bool isAttacking;

    public WeaponBehaviour currentWeapon;
    public virtual void OnAttackAnimationFinished()
    {
        currentWeapon.isAttacking = false;
    }
    public virtual void OnAttackAnimationStarted()
    {
        currentWeapon.isAttacking = true;
    }
    public void setCurrentWeapon(Weapon wepaon)
    {

    }
    public void Attack()
    {
        switch (currentWeapon.baseWeapon.WeaponType)
        {
            case WeaponType.melee: return;
            case WeaponType.sidearm:
            
                break;
            case WeaponType.rifle:
                break;
            default: break;
        }
    }
}

