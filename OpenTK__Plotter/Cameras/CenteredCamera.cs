using OpenTK.Mathematics;
using OpenTK__Plotter.Cameras.Camera_controls;

namespace OpenTK__Plotter.Cameras
{
    public class CenteredCamera : Camera
    {
        //Flag whether the resulting image is orthographic or perspective
        public bool IsOrthographic { get; set; } = false;

        // Those vectors are directions pointing outwards from the camera to define how it rotated
        private readonly Vector3 _front = -Vector3.UnitZ;
        private readonly Vector3 _up = Vector3.UnitY;
        private readonly Vector3 _right = Vector3.UnitX;

        //Filed of view of a perspective projection
        private float _fov = MathHelper.DegreesToRadians(45f);
        
        public CenteredCamera(Vector3 center, float aspectRatio)
            : base(new CenteredCameraControls())
        {
            Position = new Vector3(center.X, 0, center.Z) * 3;
            Center = center;
            AspectRatio = aspectRatio;
        }

        //Point in world coordinates about which the camera is rotated
        public Vector3 Center { get; set; }
        
        //Camera position in world coordinates
        public Vector3 Position { get; set; }
        
        // This is simply the aspect ratio of the viewport, used for the projection matrix.
        public float AspectRatio { get; set; }

        
        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;

        
        //Camera rotation about the Y axis (left - right)
        public float YAngle { get; set; }
        
        //Camera rotation about the Z axis (up - down)
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
            YAngle = 0;
            ZAngle = 0;
        }

        public override Matrix4 GetViewMatrix()
        {
            //We rotate about the axis parallel to the Z axis and passing through the center point
            //so that the plot space stays in the middle of the rotation
            var pseudoZAxis = Vector3.Cross(_up, Center - Position);
            var quaternion = Quaternion.FromAxisAngle(pseudoZAxis, ZAngle);

            return Matrix4.CreateTranslation(-Center)
                   * Matrix4.CreateRotationY(YAngle)
                   * Matrix4.CreateFromQuaternion(quaternion)
                   * Matrix4.CreateTranslation(Center)
                   * Matrix4.LookAt(Position, Center, _up);
        }

        public override Matrix4 GetProjectionMatrix()
        {
            //Return either orthographic or perspective projection depending on the flag state
            return IsOrthographic ? Matrix4.CreateOrthographic(20, 20, -100, 100)
                : Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }
    }
}