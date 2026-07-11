
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
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
    public Rig TorsoRig;
    public float torsoRigWeight;
    public Rig rifleRig;
    public float rifleRigWeight;
    public TwoBoneIKConstraint RiflePositionConstraint;
    public Transform RiflePositionConstraintTarget;
    public TwoBoneIKConstraint leftHandConstraint;
    public float leftHandConstraintWeight;
    public Rig leftHandRig;
    public float leftHandRigWeight;
    public Rig pistolRig;
    public float pistolRigWeight;
    public Rig throwableRig;
    public float throwableRigWeight;
    public float lerpFactor;

    public State currentState;
    public Dictionary<states, State> availableStates = new Dictionary<states, State>();
    public void transition(State state)
    {
        if (!(currentState == null))
            currentState.OnStateExit(this);
        currentState = state;
        state.OnStateEnter(this);
    }

    public void init()
    {
        availableStates.Add(states.Unarmed, new UnarmedState());
        availableStates.Add(states.Rifle, new RifleState());
        availableStates.Add(states.Sidearm, new SidearmState());
        availableStates.Add(states.Melee, new MeleeState());
        availableStates.Add(states.Throwable, new ThrowableState());
        availableStates.Add(states.OverTheShoulder, new OverTheShoulderState());
        transition(availableStates[states.Unarmed]);
    }

    public void Tick()
    {
        currentState.OnStateUpdate(this);
    }
    public void LateTick()
    {
        currentState.OnStateLateUpdate(this);
    }

    public void updateMovemementParams(Vector2 movement, bool grounded, bool climbing, bool inwater)
    {
        animator.SetBool("Grounded", grounded);
        animator.SetBool("Climbing", climbing);
        animator.SetBool("Submerged", inwater);

        if (inwater)
        {
            animator.SetFloat("Horizontal", movement.sqrMagnitude);
            return;
        }
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);

    }
    public void updateCurrentWeapon(WeaponBehaviour weapon)
    {
        leftHandConstraint.data.target = weapon.ik_target;
        leftHandConstraint.data.hint = weapon.ik_hint;

    }

    public void attack()
    {
        animator.SetTrigger("Attack");
    }
}


public abstract class State
{
    protected float timeElapsed;

    public abstract void OnStateEnter(AnimationController controller);
    public abstract void OnStateUpdate(AnimationController controller);
    public abstract void OnStateLateUpdate(AnimationController controller);
    public abstract void OnStateExit(AnimationController controller);

    public int stateToInt(states state)
    {
        switch (state)
        {
            case states.Unarmed:
                return 0;
            case states.Rifle:
                return 1;
            case states.Sidearm:
                return 2;
            case states.Melee:
                return 3;
            case states.Throwable:
                return 4;
            case states.OverTheShoulder:
                return 5;
            default: return 0;
        }
    }
}

public enum states
{
    Unarmed,
    Rifle,
    Sidearm,
    Melee,
    Throwable,
    OverTheShoulder

}

#region states
public class UnarmedState : State
{
    public override void OnStateEnter(AnimationController controller)
    {
        controller.animator.SetInteger("Weapon", stateToInt(states.Unarmed));
        timeElapsed = 0;
        controller.rifleRigWeight = 0;
        controller.pistolRigWeight = 0;
        controller.meleeRigWeight = 0;
        controller.throwableRigWeight = 0;
        controller.leftHandConstraintWeight = 0f;
        controller.leftHandRigWeight = 0f;

    }

    public override void OnStateExit(AnimationController controller)
    {

    }
    public override void OnStateUpdate(AnimationController controller) { }

    public override void OnStateLateUpdate(AnimationController controller)
    {
        controller.meleeRig.weight = Mathf.Lerp(controller.meleeRig.weight, controller.meleeRigWeight, timeElapsed / controller.lerpFactor);
        controller.rifleRig.weight = Mathf.Lerp(controller.rifleRig.weight, controller.rifleRigWeight, timeElapsed / controller.lerpFactor);
        controller.pistolRig.weight = Mathf.Lerp(controller.pistolRig.weight, controller.pistolRigWeight, timeElapsed / controller.lerpFactor);
        controller.throwableRig.weight = Mathf.Lerp(controller.throwableRig.weight, controller.throwableRigWeight, timeElapsed / controller.lerpFactor);
        controller.leftHandRig.weight = Mathf.Lerp(controller.leftHandRig.weight, controller.leftHandRigWeight, timeElapsed / controller.lerpFactor);
        controller.leftHandConstraint.weight = Mathf.Lerp(controller.leftHandConstraint.weight, controller.leftHandConstraintWeight, timeElapsed / controller.lerpFactor);
        timeElapsed += Time.deltaTime;
    }
}
public class RifleState : State
{
    public override void OnStateEnter(AnimationController controller)
    {
        controller.animator.SetInteger("Weapon", stateToInt(states.Rifle));
        timeElapsed = 0;
        controller.rifleRigWeight = 1;
        controller.pistolRigWeight = 0;
        controller.meleeRigWeight = 0;
        controller.throwableRigWeight = 0;
        controller.leftHandConstraintWeight = 1f;
        controller.leftHandRigWeight = 1f;

        controller.torsoRigWeight = 1f;

        controller.parentRig.Build();

    }

