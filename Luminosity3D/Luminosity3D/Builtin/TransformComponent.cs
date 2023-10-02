using ImGuiNET;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using OpenTK.Mathematics;
using System.Numerics;
using Vector3 = System.Numerics.Vector3;
using Quaternion = System.Numerics.Quaternion;

namespace Luminosity3D.Builtin
{
    public class TransformComponent : Component, IImguiSerialize
    {
        public Vector3 Position = Vector3.Zero;
        public Quaternion Rotation = Quaternion.Identity;
        public Vector3 Scale = Vector3.One;

        public static Component OnEditorCreation(Entity ent)
        {
            /*
            ImGui.InputFloat3("Position", ref Position);
            
            ImGui.InputFloat3("Scale", ref Scale);
            */
            return new TransformComponent(new Vector3(0,0,0));

        }

        public void EditorUI()
        {
            ImGui.InputFloat3("Position", ref Position);
            // You can use quaternion input or Euler angles here as per your preference.
            //ImGui.InputFloat4("Rotation", ref Rotation.X);
            ImGui.InputFloat3("Scale", ref Scale);
        }

        public void LookAt()
        {
            var cam = Engine.FindComponents<Camera>().FirstOrDefault();

            if (cam != null)
            {
                cam.LookAt(Position);
            }
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

        public TransformComponent(Vector3 Position) 
        {
            this.Position = Position;
            base.ExecutionOrder = 1;
        }

        public Matrix4 GetTransformMatrix()
        {
            var translationMatrix = Matrix4.CreateTranslation(LMath.ToVecTk(Position));
            var rotationMatrix = Matrix4.CreateFromQuaternion(LMath.ToQuatTk(Rotation)); // Use quaternion rotation
            var scaleMatrix = Matrix4.CreateScale(LMath.ToVecTk(Scale));
            return scaleMatrix * rotationMatrix * translationMatrix;
        }
    }
}
