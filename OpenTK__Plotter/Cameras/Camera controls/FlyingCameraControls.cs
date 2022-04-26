using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTK__Plotter.Cameras.Camera_controls;

public class FlyingCameraControls : ICameraControls
{
    public void UpdatePosition(Camera camera, FrameEventArgs e, KeyboardState keyboardState, MouseState mouseState)
    {
        var currentCamera = camera as FlyingCamera ?? throw new NullReferenceException();
        
        var kbdIn = keyboardState;
        var mouseIn = mouseState;
        
        if (kbdIn.IsKeyDown(Keys.W))
        {
            currentCamera.Position += currentCamera.Front * CameraSettings.cameraSpeed * (float) e.Time; // Forward
        }

        if (kbdIn.IsKeyDown(Keys.S))
        {
            currentCamera.Position -= currentCamera.Front * CameraSettings.cameraSpeed * (float) e.Time; // Backwards
        }

        if (kbdIn.IsKeyDown(Keys.A))
        {
            currentCamera.Position -= currentCamera.Right * CameraSettings.cameraSpeed * (float) e.Time; // Left
        }

        if (kbdIn.IsKeyDown(Keys.D))
        {
            currentCamera.Position += currentCamera.Right * CameraSettings.cameraSpeed * (float) e.Time; // Right
        }

        if (kbdIn.IsKeyDown(Keys.Space))
        {
            currentCamera.Position += currentCamera.Up * CameraSettings.cameraSpeed * (float) e.Time; // Up
        }

        if (kbdIn.IsKeyDown(Keys.LeftShift))
        {
            currentCamera.Position -= currentCamera.Up * CameraSettings.cameraSpeed * (float) e.Time; // Down
        }

        if (currentCamera.FirstMove) // This bool variable is initially set to true.
        {
            currentCamera.LastPos = new Vector2(mouseIn.X, mouseIn.Y);
            currentCamera.FirstMove = false;
        }
        else
        {
            // Calculate the offset of the mouse position
            var deltaX = mouseIn.X - currentCamera.LastPos.X;
            var deltaY = mouseIn.Y - currentCamera.LastPos.Y;
            currentCamera.LastPos = new Vector2(mouseIn.X, mouseIn.Y);

            // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            currentCamera.Yaw += deltaX * CameraSettings.sensitivity;
            currentCamera.Pitch -= deltaY * CameraSettings.sensitivity; // Reversed since y-coordinates range from bottom to top
        }
    }
}