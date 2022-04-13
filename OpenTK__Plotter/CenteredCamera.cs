using OpenTK.Mathematics;

namespace OpenTK__Plotter
{
    public class CenteredCamera
    {
        public bool IsOrthographic { get; set; } = false;

        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        private float _fov = MathHelper.DegreesToRadians(45f);

        public CenteredCamera(Vector3 center, float aspectRatio)
        {
            Position = new Vector3(center.X, 0, center.Z) * 3;
            Center = center;
            AspectRatio = aspectRatio;
        }

        public Vector3 Center { get; set; }
        public Vector3 Position { get; set; }
        public float AspectRatio { get; set; }

        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;

        //public float XAngle { get; set; }
        public float YAngle { get; set; }

        private float _zAngle;

        public float ZAngle
        {
            get => _zAngle;
            set => _zAngle = MathHelper.Clamp(value, -MathHelper.PiOver2, MathHelper.PiOver2);
        }

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 45f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public void ResetAngles()
        {
            //XAngle = 0;
            YAngle = 0;
            ZAngle = 0;
        }

        public Matrix4 GetViewMatrix()
        {
            var pseudoZAxis = Vector3.Cross(_up, Center - Position);
            var quaternion = Quaternion.FromAxisAngle(pseudoZAxis, ZAngle);

            return Matrix4.CreateTranslation(-Center)
                   //* Matrix4.CreateRotationX(XAngle)
                   * Matrix4.CreateRotationY(YAngle)
                   * Matrix4.CreateFromQuaternion(quaternion)
                   * Matrix4.CreateTranslation(Center)
                   * Matrix4.LookAt(Position, Center, _up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            if (IsOrthographic)
                return Matrix4.CreateOrthographic(20, 20, -100, 100);
            else
                return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }
    }
}