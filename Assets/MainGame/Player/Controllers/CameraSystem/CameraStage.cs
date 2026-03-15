public interface ICameraStage
{
    // Directly receives the camera controller to modify rotation, position, distance, etc.
    void Process(CameraController camera, float deltaTime);
}