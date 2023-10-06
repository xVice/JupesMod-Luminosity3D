using Luminosity3D.Builtin;
using Luminosity3D.EntityComponentSystem;
using System.Numerics;

namespace Luminosity3D.Utils
{
    public static class EntitySummoner
    {
        public static void CreatePBREntity(string entName, string filePath, Vector3 pos)
        {
            var ent = new GameObject(entName);
            ent.AddComponent(MeshBatch.FromPath(filePath));

        }

        public static void CreateCamera(string entName, Vector3 pos, bool setActive)
        {
            var cament = new GameObject(entName); // Create a new entity with the given name.

            cament.AddComponent<CameraController>(); // Add a CameraController to the entity.


        }

        public static void CreatePBREntityWithRb(string entName, string filePath, Vector3 pos)
        {
            var ent = new GameObject(entName);

            var batch = ent.AddComponent(MeshBatch.FromPath(filePath));
            ent.AddComponent<RigidBodyComponent>();


   
        }

        public static void CreatePBREntityWithRbConvexHull(string entName, string filePath, Vector3 pos)
        {
            var ent = new GameObject(entName);
            ent.AddComponent(MeshBatch.FromPath(filePath));
            ent.AddComponent<RigidBodyComponent>();

        }

        public static void CreateFPSController(string entName, string filePath, Vector3 pos)
        {
            var ent = new GameObject(entName);


        }

        public static void CreatePBREntityWithRbAndSine(string entName, string filePath, Vector3 pos)
        {
            var ent = new GameObject(entName);
            ent.AddComponent(MeshBatch.FromPath(filePath));
            ent.AddComponent<SineMovement>();

        }





    }
}
