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
            currentCamera.Position += currentCamera.Front * CameraSettings.CameraSpeed * (float) e.Time;
        }

        if (kbdIn.IsKeyDown(Keys.S))
        {
            currentCamera.Position -= currentCamera.Front * CameraSettings.CameraSpeed * (float) e.Time;
        }

        if (kbdIn.IsKeyDown(Keys.A))
        {
            currentCamera.Position -= currentCamera.Right * CameraSettings.CameraSpeed * (float) e.Time;
        }

        if (kbdIn.IsKeyDown(Keys.D))
        {
            currentCamera.Position += currentCamera.Right * CameraSettings.CameraSpeed * (float) e.Time;
        }

        if (kbdIn.IsKeyDown(Keys.Space))
        {
            currentCamera.Position += currentCamera.Up * CameraSettings.CameraSpeed * (float) e.Time;
        }

        if (kbdIn.IsKeyDown(Keys.LeftShift))
        {
            currentCamera.Position -= currentCamera.Up * CameraSettings.CameraSpeed * (float) e.Time;
        }

        if (currentCamera.FirstMove)
        {
            currentCamera.LastPos = new Vector2(mouseIn.X, mouseIn.Y);
            currentCamera.FirstMove = false;
        }
        else
        {
            var deltaX = mouseIn.X - currentCamera.LastPos.X;
            var deltaY = mouseIn.Y - currentCamera.LastPos.Y;
            currentCamera.LastPos = new Vector2(mouseIn.X, mouseIn.Y);
            
            currentCamera.Yaw += deltaX * CameraSettings.Sensitivity;
            currentCamera.Pitch -= deltaY * CameraSettings.Sensitivity;
        }
    }
}