
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.UIElements;


public class LocomotionController : MonoBehaviour, ICharacterController
{
    [SerializeField] public KinematicCharacterMotor motor;
    [SerializeField] private Vector3 gravity = new Vector3(0f, -18f, 0f);
    [SerializeField] private float runSpeed;
    [SerializeField] private float sneakSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float stableVelocityInterpolationFactor;
    [SerializeField] private float stableRotationInterpolationFactor;

    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float ladderClimbSpeed = 3f;

    bool climbing = false;
    Vector3 moveInput;
    bool sneaking;
    Vector3 ladderNormal = Vector3.up;
    Vector3 lookInput;
    public bool jumpRequested;

    Quaternion cameraForward;

    public void init()
    {
        motor.CharacterController = this;
        motor.StableGroundLayers = whatIsGround;
    }

    public void jump()
    {
        jumpRequested = true;
    }
    public void setInputs(ref InputContext inputs)
    {
        Vector3 moveInputVector = Vector3.ClampMagnitude((new Vector3(inputs.Horizontal, 0f, inputs.Vertical)), 1f);
        Vector3 cameraForwardDirection = Vector3.ProjectOnPlane(inputs.transformRotation * Vector3.forward, motor.CharacterUp).normalized;
        if (cameraForwardDirection.sqrMagnitude == 0f)
        {
            cameraForwardDirection = Vector3.ProjectOnPlane(inputs.transformRotation * Vector3.up, motor.CharacterUp).normalized;
        }

        Quaternion cameraForwardRotation = Quaternion.LookRotation(cameraForwardDirection, motor.CharacterUp);
        moveInput = cameraForwardRotation * moveInputVector;
        lookInput = moveInput.normalized;
        sneaking = inputs.crouching;
        climbing = inputs.climbing;
        cameraForward = inputs.transformRotation;
        ladderNormal = inputs.ladderNormal;

    }

    public void AfterCharacterUpdate(float deltaTime)
    {

    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
        //Debug.Log(hitCollider.name + " <---------");
    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void PostGroundingUpdate(float deltaTime)
    {

    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        if (!climbing && motor.GroundingStatus.IsStableOnGround)
            currentRotation = Quaternion.Euler(0, cameraForward.eulerAngles.y, 0);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {


        if (climbing)
        {
            // 1. Derive spatial vectors directly from your custom ladder's surface normal
            Vector3 ladderRight = Vector3.Cross(ladderNormal, motor.CharacterUp).normalized;
            Vector3 ladderUp = Vector3.Cross(ladderRight, ladderNormal).normalized;

            // 2. Determine if the player's world direction is pushing into or pulling away from the ladder
            float approachDirection = Vector3.Dot(moveInput.normalized, ladderNormal);
            float ladderNormalAngle = Vector2.Dot(ladderUp, ladderNormal);

            if (ladderNormalAngle > .3f && ladderNormalAngle < .5f)
            {
                currentVelocity = transform.forward * ladderClimbSpeed;
            }
            Vector3 targetLadderVelocity = Vector3.zero;

            if (moveInput.x != 0)
            {
                if (approachDirection < -0.1f)
                {
                    // Moving TOWARD the ladder face -> Ascend
                    targetLadderVelocity = moveInput.sqrMagnitude > 0 ? (ladderUp * ladderClimbSpeed) : Vector3.zero;
                    motor.ForceUnground();

                }
                else if (approachDirection > 0.1f)
                {
                    // Moving AWAY from the ladder face -> Descend
                    targetLadderVelocity = moveInput.sqrMagnitude > 0 ? (-ladderUp * ladderClimbSpeed) : Vector3.zero;
                    if (motor.GroundingStatus.IsStableOnGround)
                    {
                        if (approachDirection > 0.1f)
                        {
                            targetLadderVelocity = moveInput * ladderClimbSpeed;
                        }
                    }
                    //motor.ForceUnground();
                }
            }
            currentVelocity = targetLadderVelocity;



        }
        else if (motor.GroundingStatus.IsStableOnGround)
        {
            float currentVelocityMagnitude = currentVelocity.magnitude;
            Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;

            currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

            Vector3 inputRight = Vector3.Cross(moveInput, motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveInput.magnitude;

            Vector3 targetMovementVelocity = reorientedInput * (sneaking ? sneakSpeed : runSpeed);

            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-stableVelocityInterpolationFactor * deltaTime));
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-stableRotationInterpolationFactor * deltaTime));
        }
        else
        {
            currentVelocity += gravity * deltaTime;
        }

        if (jumpRequested)
        {
            if (climbing)
            {
                // Push the player away from the ladder face, plus an upward kick
                currentVelocity = ladderNormal * (jumpForce * .5f);
                motor.ForceUnground();
                jumpRequested = false;
                climbing = false;
            }
            else if (motor.GroundingStatus.IsStableOnGround)
            {
                currentVelocity += (motor.CharacterUp * jumpForce) - Vector3.Project(currentVelocity, motor.CharacterUp);
                motor.ForceUnground();
                jumpRequested = false;
            }
        }
    }

}
