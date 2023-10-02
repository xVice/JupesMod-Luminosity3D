using Luminosity3D.EntityComponentSystem;
using System.Numerics;
using System;
using OpenTK.Mathematics;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;
using Luminosity3D.Utils;

namespace Luminosity3D.Builtin
{
    [RequireComponent(typeof(TransformComponent))]
    public class Camera : LuminosityBehaviour
    {
        private TransformComponent transform = null;
        private float MoveSpeed = 2.5f;
        private float MouseSensitivity = 0.1f;

        public Vector3 Position = Vector3.Zero;

        public Matrix4 ViewMatrix { get; private set; }
        public Matrix4 ProjectionMatrix { get; private set; }
        public Quaternion Orientation { get; set; } = Quaternion.Identity;
        public float FieldOfView { get; set; }
        public float AspectRatio { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }
        public Vector3 Forward
        {
            get
            {
                // Calculate the forward vector based on the camera's orientation
                return -Vector3.Transform(Vector3.UnitZ, Orientation);
            }
        }
        public Vector3 Right
        {
            get
            {
                Vector3 forward = -Vector3.Transform(Vector3.UnitZ, Orientation);
                Vector3 up = Vector3.Transform(Vector3.UnitY, Orientation);
                return Vector3.Cross(up, forward);
            }
        }


        public override void Awake()
        {
            transform = GetComponent<TransformComponent>();

            FieldOfView = 90;
            AspectRatio = 1920 / 1080;
            NearClip = 0.1f;
            FarClip = 1000f;
            Orientation = Quaternion.Identity;
            SetActive();
            UpdateProjectionMatrix();
            UpdateViewMatrix();
        }

        public void UpdateProjectionMatrix()
        {
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(FieldOfView),
                AspectRatio,
                NearClip,
                FarClip
            );
        }

        public void UpdateViewMatrix()
        {
            ViewMatrix = Matrix4.LookAt(LMath.ToVecTk(Position), LMath.ToVecTk(Position) - LMath.ToVecTk(Vector3.Transform(Vector3.UnitZ, Orientation)), LMath.ToVecTk(Vector3.Transform(Vector3.UnitY, Orientation)));
        }

        public void SetActive()
        {
            Engine.SceneManager.ActiveScene.activeCam = this;
        }

        public void Move(Vector3 direction, float deltaTime)
        {
            Position += direction * MoveSpeed * deltaTime;
            UpdateViewMatrix();
        }

        public void RotateCamera(Vector3 orientation, float angle)
        {
            // Create a quaternion to represent the rotation
            Quaternion rotation = Quaternion.CreateFromAxisAngle(orientation, angle);

            // Apply the rotation to the camera's orientation
            Orientation = rotation * Orientation;

            // Normalize the camera's orientation to ensure it remains a unit quaternion
            //Orientation.Normalize();

            UpdateViewMatrix();
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Clamp(value, min, max);
        }

        public void LookAt(Vector3 target)
        {

           
        }

    }
}
