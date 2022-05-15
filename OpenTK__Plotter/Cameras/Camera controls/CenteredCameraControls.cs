using System.Diagnostics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTK__Plotter.Cameras.Camera_controls;

public class CenteredCameraControls : ICameraControls
{
    public void UpdatePosition(Camera camera, FrameEventArgs e, KeyboardState keyboardState, MouseState mouseState)
    {
        var currentCam = camera as CenteredCamera;

        if (keyboardState.IsKeyDown(Keys.E))
        {
            currentCam.ZAngle += 0.5f * (float) e.Time;
        }

        if (keyboardState.IsKeyDown(Keys.Q))
        {
            currentCam.ZAngle -= 0.5f * (float) e.Time;
        }

        if (keyboardState.IsKeyDown(Keys.R))
        {
            currentCam.ResetAngles();
        }

        if (keyboardState.IsKeyReleased(Keys.F))
        {
            currentCam.IsOrthographic = !currentCam.IsOrthographic;
        }

        if (keyboardState.IsKeyDown(Keys.A))
        {
            currentCam.YAngle += 0.5f * (float) e.Time;
        }

        if (keyboardState.IsKeyDown(Keys.D))
        {
            currentCam.YAngle -= 0.5f * (float) e.Time;
        }
        
        if (mouseState.IsAnyButtonDown)
        {
            var deltaX = mouseState.X - mouseState.PreviousX;
            var deltaY = mouseState.Y - mouseState.PreviousY;
            
            currentCam.YAngle += deltaX * CameraSettings.Sensitivity * (float) e.Time;
            currentCam.ZAngle -= deltaY * CameraSettings.Sensitivity * (float) e.Time;
        }

        if (mouseState.ScrollDelta != Vector2.Zero)
        {
            if (mouseState.ScrollDelta.Y < 0)
                currentCam.Position += (currentCam.Center - currentCam.Position) * 0.1f;
            else
                currentCam.Position -= (currentCam.Center - currentCam.Position) * 0.1f;
        }
    }
}