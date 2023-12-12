using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using System.Numerics;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;
using Vector4 = System.Numerics.Vector4;
using ImGuiNET;
using OpenTK.Mathematics;

namespace Luminosity3D.Builtin
{
    public unsafe class TransformComponent : LuminosityBehaviour, Networkable, IImguiSerialize
    {

        public Matrix4x4 transformMatrix = Matrix4x4.Identity;
        [SerializeField] bool drawGizmo = false;

        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private Vector3 targetScale;

        // Interpolation speed for position and scale
        public float positionLerpSpeed = 1.0f;
        public float scaleLerpSpeed = 1.0f;

        // Interpolation speed for rotation (slerp)
        public float rotationSlerpSpeed = 1.0f;


        public Matrix4x4 ModelMatrix
        {
            get
            {
                // The model matrix is the transformation matrix itself
                return transformMatrix;
            }
        }

        public Matrix4x4? ViewMatrix
        {
            get
            {
                if(Matrix4x4.Invert(transformMatrix, out Matrix4x4 viewMatrix))
                {
                    return viewMatrix;
                }
                // The view matrix is the inverse of the transformation matrix
                return null;
            }
        }

        public Matrix4x4 ProjectionMatrix
        {
            get
            {
                // You need to define how the projection matrix is obtained
                // This might be a property or field in your class
                // For example, if you have a field named projectionMatrix:
                // return projectionMatrix;
                // Adjust the code according to how you handle the projection matrix.
                return Matrix4x4.Identity; // Placeholder, replace with actual logic
            }
        }


        public Vector3 Position
        {
            get { return transformMatrix.Translation; }
            set
            {
                transformMatrix.Translation = value;
            }
        }

        public Vector3 Velocity
        {
            get { return (Position - previousPosition); }
        }

        public Quaternion Rotation
        {
            get { return Quaternion.CreateFromRotationMatrix(transformMatrix); }
            set
            {
                // Extract the existing translation and scale from the transformMatrix.
                var translation = transformMatrix.Translation;
                var scale = new Vector3(transformMatrix.M11, transformMatrix.M22, transformMatrix.M33);

                // Create a new transformation matrix with the new rotation, existing translation, and scale.
                transformMatrix = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(value) * Matrix4x4.CreateTranslation(translation);
            }
        }

        public Vector3 Scale
        {
            get { return new Vector3(transformMatrix.M11, transformMatrix.M22, transformMatrix.M33); }
            set
            {
                transformMatrix.M11 = value.X;
                transformMatrix.M22 = value.Y;
                transformMatrix.M33 = value.Z;
            }
        }


        public Vector3 Forward
        {
            get { return Vector3.Transform(Vector3.UnitZ, Matrix4x4.CreateFromQuaternion(Rotation)); }
        }

        public Vector3 Right
        {
            get { return Vector3.Transform(Vector3.UnitX, Matrix4x4.CreateFromQuaternion(Rotation)); }
        }

        public Vector3 Up
        {
            get { return Vector3.Transform(Vector3.UnitY, Matrix4x4.CreateFromQuaternion(Rotation)); }
        }
        public void EditorUI()
        {
            // Get the current rotation in Euler angles
            Vector3 currentRotation = QuaternionToEulerAngles(Rotation);

            // Display and edit the three float fields for rotation

            var pos = Position;
            var sca = Scale;

            ImGui.InputFloat3("Position", ref pos);
            ImGui.InputFloat3("Scale", ref sca);
            ImGui.InputFloat3("Rotation", ref currentRotation);
            Position = pos;
            Scale = sca;

            // Update the rotation if it has changed
            Quaternion newRotation = EulerAnglesToQuaternion(currentRotation);
            if (!QuaternionApproximatelyEqual(newRotation, Rotation))
            {
                Rotation = newRotation;
            }
        }

        private Vector3 QuaternionToEulerAngles(Quaternion q)
        {
            Matrix4 rotationMatrix = Matrix4.CreateFromQuaternion(LMath.ToQuatTk(q));
            return new Vector3(
                (float)Math.Atan2(rotationMatrix.M32, rotationMatrix.M33),
                (float)Math.Asin(-rotationMatrix.M31),
                (float)Math.Atan2(rotationMatrix.M21, rotationMatrix.M11)
            );
        }

        private Quaternion EulerAnglesToQuaternion(Vector3 euler)
        {
            return Quaternion.CreateFromYawPitchRoll(euler.Y, euler.X, euler.Z);
        }

        private bool QuaternionApproximatelyEqual(Quaternion a, Quaternion b)
        {
            // You can define a small threshold for equality
            float epsilon = 0.0001f;
            return Math.Abs(a.X - b.X) < epsilon &&
                   Math.Abs(a.Y - b.Y) < epsilon &&
                   Math.Abs(a.Z - b.Z) < epsilon &&
                   Math.Abs(a.W - b.W) < epsilon;
        }

        public void LookAt(Vector3 target)
        {
            Rotation = Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateLookAt(Position, target, Vector3.UnitY));
        }

        public static LuminosityBehaviour OnEditorCreation()
        {
            return new TransformComponent();
        }

        public override void Awake()
        {
            //Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, LMath.ToRadians(-90));
        }
        private Vector3 previousPosition;
        public void Update()
        {
            // Calculate velocity
            Vector3 currentVelocity = (Position - previousPosition) / Time.deltaTime;

            // Store the current position for the next frame
            previousPosition = Position;

            // Interpolate position and scale
            Position = Vector3.Lerp(Position, targetPosition, positionLerpSpeed * Time.deltaTime);
            Scale = Vector3.Lerp(Scale, targetScale, scaleLerpSpeed * Time.deltaTime);

            // Interpolate rotation using slerp
            Rotation = Quaternion.Slerp(Rotation, targetRotation, rotationSlerpSpeed * Time.deltaTime);
        }


        public Vector3 LocalToWorldDirection(Vector3 localDirection)
        {
            return Vector3.Transform(localDirection, Rotation);
        }

        public void Translate(Vector3 translation)
        {
            Position += translation;
        }

        public Vector3 TransformPoint(Vector3 vertex)
        {
            return Vector3.Transform(vertex, transformMatrix);
        }

        public Matrix4x4 GetTransformMatrix()
        {
            return transformMatrix;
        }

        public void Net(GameObject go)
        {
            var transformComp = go.GetComponent<TransformComponent>();
            if (transformComp != null)
            {
                // Set the target values
                targetPosition = transformComp.Position;
                targetRotation = transformComp.Rotation;
                targetScale = transformComp.Scale;
            }
        }
    }
}
