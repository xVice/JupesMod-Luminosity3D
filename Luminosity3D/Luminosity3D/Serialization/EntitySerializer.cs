using Luminosity3D.EntityComponentSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Serialization
{
    public class SerializedEntity
    {
        public string ParentName { get; set; }
        public string[] ChildNames { get; set; }
        public string Name { get; set; }
        public int ExecutionOrder { get; set; }
        public List<Component> Components { get; set; }
    }

    

    public class EntitySerializer
    {



    }
}
