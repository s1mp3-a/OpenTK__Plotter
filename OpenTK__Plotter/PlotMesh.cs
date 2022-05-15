using System;
using OpenTK.Mathematics;

namespace OpenTK__Plotter
{
    public class PlotMesh
    {
        private Vector3[] _vertices;
        private uint[] _triangles;

        private readonly float _xSpan;
        private readonly float _ySpan;
        private readonly float _zSpan;
        private readonly int _xRes;
        private readonly int _zRes;

        public Vector3[] Vertices => _vertices;
        public uint[] Triangles => _triangles;

        public PlotMesh(float xSpan, float ySpan, float zSpan, int xRes, int zRes)
        {
            this._xSpan = xSpan;
            this._ySpan = ySpan;
            this._zSpan = zSpan;
            this._xRes = xRes;
            this._zRes = zRes;
        }

        public void ConstructMesh(float timer = 0f)
        {
            var rnd = new Random();

            _vertices = new Vector3[(_xRes) * (_zRes)];
            _triangles = new uint[(_xRes - 1) * (_zRes - 1) * 6];

            var ySize = _ySpan * 0.9f;
            var yOffset = _ySpan - ySize;

            int triIndex = 0;
            for (int z = 0; z < _zRes; z++)
            {
                for (int x = 0; x < _xRes; x++)
                {
                    int idx = x + z * _xRes;

                    float vx = x * _xSpan / (_xRes - 1);
                    float vz = z * _zSpan / (_zRes - 1);
                    float vy = Function(timer, rnd, vx, vz);
                    vy = MathHelper.MapRange(vy, 0, 1, 0, _ySpan);
                    _vertices[idx] = new Vector3(vx, vy, vz);

                    //Calculate triangle indices
                    if (x != _xRes - 1 && z != _zRes - 1)
                    {
                        _triangles[triIndex++] = (uint) idx;
                        _triangles[triIndex++] = (uint) (idx + _xRes + 1);
                        _triangles[triIndex++] = (uint) (idx + _xRes);

                        _triangles[triIndex++] = (uint) idx;
                        _triangles[triIndex++] = (uint) (idx + 1);
                        _triangles[triIndex++] = (uint) (idx + _xRes + 1);
                    }
                }
            }
        }

        private float Function(float s, Random rnd, float vx, float vz)
        {
            float zOff = 6 * MathF.Cos(s) + 8;
            float xOff = 2 * MathF.Sin(s) + 4;

            float x = 1 - MathF.Pow(MathF.Abs(2 * (vx - xOff) * 16 / 9 / 5f), 3f);
            x = MathHelper.Clamp(x, 0, 1);

            float z = 1 - MathF.Pow(MathF.Abs(2 * (vz - zOff) * 9 / 16 / 2f), 1.5f);
            z = MathHelper.Clamp(z, 0, 1);

            return MathF.Pow(x, 1 / 3f) * MathF.Pow(z, 1 / 1.5f);
        }
    }
}