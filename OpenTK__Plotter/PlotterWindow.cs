using System;
using System.Runtime.InteropServices;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK__Plotter.Cameras;

namespace OpenTK__Plotter
{
    static class PlotConfig
    {
        public const float GridXSpan = 9f;
        public const float GridYSpan = 9f;
        public const float GridZSpan = 16f;

        public const float PlotXSpan = 9f;
        public const float PlotYSpan = 9f;
        public const float PlotZSpan = 16f;

        public const int MeshResX = 50;
        public const int MeshResZ = 50;

        public static readonly Color4 Background = new Color4(.2f, .3f, .4f, 1f);
    }

    public class PlotterWindow : GameWindow
    {
        private readonly PlotGrid _plotGrid;
        private readonly PlotMesh _plotMesh;

        private int _vaoBoundary;
        private int _vboBoundary;

        private int _vaoGridLines;
        private int _vboGridLines;

        private int _vaoPlotMesh;
        private int _vboPlotMesh;
        private int _eboPlotMesh;

        private int _computeBuffer;

        private FlyingCamera _flyingCamera;
        private CenteredCamera _centeredCamera;
        private Camera _testCam;

        private bool _firstMove = true;
        private Vector2 _lastPos;

        private Shader _gridShader;
        private Shader _plotShader;
        private ComputeShader _meshComputeShader;

        private float _timer;

        public PlotterWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _plotGrid = new PlotGrid(PlotConfig.GridXSpan,
                PlotConfig.GridZSpan,
                PlotConfig.GridYSpan);

            _plotMesh = new PlotMesh(PlotConfig.PlotXSpan,
                PlotConfig.PlotYSpan,
                PlotConfig.PlotZSpan,
                PlotConfig.MeshResX,
                PlotConfig.MeshResZ);

            _plotMesh.ConstructMesh();
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            PrintOpenGlInfo();

            GL.ClearColor(PlotConfig.Background);

            GL.Enable(EnableCap.DepthTest);

