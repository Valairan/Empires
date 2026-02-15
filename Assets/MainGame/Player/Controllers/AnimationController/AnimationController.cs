
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
    public Rig meleeRig;
    public Rig rifleRig;
    public TwoBoneIKConstraint leftHandConstraint;
    public Rig leftHandRig;
    public Rig pistolRig;
    public Rig throwableRig;
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


    public void updateCurrentWeapon(WeaponType type, Transform leftHandTarget, Transform leftHandHint)
    {
        switch (type)
        {
            case WeaponType.melee: meleeRig.weight = 1f; break;
            case WeaponType.rifle: rifleRig.weight = 1f; break;
            case WeaponType.sidearm: pistolRig.weight = 1f; break;
            case WeaponType.throwable: throwableRig.weight = 1f; break;
        }
        if (leftHandTarget != null)
        {
            leftHandConstraint.data.target = leftHandTarget;
            leftHandConstraint.data.hint = leftHandHint;
            parentRig.Build();
            leftHandRig.weight = 1f;
        }
    }

    private void deactivateAllRigs()
    {
        meleeRig.weight = 0f;
        rifleRig.weight = 0f;
        pistolRig.weight = 0f;
        throwableRig.weight = 0f;
        leftHandRig.weight = 0f;
    }

}
