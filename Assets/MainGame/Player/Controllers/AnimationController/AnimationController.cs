
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class AnimationController : MonoBehaviour
{
    public Animator animator;
    public Transform lookTargetTransform;
    public Transform leftHandGrabTransform;
    public Transform leftHandHintTransform;
    public RigBuilder parentRig;
    public float parentRigWeight;
    public Rig meleeRig;
    public float meleeRigWeight;
    public Rig rifleRig;
    public float rifleRigWeight;
    public TwoBoneIKConstraint leftHandConstraint;
    public float leftHandConstraintWeight;
    public Rig leftHandRig;
    public float leftHandRigWeight;
    public Rig pistolRig;
    public float pistolRigWeight;
    public Rig throwableRig;
    public float throwableRigWeight;
    public float lerpFactor;


    public void updateAnimationParams(Vector2 movement, bool grounded, bool inwater)
    {
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Submerged", inwater);
        if (inwater)
        {
            animator.SetFloat("Horizontal", movement.sqrMagnitude);
            return;
        }
        animator.SetFloat("Horizontal", movement.normalized.x);
        animator.SetFloat("Vertical", movement.normalized.y);

    }
    float timeElapsed;
    public void interpolateRigWeights()
    {
        meleeRig.weight = Mathf.Lerp(meleeRig.weight, meleeRigWeight, timeElapsed / lerpFactor);
        rifleRig.weight = Mathf.Lerp(rifleRig.weight, rifleRigWeight, timeElapsed / lerpFactor);
        pistolRig.weight = Mathf.Lerp(pistolRig.weight, pistolRigWeight, timeElapsed / lerpFactor);
        throwableRig.weight = Mathf.Lerp(throwableRig.weight, throwableRigWeight, timeElapsed / lerpFactor);
        leftHandRig.weight = Mathf.Lerp(leftHandRig.weight, leftHandRigWeight, timeElapsed / lerpFactor);
        leftHandConstraint.weight = Mathf.Lerp(leftHandConstraint.weight, leftHandConstraintWeight, timeElapsed / lerpFactor);
        timeElapsed += Time.deltaTime;
    }

    public void dropCurrentWeapon()
    {
        animator.SetInteger("Weapon", 0); // unarmed

        meleeRigWeight = 0f;
        rifleRigWeight = 0f;
        pistolRigWeight = 0f;
        throwableRigWeight = 0f;
        leftHandRigWeight = 0f;
        leftHandConstraintWeight = 0f;
        leftHandGrabTransform = null;
        leftHandHintTransform = null;
        parentRig.Build();

    }
    public void updateCurrentWeapon(Transform leftHandTarget, Transform leftHandHint)
    {
        leftHandConstraint.data.target = leftHandTarget;
        leftHandConstraint.data.hint = leftHandHint;
        leftHandRigWeight = 1f;
        parentRig.Build();
    }

    public void attack()
    {
        animator.SetTrigger("Attack");
    }

    public void SetWeaponIK(WeaponBehaviour weapon)
    {
        if (weapon == null)
        {
            leftHandConstraintWeight = 0f;
            leftHandRigWeight = 0f;
            return;
        }

        leftHandConstraint.data.target = weapon.ik_target;
        leftHandConstraint.data.hint = weapon.ik_hint;

        leftHandConstraintWeight = 1f;
        leftHandRigWeight = 1f;

        parentRig.Build();
    }


    public void SetWeapon(WeaponType type)
    {
        animator.SetInteger("Weapon", (int)type + 1);

        switch (type)
        {
            case WeaponType.rifle:
                rifleRigWeight = 1;
                pistolRigWeight = 0;
                meleeRigWeight = 0;
                throwableRigWeight = 0;
                break;

            case WeaponType.sidearm:
                rifleRigWeight = 0;
                pistolRigWeight = 1;
                meleeRigWeight = 0;
                throwableRigWeight = 0;
                break;

            case WeaponType.melee:
                rifleRigWeight = 0;
                pistolRigWeight = 0;
                meleeRigWeight = 1;
                throwableRigWeight = 0;
                break;

            case WeaponType.throwable:
                rifleRigWeight = 0;
                pistolRigWeight = 0;
                meleeRigWeight = 0;
                throwableRigWeight = 1;
                break;

        }

    }
}


