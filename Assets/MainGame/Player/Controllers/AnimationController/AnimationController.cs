
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    public Animator animator;
    public Transform leftHandTarget;
    [Range(0, 1)] public float ikWeight = 1f;

    public void setTarget(Transform target)
    {
        leftHandTarget = target;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!leftHandTarget) return;
        animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
    }
}
