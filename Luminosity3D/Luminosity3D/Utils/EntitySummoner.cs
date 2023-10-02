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

            ent.AddComponent(new TransformComponent(pos));
            ent.AddComponent(new MeshBatch(filePath));

            return ent;
        }

        public static Entity CreatePBREntityWithRb(string entName, string filePath, Vector3 pos)
        {
            var ent = new Entity(entName);

            ent.AddComponent(new TransformComponent(pos));
            var batch = ent.AddComponent(new MeshBatch(filePath));
            var collider = ent.AddComponent(ColliderComponent.BuildFromMesh(batch));
            ent.AddComponent(new RigidBodyComponent(collider));

            return ent;
        }

        public static Entity CreatePBREntityWithRbConvexHull(string entName, string filePath, Vector3 pos)
        {
            var ent = new Entity(entName);

            ent.AddComponent(new TransformComponent(pos));
            var batch = ent.AddComponent(new MeshBatch(filePath));
            var collider = ent.AddComponent(ColliderComponent.BuildConvexHull(batch));
            ent.AddComponent(new RigidBodyComponent(collider));

            return ent;
        }

        public static Entity CreateFPSController(string entName, string filePath, Vector3 pos)
        {
            var ent = new Entity(entName);

            ent.AddComponent(new TransformComponent(pos));
            var collider = ent.AddComponent(ColliderComponent.BuildSphere(5f));
            ent.AddComponent(new MeshBatch(filePath));
            ent.AddComponent(new RigidBodyComponent(collider));
            ent.AddComponent<Camera>();
            ent.AddComponent(new FPSController());

            return ent;
        }

        public static Entity CreatePBREntityWithRbAndSine(string entName, string filePath, Vector3 pos)
        {
            var ent = new Entity(entName);

            ent.AddComponent(new TransformComponent(pos));
            ent.AddComponent<SineMovement>(new SineMovement());
            var batch = ent.AddComponent(new MeshBatch(filePath));
            var collider = ent.AddComponent(ColliderComponent.BuildFromMesh(batch));
            ent.AddComponent(new RigidBodyComponent(collider));

            return ent;
        }

        public static Entity CreateCamera(string entName, Vector3 pos, bool setActive)
        {
            var cament = new Entity(entName);
            cament.AddComponent<TransformComponent>(new TransformComponent(pos));
            cament.AddComponent<Camera>();
            cament.AddComponent<CameraController>();
            if (setActive)
            {
                cament.GetComponent<Camera>().SetActive();
            }

            return cament;
        }


    }
}
