using System;

namespace OpenTK__Plotter
{
    public class PlotGrid
    {

        private readonly float[] _boundaries;
        private readonly float[] _gridLines;


        private readonly int _tiles;

        public float XSpan { get; private set; }
        public float ZSpan { get; private set; }
        public float YSpan { get; private set; }

        public float[] Boundaries => _boundaries;
        public float[] GridLines => _gridLines;
        public int Tiles => _tiles;

        public PlotGrid(float xSpan, float zSpan, float ySpan, int tileCount)
        {
            _tiles = tileCount;
            
            XSpan = xSpan;
            ZSpan = zSpan;
            YSpan = ySpan;

            float[] zxBoundary = Array.Empty<float>();
            float[] zxGridLines = Array.Empty<float>();
            
            float[] yxBoundary = Array.Empty<float>();
            float[] yxGridLines = Array.Empty<float>();
            
            float[] zyBoundary = Array.Empty<float>();
            float[] zyGridLines = Array.Empty<float>();

            GenerateZxBoundary(ref zxBoundary);
            GenerateZxGridLines(ref zxGridLines);
            GenerateYxBoundary(ref yxBoundary);
            GenerateYxGridLines(ref yxGridLines);
            GenerateZyBoundary(ref zyBoundary);
            GenerateZyGridLines(ref zyGridLines);
            
            _boundaries = new float[zxBoundary.Length + yxBoundary.Length + zyBoundary.Length];
            zxBoundary.CopyTo(_boundaries, 0);
            yxBoundary.CopyTo(_boundaries, zxBoundary.Length);
            zyBoundary.CopyTo(_boundaries, zxBoundary.Length + yxBoundary.Length);

            _gridLines = new float[zxGridLines.Length + yxGridLines.Length + zyGridLines.Length];
            zxGridLines.CopyTo(_gridLines, 0);
            yxGridLines.CopyTo(_gridLines, zxGridLines.Length);
            zyGridLines.CopyTo(_gridLines, zxGridLines.Length + zyGridLines.Length);
        }

        private void GenerateZxBoundary(ref float[] zxBoundary)
        {
            //Generate ZX boundary
            zxBoundary = new float[3 * 4];
            int s = 0;
            // o---x
            // |   |
            // x---x
            zxBoundary[s++] = 0; //X
            zxBoundary[s++] = 0; //Y
            zxBoundary[s++] = 0; //Z
            // x---o
            // |   |
            // x---x
            zxBoundary[s++] = XSpan; //X
            zxBoundary[s++] = 0;     //Y
            zxBoundary[s++] = 0;     //Z
            // x---x
            // |   |    
            // x---o
            zxBoundary[s++] = XSpan; //X
            zxBoundary[s++] = 0; //Y
            zxBoundary[s++] = ZSpan; //Z
            // x---x
            // |   |
            // o---x
            zxBoundary[s++] = 0; //X
            zxBoundary[s++] = 0; //Y
            zxBoundary[s++] = ZSpan; //Z
        }

        private void GenerateZxGridLines(ref float[] zxGridLines)
        {
            //Generate ZX grid lines
            zxGridLines = new float[(_tiles - 1 + _tiles - 1) * 2 * 3];
            int s = 0;
            float step = XSpan / _tiles;
            //Generate grid lines parallel to Z axis
            for (int i = 1; i < _tiles; i++)
            {
                //First vertex of line
                zxGridLines[s++] = i * step; //X
                zxGridLines[s++] = 0; //Y
                zxGridLines[s++] = 0; //Z

                //Second vertex of line
                zxGridLines[s++] = i * step; //X
                zxGridLines[s++] = 0; //Y
                zxGridLines[s++] = ZSpan; //Z
            }

            //Generate grid lines parallel to X axis
            step = ZSpan / _tiles;
            for (int i = 1; i < _tiles; i++)
            {
                //First vertex of line
                zxGridLines[s++] = 0; //X
                zxGridLines[s++] = 0; //Y
                zxGridLines[s++] = i * step; //Z

                //Second vertex of line
                zxGridLines[s++] = XSpan; //X
                zxGridLines[s++] = 0; //Y
                zxGridLines[s++] = i * step; //Z
            }
        }