    public override void OnStateExit(AnimationController controller)
    {

    }
    public override void OnStateUpdate(AnimationController controller) { }

    public override void OnStateLateUpdate(AnimationController controller)
    {
        controller.meleeRig.weight = Mathf.Lerp(controller.meleeRig.weight, controller.meleeRigWeight, timeElapsed / controller.lerpFactor);
        controller.rifleRig.weight = Mathf.Lerp(controller.rifleRig.weight, controller.rifleRigWeight, timeElapsed / controller.lerpFactor);
        controller.pistolRig.weight = Mathf.Lerp(controller.pistolRig.weight, controller.pistolRigWeight, timeElapsed / controller.lerpFactor);
        controller.throwableRig.weight = Mathf.Lerp(controller.throwableRig.weight, controller.throwableRigWeight, timeElapsed / controller.lerpFactor);
        controller.leftHandRig.weight = Mathf.Lerp(controller.leftHandRig.weight, controller.leftHandRigWeight, timeElapsed / controller.lerpFactor);
        controller.leftHandConstraint.weight = Mathf.Lerp(controller.leftHandConstraint.weight, controller.leftHandConstraintWeight, timeElapsed / controller.lerpFactor);

        //controller.TorsoRig.weight = Mathf.Lerp(controller.TorsoRig.weight, controller.torsoRigWeight, timeElapsed / controller.lerpFactor);

        timeElapsed += Time.deltaTime;
    }
}
public class MeleeState : State
{
    public override void OnStateEnter(AnimationController controller)
    {
        controller.animator.SetInteger("Weapon", stateToInt(states.Melee));
        timeElapsed = 0;
        controller.rifleRigWeight = 0;
        controller.pistolRigWeight = 0;
        controller.meleeRigWeight = 0;
        controller.throwableRigWeight = 0;
        controller.leftHandConstraintWeight = 0f;
        controller.leftHandRigWeight = 0f;

    }

    public override void OnStateExit(AnimationController controller)
    {

    }
    public override void OnStateUpdate(AnimationController controller) { }

    public override void OnStateLateUpdate(AnimationController controller)
    {
        controller.meleeRig.weight = Mathf.Lerp(controller.meleeRig.weight, controller.meleeRigWeight, timeElapsed / controller.lerpFactor);
        controller.rifleRig.weight = Mathf.Lerp(controller.rifleRig.weight, controller.rifleRigWeight, timeElapsed / controller.lerpFactor);
        controller.pistolRig.weight = Mathf.Lerp(controller.pistolRig.weight, controller.pistolRigWeight, timeElapsed / controller.lerpFactor);
        controller.throwableRig.weight = Mathf.Lerp(controller.throwableRig.weight, controller.throwableRigWeight, timeElapsed / controller.lerpFactor);
        controller.leftHandRig.weight = Mathf.Lerp(controller.leftHandRig.weight, controller.leftHandRigWeight, timeElapsed / controller.lerpFactor);
        controller.leftHandConstraint.weight = Mathf.Lerp(controller.leftHandConstraint.weight, controller.leftHandConstraintWeight, timeElapsed / controller.lerpFactor);
        timeElapsed += Time.deltaTime;
    }
}
public class SidearmState : State
{
    public override void OnStateEnter(AnimationController controller)
    {
        controller.animator.SetInteger("Weapon", stateToInt(states.Sidearm));
        timeElapsed = 0;
        controller.rifleRigWeight = 0;
        controller.pistolRigWeight = 1;
        controller.meleeRigWeight = 0;
        controller.throwableRigWeight = 0;
        controller.leftHandConstraintWeight = 1f;
        controller.leftHandRigWeight = 1f;
        controller.parentRig.Build();

    }

    public override void OnStateExit(AnimationController controller)
    {

    }
    public override void OnStateUpdate(AnimationController controller) { }

