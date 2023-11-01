using Luminosity3D.Builtin;
using Luminosity3D.EntityComponentSystem;
using Luminosity3DScening;
using System.Numerics;

namespace Luminosity3D.Utils
{
    public static class EntitySummoner
    {
        public static GameObject CreatePBREntity(string entName, string filePath, Vector3 pos)
        {
            var ent = new GameObject(entName);

            ent.AddComponent(MeshBatch.FromPath(filePath));
            return ent;
        }

        public static void CreateCamera(string entName, Vector3 pos, bool setActive)
        {
            var cament = new GameObject(entName); // Create a new entity with the given name.

            cament.AddComponent<CameraController>(); // Add a CameraController to the entity.


        }

        public static void CreatePBREntityWithRb(string entName, string filePath, Vector3 pos)
        {
            var ent = new GameObject(entName);

            ent.AddComponent(MeshBatch.FromPath(filePath));
            ent.AddComponent<RigidBodyComponent>();



        }

        public static void CreatePBREntityWithRbStatic(string entName, string filePath, Vector3 pos)
        {
            var ent = new GameObject(entName);
            var trans = ent.AddComponent<TransformComponent>();
            //trans.Position -= Vector3.UnitY * 25;
           
            var batch = ent.AddComponent(MeshBatch.FromPath(filePath));
            ent.AddComponent<ColliderComponent>();


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
            var trans = ent.AddComponent<TransformComponent>();
            trans.Scale = new Vector3(0.1f, 0.1f, 0.1f);
            trans.Position = SceneManager.ActiveScene.activeCam.Position;
            ent.AddComponent(MeshBatch.FromPath("./resources/tr_phoenix/scene.gltf"));
            ent.AddComponent<FPSController>();

        }

        public static void CreatePBREntityWithRbAndSine(string entName, string filePath, Vector3 pos)
        {
            var ent = new GameObject(entName);
            ent.AddComponent(MeshBatch.FromPath(filePath));
            ent.AddComponent<SineMovement>();

        }





    }
}
