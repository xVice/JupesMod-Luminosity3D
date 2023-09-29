using Luminosity3D.Builtin;
using Luminosity3D.EntityComponentSystem;
using System.Numerics;

namespace Luminosity3D.Utils
{
    public static class EntitySummoner
    {
        public static Entity CreatePBREntity(string entName, string filePath, OpenTK.Mathematics.Vector3 pos)
        {
            var ent = new Entity(entName);

            ent.AddComponent(new TransformComponent(pos, new OpenTK.Mathematics.Vector3(0, 0, 0), new OpenTK.Mathematics.Vector3(1, 1, 1)));
            ent.AddComponent(new MeshBatch(filePath));

            return ent;
        }
    }
}
