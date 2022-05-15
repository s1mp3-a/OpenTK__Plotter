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
        public const int GridTileCount = 6;
        public const float GridXSpan = 9f;
        public const float GridYSpan = 9f;
        public const float GridZSpan = 16f;

        public const float PlotXSpan = 9f;
        public const float PlotYSpan = 9f;
        public const float PlotZSpan = 16f;

        public const int MeshResX = 100;
        public const int MeshResZ = 100;

        public static readonly Color4 Background = new Color4(.2f, .3f, .4f, 1f);
    }

    public class PlotterWindow : GameWindow
    {
        //The grid of the plot space
        private readonly PlotGrid _plotGrid;
        
        //The mesh to render
        //It also contains indices that determine in which order to render triangles of the mesh
        private readonly PlotMesh _plotMesh;

        //Vertex array object of the boundary of the plot space
        private int _vaoBoundary;
        //Vertex buffer object of the boundary of the plot space
        private int _vboBoundary;

        //Vertex array object of the lines that make up tiles of the grid
        private int _vaoGridLines;
        //Vertex buffer object of these lines
        private int _vboGridLines;

        //Vertex array object of the mesh of the surface that is plotted
        private int _vaoPlotMesh;
        //Vertex buffer object of the mesh
        private int _vboPlotMesh;
        //Element buffer object that stores the indices of triangles of the plot mesh
        private int _eboPlotMesh;

        //The buffer that is used to compute the vertices of the plot mesh on a GPU
        private int _computeBuffer;

        //Cameras setup
        private FlyingCamera _flyingCamera;
        private CenteredCamera _centeredCamera;
        private Camera _activeCamera;

        //Render shader setup
        private Shader _gridShader;
        private Shader _plotShader;
        //Compute shader setup
        private ComputeShader _meshComputeShader;

        private float _timer;

        public PlotterWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _plotGrid = new PlotGrid(PlotConfig.GridXSpan,
                PlotConfig.GridZSpan,
                PlotConfig.GridYSpan,
                PlotConfig.GridTileCount);

            _plotMesh = new PlotMesh(PlotConfig.PlotXSpan,
                PlotConfig.PlotYSpan,
                PlotConfig.PlotZSpan,
                PlotConfig.MeshResX,
                PlotConfig.MeshResZ);

            _plotMesh.ConstructMesh();
            
            _flyingCamera = new FlyingCamera(Vector3.UnitZ * 3, Size.X / (float) Size.Y);
            var center = new Vector3(_plotGrid.XSpan / 2, _plotGrid.YSpan / 2, _plotGrid.ZSpan / 2);
            _centeredCamera = new CenteredCamera(center, Size.X / (float) Size.Y);

            _activeCamera = _centeredCamera;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            PrintOpenGlInfo();

            //The next stuff is set here instead of being initialized in a constructor so that there is
            //an existing openGL context to work with
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

            //Use the grid render pipeline
            _gridShader.Use();

            //Set model-view-projection uniforms in the render pipeline
            _gridShader.SetMatrix4("model", _activeCamera.GetModelMatrix());
            _gridShader.SetMatrix4("view", _activeCamera.GetViewMatrix());
            _gridShader.SetMatrix4("projection", _activeCamera.GetProjectionMatrix());

            //Draw the grid of the plot
            GL.BindVertexArray(_vaoBoundary);
            GL.DrawArrays(PrimitiveType.LineLoop, 0, 12);
            GL.BindVertexArray(_vaoGridLines);
            GL.DrawArrays(PrimitiveType.Lines, 0, (_plotGrid.Tiles + _plotGrid.Tiles - 2) * 2 * 3);

            //Use the surface render pipeline
            _plotShader.Use();

            //set model-view-projection uniforms in the render pipeline
            _plotShader.SetMatrix4("model", _activeCamera.GetModelMatrix());
            _plotShader.SetMatrix4("view", _activeCamera.GetViewMatrix());
            _plotShader.SetMatrix4("projection", _activeCamera.GetProjectionMatrix());

            //Draw surface mesh
            GL.BindVertexArray(_vaoPlotMesh);
            /*GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.PointSize(5f);*/
            GL.DrawElements(PrimitiveType.Triangles, _plotMesh.Triangles.Length, DrawElementsType.UnsignedInt, 0);

            //Swap the render and draw buffers (that's just how openGL works)
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            if (!IsFocused)
            {
                return;
            }

            if (IsKeyDown(Keys.Escape))
            {
                Close();
            }

            Title = $"FPS: {1 / (float) e.Time}";

            _timer += (float) e.Time;
            if (_timer > 1 / 8f)
            {
                //UpdatePlotBuffer(_timer);
                UpdatePlotBufferCompute(_timer);
            }
            
            _activeCamera.UpdateCamera(e, KeyboardState, MouseState);
        }


        //Surface mesh generation on a GPU
        private unsafe void UpdatePlotBufferCompute(float timer)
        {
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

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboPlotMesh);
            GL.BufferSubData
            (
                BufferTarget.ArrayBuffer,
                IntPtr.Zero,
                sizeof(float) * PlotConfig.MeshResX * PlotConfig.MeshResZ * 3,
                (IntPtr) dataPtr
            );
        }

        //Surface mesh generation on a CPU
        private void UpdatePlotBuffer(float timer)
        {
            _plotMesh.ConstructMesh(timer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboPlotMesh);
            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _plotMesh.Vertices.Length * sizeof(float) * 3,
                _plotMesh.Vertices);
        }

        //Set the viewport of a newly resized window
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, e.Width, e.Height);
        }
    }
}