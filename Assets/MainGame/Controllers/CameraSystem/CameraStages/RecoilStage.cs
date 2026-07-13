using UnityEngine;
public class RecoilStage : ICameraStage
{
    private Vector2 currentOffset;    // Camera offset this frame
    private Vector2 targetOffset;     // Desired offset from last shot
    private bool returnToCenter;
    private float returnSpeed;

    /// <summary>
    /// Call once per shot
    /// </summary>
    public void ApplyRecoil(Vector2 pattern, float speed, float recovery, bool toCenter)
    {
        float xOffset = pattern.x * UnityEngine.Random.Range(0.8f, 1.2f);
        float yOffset = pattern.y * UnityEngine.Random.Range(0.8f, 1.2f);

        targetOffset += new Vector2(xOffset, yOffset); // add per shot
        returnToCenter = toCenter;
        returnSpeed = recovery;
    }

    public void Process(CameraController camera, float deltaTime)
    {
        // Apply full target offset instantly
        currentOffset = targetOffset;

        //camera.ModifyPitch(currentOffset.y);
        //camera.ModifyYaw(currentOffset.x);

        // Handle return to center
        if (returnToCenter)
        {
            // Linear return per second
            Vector2 returnDelta = Vector2.ClampMagnitude(targetOffset, returnSpeed * deltaTime);
            targetOffset -= returnDelta;
        }
        else
        {
            targetOffset = currentOffset; // lock at last recoil
        }
    }
}