    public override void OnStateLateUpdate(AnimationController controller)
    {
        controller.meleeRig.weight = Mathf.Lerp(controller.meleeRig.weight, controller.meleeRigWeight, timeElapsed / controller.lerpFactor);
        controller.rifleRig.weight = Mathf.Lerp(controller.rifleRig.weight, controller.rifleRigWeight, timeElapsed / controller.lerpFactor);
        controller.pistolRig.weight = Mathf.Lerp(controller.pistolRig.weight, controller.pistolRigWeight, timeElapsed / controller.lerpFactor);
        controller.throwableRig.weight = Mathf.Lerp(controller.throwableRig.weight, controller.throwableRigWeight, timeElapsed / controller.lerpFactor);
        controller.leftHandRig.weight = Mathf.Lerp(controller.leftHandRig.weight, controller.leftHandRigWeight, timeElapsed / controller.lerpFactor);
        controller.leftHandConstraint.weight = Mathf.Lerp(controller.leftHandConstraint.weight, controller.leftHandConstraintWeight, timeElapsed / controller.lerpFactor);
        timeElapsed += Time.deltaTime;
    }
}
public class ThrowableState : State
{
    public override void OnStateEnter(AnimationController controller)
    {
        controller.animator.SetInteger("Weapon", stateToInt(states.Throwable));
        timeElapsed = 0;
        controller.rifleRigWeight = 0;
        controller.pistolRigWeight = 0;
        controller.meleeRigWeight = 0;
        controller.throwableRigWeight = 1;
        controller.leftHandConstraintWeight = 0f;
        controller.leftHandRigWeight = 0f;

        controller.torsoRigWeight = 1f;


    }

    public override void OnStateExit(AnimationController controller)
    {

    }
    public override void OnStateUpdate(AnimationController controller) { }

    public override void OnStateLateUpdate(AnimationController controller)
    {
        controller.meleeRig.weight = Mathf.Lerp(controller.meleeRig.weight, controller.meleeRigWeight, timeElapsed / controller.lerpFactor);
        controller.rifleRig.weight = Mathf.Lerp(controller.rifleRig.weight, controller.rifleRigWeight, timeElapsed / controller.lerpFactor);
        controller.pistolRig.weight = Mathf.Lerp(controller.pistolRig.weight, controller.pistolRigWeight, timeElapsed / controller.lerpFactor);
        controller.throwableRig.weight = Mathf.Lerp(controller.throwableRig.weight, controller.throwableRigWeight, timeElapsed / controller.lerpFactor);
        controller.leftHandRig.weight = Mathf.Lerp(controller.leftHandRig.weight, controller.leftHandRigWeight, timeElapsed / controller.lerpFactor);
        controller.leftHandConstraint.weight = Mathf.Lerp(controller.leftHandConstraint.weight, controller.leftHandConstraintWeight, timeElapsed / controller.lerpFactor);

        controller.TorsoRig.weight = Mathf.Lerp(controller.TorsoRig.weight, controller.torsoRigWeight, timeElapsed / controller.lerpFactor);

        timeElapsed += Time.deltaTime;
    }
}
public class OverTheShoulderState : State
{
    public override void OnStateEnter(AnimationController controller)
    {
        controller.animator.SetInteger("Weapon", stateToInt(states.OverTheShoulder));
        timeElapsed = 0;
        controller.rifleRigWeight = 0;
        controller.pistolRigWeight = 0;
        controller.meleeRigWeight = 0;
        controller.throwableRigWeight = 0;
        controller.leftHandConstraintWeight = 1f;
        controller.leftHandRigWeight = 1f;

        controller.torsoRigWeight = 1f;

        controller.parentRig.Build();

    }

    public override void OnStateExit(AnimationController controller)
    {

    }
    public override void OnStateUpdate(AnimationController controller) { }

    public override void OnStateLateUpdate(AnimationController controller)
    {
        controller.meleeRig.weight = Mathf.Lerp(controller.meleeRig.weight, controller.meleeRigWeight, timeElapsed / controller.lerpFactor);
        controller.rifleRig.weight = Mathf.Lerp(controller.rifleRig.weight, controller.rifleRigWeight, timeElapsed / controller.lerpFactor);
        controller.pistolRig.weight = Mathf.Lerp(controller.pistolRig.weight, controller.pistolRigWeight, timeElapsed / controller.lerpFactor);
        controller.throwableRig.weight = Mathf.Lerp(controller.throwableRig.weight, controller.throwableRigWeight, timeElapsed / controller.lerpFactor);
        controller.leftHandRig.weight = Mathf.Lerp(controller.leftHandRig.weight, controller.leftHandRigWeight, timeElapsed / controller.lerpFactor);
        controller.leftHandConstraint.weight = Mathf.Lerp(controller.leftHandConstraint.weight, controller.leftHandConstraintWeight, timeElapsed / controller.lerpFactor);

        //controller.TorsoRig.weight = Mathf.Lerp(controller.TorsoRig.weight, controller.torsoRigWeight, timeElapsed / controller.lerpFactor);

        timeElapsed += Time.deltaTime;
    }
}

#endregion