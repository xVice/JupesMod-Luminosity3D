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

            // Manually add the required TransformComponent
            ent.AddComponent<TransformComponent>();

            ent.AddComponent<MeshBatch>();

            return ent;
        }

        public static Entity CreateCamera(string entName, Vector3 pos, bool setActive)
        {
            var cament = new Entity(entName); // Create a new entity with the given name.
            cament.AddComponent<TransformComponent>(); // Add a TransformComponent to the entity.
            var cam = cament.AddComponent<Camera>(); // Attach a Camera script to the entity.
            cament.AddComponent<CameraController>(); // Add a CameraController to the entity.
            if (setActive)
            {
                cam.SetActive();
            }

            return cament; // Return the created entity.
        }

        public static Entity CreatePBREntityWithRb(string entName, string filePath, Vector3 pos)
        {
            var ent = new Entity(entName);

            // Manually add the required TransformComponent
            ent.AddComponent<TransformComponent>();
            var batch = ent.AddComponent<MeshBatch>();
            ent.AddComponent(ColliderComponent.BuildFromMesh(batch));
            ent.AddComponent<RigidBodyComponent>();


            return ent;
        }

        public static Entity CreatePBREntityWithRbConvexHull(string entName, string filePath, Vector3 pos)
        {
            var ent = new Entity(entName);
            ent.AddComponent<TransformComponent>();
            ent.AddComponent<MeshBatch>();
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
