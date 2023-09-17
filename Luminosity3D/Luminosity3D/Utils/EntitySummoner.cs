using Luminosity3D.Builtin;
using Luminosity3D.EntityComponentSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Utils
{
    public static class EntitySummoner
    {
        public static Entity CreatePBREntity(string entName, string filePath)
        {
            var ent = new Entity(entName);

            ent.AddComponent(new TransformComponent(new OpenTK.Mathematics.Vector3(0, 0, 0), new OpenTK.Mathematics.Vector3(0, 0, 0), new OpenTK.Mathematics.Vector3(1, 1, 1)));
            ent.AddComponent(new MeshBatch(filePath));


            return ent;
        }
    }
}
