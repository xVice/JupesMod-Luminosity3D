using Luminosity3D.EntityComponentSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Utils
{
    /// <summary>
    /// Used for storing entity in a cache to instantiate/clone into the engine, useful for inventorys for example
    /// 
    /// need to heavily rewrite the entire engine for this, starting: 20:50 - 19.09.23
    /// </summary>
    public class EntityManager
    {
        
        private List<Entity> Cache = new List<Entity>();

        public void CacheEntity(Entity ent)
        {
            Cache.Add(ent);
        }

        public Entity Get(string name)
        {
            return Cache.Where(x => x.Name == name).FirstOrDefault();
        }
    }
}
