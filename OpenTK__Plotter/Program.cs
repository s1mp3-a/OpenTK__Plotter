using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;

namespace OpenTK__Plotter
{
    public static class Program
    {
        private static void Main()
        {

            var nativeWindowSettings = new NativeWindowSettings()
            {
                Size = new Vector2i(1920, 1080),
                Title = "OpenTK Plotter",
                Flags = ContextFlags.ForwardCompatible,
            };

            using (var window = new PlotterWindow(GameWindowSettings.Default, nativeWindowSettings))
            {
                window.VSync = VSyncMode.On;
                window.Run();
            }
        }
    }
}