        private void GenerateYxBoundary(ref float[] yxBoundary)
        {
            //Generate YX boundary
            yxBoundary = new float[3 * 4];
            int s = 0;
            // x---x
            // |   |
            // o---x
            yxBoundary[s++] = 0; //X
            yxBoundary[s++] = 0; //Y
            yxBoundary[s++] = 0; //Z
            // o---x
            // |   |
            // x---x
            yxBoundary[s++] = 0; //X
            yxBoundary[s++] = YSpan; //Y
            yxBoundary[s++] = 0;
            // x---o
            // |   |
            // x---x
            yxBoundary[s++] = XSpan; //X
            yxBoundary[s++] = YSpan; //Y
            yxBoundary[s++] = 0; //Z
            // x---x
            // |   |
            // x---o
            yxBoundary[s++] = XSpan; //X
            yxBoundary[s++] = 0; //Y
            yxBoundary[s++] = 0; //Z
        }

        private void GenerateYxGridLines(ref float[] yxGridLines)
        {
            //Generate YX grid lines
            yxGridLines = new float[(_tiles - 1 + _tiles - 1) * 2 * 3];
            int s = 0;
            float step = XSpan / _tiles;
            //Generate grid lines parallel to Y axis
            for (int i = 1; i < _tiles; i++)
            {
                //First vertex of line
                yxGridLines[s++] = i * step; //X
                yxGridLines[s++] = 0; //Y
                yxGridLines[s++] = 0; //Z
                
                //Second vertex of line
                yxGridLines[s++] = i * step; //X
                yxGridLines[s++] = YSpan; //Y
                yxGridLines[s++] = 0; //Z
            }
            //Generate grid lines parallel to X axis
            step = YSpan / _tiles;
            for (int i = 1; i < _tiles; i++)
            {
                //First vertex of line
                yxGridLines[s++] = 0; //X
                yxGridLines[s++] = i * step; //Y
                yxGridLines[s++] = 0; //Z
                
                //Second vertex of line
                yxGridLines[s++] = XSpan;
                yxGridLines[s++] = i * step;
                yxGridLines[s++] = 0;
            }
        }

        private void GenerateZyBoundary(ref float[] zyBoundary)
        {
            //Generate ZY boundary
            zyBoundary = new float[3 * 4];
            int s = 0;
            // x---o
            // |   |
            // x---x
            zyBoundary[s++] = 0; //X
            zyBoundary[s++] = 0; //Y
            zyBoundary[s++] = 0; //Z
            // x---x
            // |   |
            // x---o
            zyBoundary[s++] = 0; //X
            zyBoundary[s++] = 0; //Y
            zyBoundary[s++] = ZSpan;
            // x---x
            // |   |
            // o---x
            zyBoundary[s++] = 0; //X
            zyBoundary[s++] = YSpan; //Y
            zyBoundary[s++] = ZSpan; //Z
            // o---x
            // |   |
            // x---x
            zyBoundary[s++] = 0; //X
            zyBoundary[s++] = YSpan; //Y
            zyBoundary[s++] = 0; //Z
        }

        private void GenerateZyGridLines(ref float[] zyGridLines)
        {
            //Generate ZY grid lines
            zyGridLines = new float[(_tiles - 1 + _tiles - 1) * 2 * 3];
            int s = 0;
            float step = ZSpan / _tiles;
            //Generate grid lines parallel to Y axis
            for (int i = 1; i < _tiles; i++)
            {
                //First vertex of line
                zyGridLines[s++] = 0; //X
                zyGridLines[s++] = 0; //Y
                zyGridLines[s++] = i * step; //Z
                
                //Second vertex of line
                zyGridLines[s++] = 0; //X
                zyGridLines[s++] = YSpan; //Y
                zyGridLines[s++] = i * step; //Z
            }
            //Generate grid lines parallel to Z axis
            step = YSpan / _tiles;
            for (int i = 1; i < _tiles; i++)
            {
                //First vertex of line
                zyGridLines[s++] = 0; //X
                zyGridLines[s++] = i * step; //Y
                zyGridLines[s++] = 0; //Z
                
                //Second vertex of line
                zyGridLines[s++] = 0;
                zyGridLines[s++] = i * step;
                zyGridLines[s++] = ZSpan;
            }
        }
    }
}