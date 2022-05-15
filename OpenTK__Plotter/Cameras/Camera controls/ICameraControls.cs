using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTK__Plotter.Cameras.Camera_controls;

public interface ICameraControls
{
    public void UpdatePosition(Camera camera, FrameEventArgs e, KeyboardState keyboardState, MouseState mouseState);
}