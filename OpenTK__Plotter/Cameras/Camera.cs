using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK__Plotter.Cameras.Camera_controls;

namespace OpenTK__Plotter.Cameras;

public abstract class Camera
{
    //Control map for the camera
    private readonly ICameraControls _cameraControls;

    public void UpdateCamera(FrameEventArgs e, KeyboardState kbdIn, MouseState mouseIn)
    {
        _cameraControls.UpdatePosition(this, e, kbdIn, mouseIn);
    }

    //Model matrix is identity since we map world coordinates to screen in a 1 : 1 ratio
    public Matrix4 GetModelMatrix() => Matrix4.Identity;
    public abstract Matrix4 GetViewMatrix();
    public abstract Matrix4 GetProjectionMatrix();

    protected Camera(ICameraControls cameraControls)
    {
        _cameraControls = cameraControls;
    }
}