            _vaoBoundary = GL.GenVertexArray();
            GL.BindVertexArray(_vaoBoundary);
            _vboBoundary = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboBoundary);
            GL.BufferData
            (
                BufferTarget.ArrayBuffer,
                _plotGrid.Boundaries.Length * sizeof(float),
                _plotGrid.Boundaries,
                BufferUsageHint.StaticDraw
            );
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _vaoGridLines = GL.GenVertexArray();
            GL.BindVertexArray(_vaoGridLines);
            _vboGridLines = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboGridLines);
            GL.BufferData
            (
                BufferTarget.ArrayBuffer,
                _plotGrid.GridLines.Length * sizeof(float),
                _plotGrid.GridLines,
                BufferUsageHint.StaticDraw
            );
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            _vboPlotMesh = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboPlotMesh);
            GL.BufferData
            (
                BufferTarget.ArrayBuffer,
                _plotMesh.Vertices.Length * sizeof(float) * 3,
                _plotMesh.Vertices,
                BufferUsageHint.DynamicDraw
            );
            _vaoPlotMesh = GL.GenVertexArray();
            GL.BindVertexArray(_vaoPlotMesh);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            _eboPlotMesh = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _eboPlotMesh);
            GL.BufferData
            (
                BufferTarget.ElementArrayBuffer,
                _plotMesh.Triangles.Length * sizeof(uint),
                _plotMesh.Triangles,
                BufferUsageHint.DynamicDraw
            );

            _gridShader = new Shader("../../../gridShader.vert", "../../../gridShader.frag");
            _gridShader.Use();

            _plotShader = new Shader("../../../gridShader.vert", "../../../plotShader.frag");

            _meshComputeShader = new ComputeShader("../../../meshCompute.glsl");
            _computeBuffer = GL.GenBuffer();
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, _computeBuffer);
            GL.BufferData
            (
                BufferTarget.ShaderStorageBuffer,
                sizeof(float) * PlotConfig.MeshResX * PlotConfig.MeshResZ * 3,
                IntPtr.Zero,
                BufferUsageHint.DynamicRead
            );

            _flyingCamera = new FlyingCamera(Vector3.UnitZ * 3, Size.X / (float) Size.Y);
            _testCam = _flyingCamera;
            var center = new Vector3(_plotGrid.XSpan / 2, _plotGrid.YSpan / 2, _plotGrid.ZSpan / 2);
            _centeredCamera = new CenteredCamera(center, Size.X / (float) Size.Y);

            CursorGrabbed = false;
        }

        private void PrintOpenGlInfo()
        {
            Console.WriteLine("Using the following openGL parameters:");
            Console.WriteLine($"GL vendor -- {GL.GetString(StringName.Vendor)}");
            Console.WriteLine($"GL renderer -- {GL.GetString(StringName.Renderer)}");
            Console.WriteLine($"GL extensions -- {GL.GetString(StringName.Extensions)}");
            Console.WriteLine($"GL version -- {GL.GetString(StringName.Version)}");
            Console.WriteLine($"GL shading lang version -- {GL.GetString(StringName.ShadingLanguageVersion)}");
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _gridShader.Use();

            _gridShader.SetMatrix4("model", _testCam.GetModelMatrix());
            _gridShader.SetMatrix4("view", _testCam.GetViewMatrix());
            _gridShader.SetMatrix4("projection", _testCam.GetProjectionMatrix());

            GL.BindVertexArray(_vaoBoundary);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 12);
            GL.BindVertexArray(_vaoGridLines);
            GL.DrawArrays(PrimitiveType.Lines, 0, (_plotGrid.Tiles + _plotGrid.Tiles - 2) * 2 * 3);

            _plotShader.Use();

            _plotShader.SetMatrix4("model", _testCam.GetModelMatrix());
            _plotShader.SetMatrix4("view", _testCam.GetViewMatrix());
            _plotShader.SetMatrix4("projection", _testCam.GetProjectionMatrix());

            GL.BindVertexArray(_vaoPlotMesh);
            //GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Point);
            //GL.PointSize(5f);
            GL.DrawElements(PrimitiveType.Triangles, _plotMesh.Triangles.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (!IsFocused)
            {
                //   return;
            }

            Title = $"FPS: {1 / (float) e.Time}";

            _timer += (float) e.Time;
            if (_timer > 1 / 8f)
            {
                //UpdatePlotBuffer(_timer);
                UpdatePlotBufferCompute(_timer);
            }
            
            _testCam.UpdateCamera(e, KeyboardState, MouseState);
            
            //
            // #region InputControls
            //
            // var input = KeyboardState;
            //
            // if (input.IsKeyDown(Keys.Escape))
            // {
            //     Close();
            // }
            //
            // const float cameraSpeed = 1.5f;
            // const float sensitivity = 0.2f;
            //
            // if (input.IsKeyDown(Keys.E))
            // {
            //     _centeredCamera.ZAngle += 0.5f * (float) e.Time;
            // }
            //
            // if (input.IsKeyDown(Keys.Q))
            // {
            //     _centeredCamera.ZAngle -= 0.5f * (float) e.Time;
            // }
            //
            // if (input.IsKeyDown(Keys.R))
            // {
            //     _centeredCamera.ResetAngles();
            // }
            //
            // if (input.IsKeyReleased(Keys.F))
            // {
            //     _centeredCamera.IsOrthographic = !_centeredCamera.IsOrthographic;
            // }
            //
            // if (input.IsKeyDown(Keys.W))
            // {
            //     _flyingCamera.Position += _flyingCamera.Front * cameraSpeed * (float) e.Time; // Forward
            //     //_centeredCamera.XAngle += 0.5f * (float) e.Time;
            // }
            //
            // if (input.IsKeyDown(Keys.S))
            // {
            //     _flyingCamera.Position -= _flyingCamera.Front * cameraSpeed * (float) e.Time; // Backwards
            //     //_centeredCamera.XAngle -= 0.5f * (float) e.Time;
            // }
            //
            // if (input.IsKeyDown(Keys.A))
            // {
            //     _flyingCamera.Position -= _flyingCamera.Right * cameraSpeed * (float) e.Time; // Left
            //     _centeredCamera.YAngle += 0.5f * (float) e.Time;
            // }
            //
            // if (input.IsKeyDown(Keys.D))
            // {
            //     _flyingCamera.Position += _flyingCamera.Right * cameraSpeed * (float) e.Time; // Right
            //     _centeredCamera.YAngle -= 0.5f * (float) e.Time;
            // }
            //
            // if (input.IsKeyDown(Keys.Space))
            // {
            //     _flyingCamera.Position += _flyingCamera.Up * cameraSpeed * (float) e.Time; // Up
            // }
            //
            // if (input.IsKeyDown(Keys.LeftShift))
            // {
            //     _flyingCamera.Position -= _flyingCamera.Up * cameraSpeed * (float) e.Time; // Down
            // }
            //
            // // Get the mouse state
            // var mouse = MouseState;
            //
            // if (_firstMove) // This bool variable is initially set to true.
            // {
            //     _lastPos = new Vector2(mouse.X, mouse.Y);
            //     _firstMove = false;
            // }
            // else
            // {
            //     // Calculate the offset of the mouse position
            //     var deltaX = mouse.X - _lastPos.X;
            //     var deltaY = mouse.Y - _lastPos.Y;
            //     _lastPos = new Vector2(mouse.X, mouse.Y);
            //
            //     // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            //     _flyingCamera.Yaw += deltaX * sensitivity;
            //     _flyingCamera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            //
            //     if (mouse.IsAnyButtonDown)
            //     {
            //         _centeredCamera.YAngle += deltaX * sensitivity * (float) e.Time;
            //         _centeredCamera.ZAngle -= deltaY * sensitivity * (float) e.Time;
            //     }
            // }
            //
            // #endregion
        }


        private unsafe void UpdatePlotBufferCompute(float timer)
        {
            // For some reason buffer sub data does not work
            //  GL.BufferSubData
            //  (
            //      BufferTarget.ShaderStorageBuffer,
            //      IntPtr.Zero,
            //      sizeof(float) * PlotConfig.MeshResX * PlotConfig.MeshResZ * 3,
            //      IntPtr.Zero
            //  );

            //GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, _computeBuffer);
            GL.BufferData
            (
                BufferTarget.ShaderStorageBuffer,
                sizeof(float) * PlotConfig.MeshResX * PlotConfig.MeshResZ * 3,
                IntPtr.Zero,
                BufferUsageHint.DynamicRead
            );
            
            _meshComputeShader.Use();
            _meshComputeShader.SetFloat("xSpan", PlotConfig.PlotXSpan);
            _meshComputeShader.SetFloat("zSpan", PlotConfig.PlotZSpan);
            _meshComputeShader.SetFloat("timer", timer);
            _meshComputeShader.Dispatch(PlotConfig.MeshResX, PlotConfig.MeshResZ);
            _meshComputeShader.Wait();

            var dataPtr = (float*) GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly);
            //var vertices = new float[PlotConfig.MeshResX * PlotConfig.MeshResZ];
            //Marshal.Copy((IntPtr) dataPtr, vertices, 0, vertices.Length);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboPlotMesh);
            GL.BufferSubData
            (
                BufferTarget.ArrayBuffer,
                IntPtr.Zero,
                sizeof(float) * PlotConfig.MeshResX * PlotConfig.MeshResZ * 3,
                (IntPtr) dataPtr
            );
        }

        private void UpdatePlotBuffer(float timer)
        {
            _plotMesh.ConstructMesh(timer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboPlotMesh);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _plotMesh.Vertices.Length * sizeof(float) * 3,
                _plotMesh.Vertices);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (e.OffsetY > 0)
                _centeredCamera.Position -= (_centeredCamera.Center - _centeredCamera.Position) * 0.1f;
            else
                _centeredCamera.Position += (_centeredCamera.Center - _centeredCamera.Position) * 0.1f;

            base.OnMouseWheel(e);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
        }
    }
}