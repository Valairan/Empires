using UnityEngine;
using Unity.Netcode;

public class CombatController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public WeaponBehaviour currentWeapon;

    [HideInInspector] public Vector3 lookingAtPoint;

    private bool isAttacking;


    public void init()
    {
        cameraTransform = Camera.main.transform;
    }

    public Vector3 RaycastFromCamera()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, 500f))
        {
            lookingAtPoint = hit.point;
        }
        else
        {
            lookingAtPoint = cameraTransform.position + cameraTransform.forward * 500f;
        }

        return lookingAtPoint;
    }


    public void OnAttackDown()
    {
        isAttacking = true;

        if (currentWeapon is IWeaponTriggerable triggerable)
        {
            triggerable.TriggerPressed(lookingAtPoint);
        }
    }

    public void OnAttackUp()
    {
        isAttacking = false;

        if (currentWeapon is IWeaponTriggerable triggerable)
        {
            triggerable.TriggerReleased();
        }
    }
 
    public void UpdateWeapon()
    {
        if (currentWeapon == null) return;

        if (currentWeapon is IWeaponUpdatable updatable)
        {
            updatable.UpdateWeapon(lookingAtPoint);
        }
    }
 
    public virtual void OnAttackAnimationStarted()
    {
        if (currentWeapon != null)
            currentWeapon.isAttacking = true;
    }

    public virtual void OnAttackAnimationFinished()
    {
        if (currentWeapon != null)
            currentWeapon.isAttacking = false;
    }
}
