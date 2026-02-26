
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
    public void updateAnimationParams(Vector2 movement, bool grounded, bool sideArm, bool rifle, bool melee, bool inwater)
    {
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Submerged", inwater);
        if (inwater)
        {
            animator.SetFloat("Horizontal", movement.sqrMagnitude);
            return;
        }
        animator.SetFloat("Horizontal", Mathf.Round(movement.x));
        animator.SetFloat("Vertical", Mathf.Round(movement.y));
        animator.SetBool("SideArm", sideArm);
        animator.SetBool("Rifle", rifle);
        animator.SetBool("Melee", melee);
    }

    public void interpolateRigWeights()
    {
        meleeRig.weight = Mathf.Lerp(meleeRig.weight, meleeRigWeight, Time.deltaTime * lerpFactor);
        rifleRig.weight = Mathf.Lerp(rifleRig.weight, rifleRigWeight, Time.deltaTime * lerpFactor);
        pistolRig.weight = Mathf.Lerp(pistolRig.weight, pistolRigWeight, Time.deltaTime * lerpFactor);
        throwableRig.weight = Mathf.Lerp(throwableRig.weight, throwableRigWeight, Time.deltaTime * lerpFactor);
        leftHandRig.weight = Mathf.Lerp(leftHandRig.weight, leftHandRigWeight, Time.deltaTime * lerpFactor);
        leftHandConstraint.weight = Mathf.Lerp(leftHandConstraint.weight, leftHandConstraintWeight, Time.deltaTime * lerpFactor);
    }

    public void updateCurrentWeapon(WeaponType type, Transform leftHandTarget, Transform leftHandHint)
    {
        switch (type)
        {
            case WeaponType.melee: meleeRigWeight = 1f; break;
            case WeaponType.rifle: rifleRigWeight = 1f; break;
            case WeaponType.sidearm: pistolRigWeight = 1f; break;
            case WeaponType.throwable: throwableRigWeight = 1f; break;
        }

        leftHandConstraint.data.target = leftHandTarget;
        leftHandConstraint.data.hint = leftHandHint;
        leftHandRigWeight = 1f;
        parentRig.Build();
    }

    public void attack()
    {
        animator.SetTrigger("Attack");
    }


}
