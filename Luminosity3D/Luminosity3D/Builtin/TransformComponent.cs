using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using System.Numerics;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;
using Vector4 = System.Numerics.Vector4;
using ImGuiSharp;
using OpenTK.Mathematics;

namespace Luminosity3D.Builtin
{
    public unsafe class TransformComponent : LuminosityBehaviour,IImguiSerialize
    {
        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = new Vector3(.1f,.1f,.1f);

        private bool DrawGizmo = false;

        public Vector3 Forward
        {
            get
            {
                return Vector3.Transform(Vector3.UnitX, Rotation);
            }
        }

        public Vector3 Right
        {
            get
            {
                return Vector3.Transform(Vector3.UnitZ, Rotation);
            }
        }

        public Vector3 Up
        {
            get
            {
                return Vector3.Transform(Vector3.UnitY, Rotation);
            }
        }

        public void LookAt()
        {
            Engine.SceneManager.ActiveScene.activeCam.LookAt(Position);
        }

        public static LuminosityBehaviour OnEditorCreation()
        {
            return new TransformComponent();
        }

        public override void Awake()
        {
            Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, LMath.ToRadians(-90));
        }

        public void EditorUI()
        {
            ImGui.InputFloat3("Position", ref Position);
            Vector3 eulerRotation = QuaternionToEulerAngles(Rotation);
            if (ImGui.InputFloat3("Rotation (Euler)", ref eulerRotation))
            {
                Rotation = EulerAnglesToQuaternion(eulerRotation);
            }
            ImGui.InputFloat3("Scale", ref Scale);

            if(ImGui.Button("Look At"))
            {
                LookAt();
            }

            if(ImGui.Button("Draw Gizmo"))
            {
                DrawGizmo = !DrawGizmo;
            }

            if (DrawGizmo)
            {
                var cam = Engine.SceneManager.ActiveScene.activeCam;
                if(cam != null)
                {
                    var view = LMath.Matrix4x4ToFloatPointer(cam.ViewMatrix);
                    var proj = LMath.Matrix4x4ToFloatPointer(cam.ProjectionMatrix);

                    //ImGuizmo.Enable(true);
                    //ImGuizmo.BeginFrame();
                    //ImGuizmo.Manipulate(view, proj, ImGuizmoOperation.Rotate, ImGuizmoMode.World, LMath.Matrix4x4ToFloatPointer(GetTransformMatrix()));
                    
                }
            }
        }

        public Vector3 QuaternionToEulerAngles(Quaternion quaternion)
        {
            float sinr_cosp = 2.0f * (quaternion.W * quaternion.X + quaternion.Y * quaternion.Z);
            float cosr_cosp = 1.0f - 2.0f * (quaternion.X * quaternion.X + quaternion.Y * quaternion.Y);
            float roll = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            float sinp = 2.0f * (quaternion.W * quaternion.Y - quaternion.Z * quaternion.X);
            float pitch;
            if (Math.Abs(sinp) >= 1)
            {
                pitch = (float)Math.CopySign(Math.PI / 2, sinp); // Use 90 degrees if out of range
            }
            else
            {
                pitch = (float)Math.Asin(sinp);
            }

            float siny_cosp = 2.0f * (quaternion.W * quaternion.Z + quaternion.X * quaternion.Y);
            float cosy_cosp = 1.0f - 2.0f * (quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z);
            float yaw = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return new Vector3(roll, pitch, yaw) * (180.0f / (float)Math.PI); // Convert radians to degrees
        }

        public Quaternion EulerAnglesToQuaternion(Vector3 euler)
        {
            float yaw = euler.Y * (float)Math.PI / 180.0f;
            float pitch = euler.X * (float)Math.PI / 180.0f;
            float roll = euler.Z * (float)Math.PI / 180.0f;

            float cy = (float)Math.Cos(yaw * 0.5f);
            float sy = (float)Math.Sin(yaw * 0.5f);
            float cp = (float)Math.Cos(pitch * 0.5f);
            float sp = (float)Math.Sin(pitch * 0.5f);
            float cr = (float)Math.Cos(roll * 0.5f);
            float sr = (float)Math.Sin(roll * 0.5f);

            Quaternion quaternion;
            quaternion.W = cr * cp * cy + sr * sp * sy;
            quaternion.X = sr * cp * cy - cr * sp * sy;
            quaternion.Y = cr * sp * cy + sr * cp * sy;
            quaternion.Z = cr * cp * sy - sr * sp * cy;

            return quaternion;
        }

        public Vector3 LocalToWorldDirection(Vector3 localDirection)
        {
            // Transform a local direction vector to world space using the rotation matrix
            return Vector3.Transform(localDirection, Rotation);
        }

        public void Translate(Vector3 translation)
        {
            Position += translation;
        }


        public Vector3 TransformPoint(Vector3 vertex)
        {
            // Transform the vertex from local space to world space
            Vector3 transformedVertex = vertex;
            transformedVertex = Vector3.Transform(vertex, Rotation); // Apply rotation
            transformedVertex *= Scale; // Apply scale
            transformedVertex += Position; // Apply translation
            return transformedVertex;
        }


        public Matrix4x4 GetTransformMatrix()
        {
            var translationMatrix = Matrix4x4.CreateTranslation(Position);
            var rotationMatrix = Matrix4x4.CreateFromQuaternion(Rotation); // Use quaternion rotation
            var scaleMatrix = Matrix4x4.CreateScale(Scale);
            return scaleMatrix * rotationMatrix * translationMatrix;
        }


    }
}
