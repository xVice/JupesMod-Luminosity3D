using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.EntityComponentSystem
{

    public abstract class Component
    {
        public int ExecutionOrder { get; set; } = 1;
        public string Name { get; set; } = "Component";
        public string Description { get; set; } = "A Components Description";
        public bool Enabled { get; set; } = true;

        [JsonIgnore]
        public GameObject Parent { get; set; } = null;

        public Component()
        {

        }
    }
}
