using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenTK__Plotter
{
    public class ComputeShader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public ComputeShader(string path)
        {
            var shaderSource = File.ReadAllText(path);

            var computeShader = GL.CreateShader(ShaderType.ComputeShader);

            GL.ShaderSource(computeShader, shaderSource);

            CompileShader(computeShader);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, computeShader);

            LinkProgram(Handle);

            GL.DetachShader(Handle, computeShader);
            GL.DeleteShader(computeShader);

            GL.GetProgram(Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            _uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(Handle, i, out _, out _);

                var location = GL.GetUniformLocation(Handle, key);

                _uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            // Try to compile the shader
            GL.CompileShader(shader);

            // Check for compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int) All.True)
            {
                // We can use `GL.GetShaderInfoLog(shader)` to get information about the error.
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            // We link the program
            GL.LinkProgram(program);

            // Check for linking errors
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int) All.True)
            {
                // We can use `GL.GetProgramInfoLog(program)` to get information about the error.
                var log = GL.GetProgramInfoLog(program);
                throw new Exception($"Error occurred whilst linking Program({program})\n\n{log}");
            }
        }


        public void Use()
        {
            GL.UseProgram(Handle);
        }

        public void Dispatch(int x = 1, int y = 1, int z = 1)
        {
            GL.DispatchCompute(x, y, z);
        }

        public void Wait()
        {
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(Handle, attribName);
        }

        public void SetInt(string name, int data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        public void SetFloat(string name, float data)
        {
            GL.UseProgram(Handle);
            GL.Uniform1(_uniformLocations[name], data);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(Handle);
            GL.UniformMatrix4(_uniformLocations[name], true, ref data);
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(Handle);
            GL.Uniform3(_uniformLocations[name], data);
        }
    }
}