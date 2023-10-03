using Luminosity3D.Builtin;
using Luminosity3D.EntityComponentSystem;
using System.Numerics;

namespace Luminosity3D.Utils
{
    public static class EntitySummoner
    {
        public static Entity CreatePBREntity(string entName, string filePath, Vector3 pos)
        {
            var ent = new Entity(entName);

            ent.AddComponent(MeshBatch.FromPath(filePath));

            return ent;
        }

        public static Entity CreateCamera(string entName, Vector3 pos, bool setActive)
        {
            var cament = new Entity(entName); // Create a new entity with the given name.

            cament.AddComponent<Camera>();
            cament.AddComponent<CameraController>(); // Add a CameraController to the entity.

            return cament; // Return the created entity.
        }

        public static Entity CreatePBREntityWithRb(string entName, string filePath, Vector3 pos)
        {
            var ent = new Entity(entName);

            ent.AddComponent<TransformComponent>();
            var batch = ent.AddComponent(MeshBatch.FromPath(filePath));
            ent.AddComponent(ColliderComponent.BuildFromMesh(batch));
            ent.AddComponent<RigidBodyComponent>();


            return ent;
        }

        public static Entity CreatePBREntityWithRbConvexHull(string entName, string filePath, Vector3 pos)
        {
            var ent = new Entity(entName);
            ent.AddComponent<TransformComponent>();
            ent.AddComponent(MeshBatch.FromPath(filePath));
            ent.AddComponent<ColliderComponent>();
            ent.AddComponent<RigidBodyComponent>();


            return ent;
        }

        public static Entity CreateFPSController(string entName, string filePath, Vector3 pos)
        {
            var ent = new Entity(entName);

           
            return ent;
        }

        public static Entity CreatePBREntityWithRbAndSine(string entName, string filePath, Vector3 pos)
        {
            var ent = new Entity(entName);


            return ent;
        }





    }
}
