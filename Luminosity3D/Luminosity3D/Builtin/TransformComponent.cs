using ImGuiNET;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using System.Numerics;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;
using Vector4 = System.Numerics.Vector4;

namespace Luminosity3D.Builtin
{
    public class TransformComponent : Component, IImguiSerialize
    {
        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = new Vector3(0.1f,0.1f,0.1f);

        public Vector3 Forward
        {
            get
            {
                return Vector3.Transform(Vector3.UnitZ, Rotation);
            }
        }

        public Vector3 Right
        {
            get
            {
                return Vector3.Transform(Vector3.UnitX, Rotation);
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

        public static Component OnEditorCreation()
        {
            return new TransformComponent();
        }

        public void EditorUI()
        {
            ImGui.InputFloat3("Position", ref Position);
            Vector4 quaternionValues = new Vector4( Rotation.X, Rotation.Y, Rotation.Z, Rotation.W );
            if (ImGui.InputFloat4("Rotation (Quaternion)", ref quaternionValues))
            {
                Rotation = new Quaternion(quaternionValues[0], quaternionValues[1], quaternionValues[2], quaternionValues[3]);
            }
            ImGui.InputFloat3("Scale", ref Scale);

            if(ImGui.Button("Look At"))
            {
                LookAt();
            }
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
