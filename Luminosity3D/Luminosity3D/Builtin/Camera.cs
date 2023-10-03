using Assimp;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using System;
using System.Numerics; // Import System.Numerics namespace
using Matrix4x4 = System.Numerics.Matrix4x4;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Luminosity3D.Builtin
{
    public class Camera : LuminosityBehaviour
    {
        public Quaternion Rotation = Quaternion.Identity; // Store camera's rotation separately
        public Vector3 Position = Vector3.Zero;
        public Matrix4x4 ViewMatrix { get; private set; }
        public Matrix4x4 ProjectionMatrix { get; private set; }
        public Vector3 Forward
        {
            get
            {
                // Calculate the forward vector based on the camera's orientation
                return -Vector3.Transform(Vector3.UnitZ, Rotation);
            }
        }
        public Vector3 Right
        {
            get
            {
                Vector3 forward = -Vector3.Transform(Vector3.UnitZ, Rotation);
                Vector3 up = Vector3.Transform(Vector3.UnitY, Rotation);
                return Vector3.Cross(up, forward);
            }
        }

        public Vector3 Up
        {
            get
            {
                return Vector3.Transform(Vector3.UnitY, Rotation);
            }
        }


        public float FieldOfView { get; set; }
        public float AspectRatio { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }

        public override void Awake()
        {

            FieldOfView = 90;
            AspectRatio = 1920f / 1080f;
            NearClip = 0.1f;
            FarClip = 1000f;

            SetActive();
            UpdateProjectionMatrix();
            UpdateViewMatrix();
        }

        public void UpdateProjectionMatrix()
        {
            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                LMath.ToRadians(FieldOfView), // Use System.Numerics MathHelper
                AspectRatio,
                NearClip,
                FarClip
            );
        }

        public void UpdateViewMatrix()
        {
            Matrix4x4 rotationMatrix = Matrix4x4.CreateFromQuaternion(Rotation);

            // Calculate the forward, right, and up vectors using the rotation matrix



            // Calculate the target position using the camera's Position and Forward vector
            Vector3 target = Position + Forward;

            // Create the view matrix using the calculated vectors
            ViewMatrix = Matrix4x4.CreateLookAt(Position, target, Up);
        }



        public void SetActive()
        {
            Engine.SceneManager.ActiveScene.activeCam = this;
        }

        public void Move(Vector3 direction, float speed)
        {
            //Vector3 worldDirection = Vector3.Transform(direction, Rotation); // Use camera's rotation
            Position += direction * speed * Time.deltaTime;
            UpdateViewMatrix();
        }

   


        public void RotateCamera(Vector3 orientation, float angle)
        {
            // Create a quaternion representing the rotation
            Quaternion rotation = Quaternion.CreateFromAxisAngle(orientation, angle);

            // Apply the rotation to the current camera rotation
            Quaternion newRotation = rotation * Rotation;

            newRotation = Quaternion.Normalize(newRotation);

            // Extract the pitch and yaw rotations from the new rotation
            Vector3 forwardVector = Vector3.Transform(Vector3.UnitZ, newRotation);
            float pitch = (float)Math.Asin(-forwardVector.Y);
            float yaw = (float)Math.Atan2(forwardVector.X, forwardVector.Z);

            // Clamp the pitch rotation to stay within a desired range (e.g., -80 to 80 degrees)
            float maxPitch = LMath.ToRadians(80); // Maximum pitch angle in radians
            pitch = LMath.Clamp(pitch, -maxPitch, maxPitch);

            // Reconstruct the new quaternion based on the clamped pitch and original yaw
            Quaternion clampedRotation = Quaternion.CreateFromYawPitchRoll(yaw, pitch, 0);

            // Update the camera's rotation
            Rotation = clampedRotation;
            // Update the view matrix
            UpdateViewMatrix();
        }





        public void LookAt(Vector3 target)
        {
            Vector3 direction = Vector3.Normalize(target - Position);
            Rotation = Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateLookAt(Vector3.Zero, direction, Vector3.UnitY));
            UpdateViewMatrix();
        }
    }
}
