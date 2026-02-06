
using KinematicCharacterController;
using UnityEngine;

public struct PlayerInputs
{
    public float Horizontal;
    public float Vertical;
    public Quaternion transformRotation;

}
public class LocomotionController : MonoBehaviour, ICharacterController
{
    [SerializeField] public KinematicCharacterMotor motor;
    [SerializeField] private Vector3 gravity = new Vector3(0f, -18f, 0f);
    [SerializeField] private float maxStableMoveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float stableVelocityInterpolationFactor;
    [SerializeField] private float stableRotationInterpolationFactor;

    [SerializeField] private LayerMask whatIsGround;

    Vector3 moveInput;
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
    public void setInputs(ref PlayerInputs inputs)
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
        cameraForward = inputs.transformRotation;

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
        currentRotation = Quaternion.Euler(0, cameraForward.eulerAngles.y, 0);
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (motor.GroundingStatus.IsStableOnGround)
        {
            float currentVelocityMagnitude = currentVelocity.magnitude;
            Vector3 effectiveGroundNormal = motor.GroundingStatus.GroundNormal;

            currentVelocity = motor.GetDirectionTangentToSurface(currentVelocity, effectiveGroundNormal) * currentVelocityMagnitude;

            Vector3 inputRight = Vector3.Cross(moveInput, motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(effectiveGroundNormal, inputRight).normalized * moveInput.magnitude;

            Vector3 targetMovementVelocity = reorientedInput * maxStableMoveSpeed;

            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-stableVelocityInterpolationFactor * deltaTime));
            currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1f - Mathf.Exp(-stableRotationInterpolationFactor * deltaTime));
        }
        else
        {
            currentVelocity += gravity * deltaTime;
        }

        if (jumpRequested && motor.GroundingStatus.IsStableOnGround)
        {
            currentVelocity += (motor.CharacterUp * jumpForce) - Vector3.Project(currentVelocity, motor.CharacterUp);
            motor.ForceUnground();
            jumpRequested = false;
        }
    }

}
