using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float defaultDistance = 6f;
    [SerializeField] private float minimumDistance = 7f;
    [SerializeField] private float cameraDefaultDistance = 10f;
    [SerializeField] private float distanceMovementSpeed = 5f;
    [SerializeField] private float movementInterpolationFactor = 10f;
    [SerializeField] private float horizontalSensitivity = 10f;
    [SerializeField] private float verticalSensitivity = 10f;
    [SerializeField] private float rotationInterpolationFactor = 10000f;
    [SerializeField] private float followInterpolationFactor = 10000f;
    [SerializeField] private float minimumAngle = -80f;
    [SerializeField] private float maximumAngle = 80f;
    [SerializeField] private float defaultAngle = 20f;
    [SerializeField] private LayerMask cameraBlockers;
    [SerializeField] public Transform cameraFollowTarget;
    [SerializeField] private Vector3 currentFollowPosition, cameraForward;
    [SerializeField] private float targetAngle;

    [HideInInspector] public float stateCameraDistance;
    [SerializeField] float cameraStateInterpolationSpeed;

    private float currentDistance, targetDistance;

    [Header("Camera States")]
    public Dictionary<CameraStates, CameraState> availableStates = new Dictionary<CameraStates, CameraState>();
    [SerializeReference]
    public CameraState defaultCameraState = new DefaultCameraState();
    [SerializeReference]
    public CameraState aimingCameraState = new AimingState();
    [SerializeReference]
    public CameraState aimingCrouchCameraState = new AimingCrouchState();
    [SerializeReference]
    public CameraState crouchCameraState = new CrouchState();
    [SerializeReference]
    public CameraState submergedCameraState = new SubmergedState();
    [SerializeReference]
    public CameraState climbingCameraState = new ClimbingState();

    public CameraState currentState;
    public float cameraCurrentFollowDistance;
    public void transition(CameraState state)
    {
        if (!(currentState == null))
            currentState.OnStateExit(this);
        currentState = state;
        state.OnStateEnter(this);
    }

    private void Awake()
    {
        currentDistance = defaultDistance;
        targetDistance = currentDistance;
        targetAngle = 0f;
        cameraForward = Vector3.forward;
        availableStates.Add(CameraStates.Default, defaultCameraState);
        availableStates.Add(CameraStates.Crouch, crouchCameraState);
        availableStates.Add(CameraStates.Aiming, aimingCameraState);
        availableStates.Add(CameraStates.AimingCrouch, aimingCrouchCameraState);
        availableStates.Add(CameraStates.Climbing, climbingCameraState);
        availableStates.Add(CameraStates.Submerged, submergedCameraState);
        transition(availableStates[CameraStates.Default]);
    }

    public void setFollowTransform(Transform t)
    {
        cameraFollowTarget = t;
        currentFollowPosition = t.position;
        cameraForward = t.forward;
    }


    private void OnValidate()
    {
        defaultDistance = Mathf.Clamp(defaultDistance, minimumDistance, cameraDefaultDistance);
        defaultAngle = Mathf.Clamp(defaultAngle, minimumAngle, maximumAngle);
    }

    public void Tick()
    {
        currentState.OnStateUpdate(this);
    }
    public void LateTick()
    {
        currentState.OnStateLateUpdate(this);

    }

    public Quaternion HandleRotation(Quaternion currentRotation, float deltaTime, Vector3 rotationInput)
    {
        Quaternion rotationFromInput = Quaternion.Euler(cameraFollowTarget.up * (rotationInput.x * horizontalSensitivity));
        cameraForward = rotationFromInput * cameraForward;
        Quaternion forwardRotation = Quaternion.LookRotation(cameraForward, cameraFollowTarget.up);

        targetAngle -= (rotationInput.y * verticalSensitivity);
        targetAngle = Mathf.Clamp(targetAngle, minimumAngle, maximumAngle);
        Quaternion verticalRotation = Quaternion.Euler(targetAngle, 0, 0);

        return Quaternion.Slerp(currentRotation, forwardRotation * verticalRotation, rotationInterpolationFactor * deltaTime);

    }

    public Vector3 HandlePosition(float deltaTime, bool AimInput, Quaternion targetRotation, Vector3 cameraCurrentPosition)
    {
        if (Physics.SphereCast(cameraFollowTarget.position, 0.4f, (cameraCurrentPosition - cameraFollowTarget.position).normalized, out RaycastHit hit, defaultDistance, cameraBlockers))
            cameraCurrentFollowDistance = Vector3.Distance(hit.point, cameraFollowTarget.position) - 0.2f;
        else
            cameraCurrentFollowDistance = cameraDefaultDistance;

        targetDistance = cameraCurrentFollowDistance < stateCameraDistance ? cameraCurrentFollowDistance : stateCameraDistance;

        currentFollowPosition = Vector3.Lerp(currentFollowPosition, cameraFollowTarget.position, 1f - Mathf.Exp(-movementInterpolationFactor * deltaTime));
        Vector3 targetPosition = currentFollowPosition - ((targetRotation * Vector3.forward) * currentDistance);

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, 1f - Mathf.Exp(-movementInterpolationFactor * deltaTime));

        return targetPosition;
    }

}
#region States
public enum CameraStates
{
    Default,
    Aiming,
    AimingCrouch,
    Crouch,
    Climbing,
    Submerged,

}
[System.Serializable]
public abstract class CameraState
{
    public float stateCameraDistance;
    public Vector3 stateCameraPosition;
    protected float timeElapsed;
    public abstract void OnStateEnter(CameraController controller);
    public abstract void OnStateUpdate(CameraController controller);
    public abstract void OnStateLateUpdate(CameraController controller);
    public abstract void OnStateExit(CameraController controller);
}

