using Luminosity3D.EntityComponentSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Utils
{
    public static class Bus
    {
        //Big slow but work good
        public static void Send<T>(Action<T> action) where T : Component
        {
            foreach(var component in Engine.Instance.FindComponents<T>())
            {
                action(component);
            }
        }
    }
}
