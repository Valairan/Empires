using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float defaultDistance = 6f;
    [SerializeField] private float minimumDistance = 7f;
    [SerializeField] private float maximumDistance = 10f;
    [SerializeField] private float distanceMovementSpeed = 5f;
    [SerializeField] private float movementInterpolationFactor = 10f;
    [SerializeField] private float horizontalSensitivity = 10f;
    [SerializeField] private float verticalSensitivity = 10f;
    [SerializeField] private float rotationInterpolationFactor = 10000f;
    [SerializeField] private float followInterpolationFactor = 10000f;
    [SerializeField] private float minimumAngle = -90f;
    [SerializeField] private float maximumAngle = 90f;
    [SerializeField] private float defaultAngle = 20f;
    [SerializeField] public Transform cameraFollowTarget;
    [SerializeField] private Vector3 currentFollowPosition, cameraForward;
    [SerializeField] private float targetAngle;

    private float currentDistance, targetDistance;

    private void Awake()
    {
        currentDistance = defaultDistance;
        targetDistance = currentDistance;
        targetAngle = 0f;
        cameraForward = Vector3.forward;
    }

    public void setFollowTransform(Transform t)
    {
        cameraFollowTarget = t;
        currentFollowPosition = t.position;
        cameraForward = t.forward;
    }


    private void OnValidate()
    {
        defaultDistance = Mathf.Clamp(defaultDistance, minimumDistance, maximumDistance);
        defaultAngle = Mathf.Clamp(defaultAngle, minimumAngle, maximumAngle);
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


    public Vector3 HandlePosition(float deltaTime, bool AimInput, Quaternion targetRotation)
    {
        targetDistance = (AimInput) ? minimumDistance : maximumDistance;

        currentFollowPosition = Vector3.Lerp(currentFollowPosition, cameraFollowTarget.position, 1f - Mathf.Exp(-movementInterpolationFactor * deltaTime));
        Vector3 targetPosition = currentFollowPosition - ((targetRotation * Vector3.forward) * currentDistance);

        currentDistance = Mathf.Lerp(currentDistance, targetDistance, 1f - Mathf.Exp(-movementInterpolationFactor * deltaTime));

        return targetPosition;
    }

}