[System.Serializable]
public class DefaultCameraState : CameraState
{
    public override void OnStateEnter(CameraController controller)
    {
        timeElapsed = 0;
        controller.cameraFollowTarget.localPosition = stateCameraPosition;
        controller.stateCameraDistance = stateCameraDistance;

    }

    public override void OnStateExit(CameraController controller)
    {
    }

    public override void OnStateLateUpdate(CameraController controller)
    {

    }

    public override void OnStateUpdate(CameraController controller)
    {
    }
}
[System.Serializable]
public class AimingState : CameraState
{
    public override void OnStateEnter(CameraController controller)
    {
        timeElapsed = 0;
        controller.cameraFollowTarget.localPosition = stateCameraPosition;
        controller.stateCameraDistance = stateCameraDistance;

    }

    public override void OnStateExit(CameraController controller)
    {
    }

    public override void OnStateLateUpdate(CameraController controller)
    {

    }

    public override void OnStateUpdate(CameraController controller)
    {
    }
}
[System.Serializable]
public class AimingCrouchState : CameraState
{
    public override void OnStateEnter(CameraController controller)
    {
        timeElapsed = 0;
        controller.cameraFollowTarget.localPosition = stateCameraPosition;
        controller.stateCameraDistance = stateCameraDistance;

    }

    public override void OnStateExit(CameraController controller)
    {
    }

    public override void OnStateLateUpdate(CameraController controller)
    {

    }

    public override void OnStateUpdate(CameraController controller)
    {
    }
}
[System.Serializable]
public class CrouchState : CameraState
{

    public override void OnStateEnter(CameraController controller)
    {
        timeElapsed = 0;
        controller.cameraFollowTarget.localPosition = stateCameraPosition;
        controller.stateCameraDistance = stateCameraDistance;

    }

    public override void OnStateExit(CameraController controller)
    {
    }

    public override void OnStateLateUpdate(CameraController controller)
    {

    }

    public override void OnStateUpdate(CameraController controller)
    {
    }
}
[System.Serializable]
public class SubmergedState : CameraState
{

    public override void OnStateEnter(CameraController controller)
    {
        timeElapsed = 0;
        controller.cameraFollowTarget.localPosition = stateCameraPosition;
        controller.stateCameraDistance = stateCameraDistance;

    }

    public override void OnStateExit(CameraController controller)
    {
    }

    public override void OnStateLateUpdate(CameraController controller)
    {

    }

    public override void OnStateUpdate(CameraController controller)
    {
    }
}
[System.Serializable]
public class ClimbingState : CameraState
{
    public override void OnStateEnter(CameraController controller)
    {
        timeElapsed = 0;
        controller.cameraFollowTarget.localPosition = stateCameraPosition;
        controller.stateCameraDistance = stateCameraDistance;

    }

    public override void OnStateExit(CameraController controller)
    {
    }

    public override void OnStateLateUpdate(CameraController controller)
    {

    }

    public override void OnStateUpdate(CameraController controller)
    {
    }
}

#endregion