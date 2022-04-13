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

            _vertices = new Vector3[_xRes * _zRes];
            _triangles = new uint[(_xRes - 1) * (_zRes - 1) * 6];

            var ySize = _ySpan * 0.9f; //Leave 10% bottom gap
            var yOffset = _ySpan - ySize;

            int triIndex = 0;
            for (int z = 1; z < _zRes; z++)
            {
                for (int x = 1; x < _xRes; x++)
                {
                    int idx = x + z * _xRes;

                    float s = 1 + MathF.Cos(timer) * 3; //TODO

                    float vx = x * _xSpan / _xRes;
                    float vz = z * _zSpan / _zRes;
                    //float vy = MathHelper.MapRange(s/5*MathF.Sin(s*vx + MathF.Cos(vz)), -1f, 1f, 0, _ySpan);
                    //float vy = yOffset + s/5 * MathF.Sin(s * vx + MathF.Cos(vz));
                    //float vy = MathHelper.MapRange(MathF.Cos(s*vx) * MathF.Sin(vz), -1f, 1f, yOffset, ySize);
                    float vy =DoTheThing(s, rnd, vx, vz);
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

        private float DoTheThing(float s, Random rnd, float vx, float vz)
        {
            float x = 1 - MathF.Pow(MathF.Abs(2 * (vx - s*s / 16 * 9) / (1.5f)), 1.5f);
            x = MathHelper.Clamp(x, 0, 1);
            float z = 1 - MathF.Pow(MathF.Abs(2 * (vz - s*s) / (5f)), 3f);
            z = MathHelper.Clamp(z, 0, 1);

            return 3 * MathF.Pow(x, 1 / 1.5f) * MathF.Pow(z, 1 / 3f);
        }
    }
}