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
    public unsafe class TransformComponent : LuminosityBehaviour, IImguiSerialize
    {
        [SerializeField] Matrix4x4 transformMatrix = Matrix4x4.Identity;
        [SerializeField] bool drawGizmo = false;

        public Vector3 Position
        {
            get { return transformMatrix.Translation; }
            set
            {
                transformMatrix.Translation = value;
            }
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
            Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, LMath.ToRadians(-90));
        }

        public void EditorUI()
        {
            
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
    }
}
