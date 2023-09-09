using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Utils
{
    public static class Time
    {
        public static double deltaTime { get => Engine.Instance.deltaTime; set => Engine.Instance.deltaTime = value; }
        public static double time { get => Engine.Instance.time; set => Engine.Instance.time = value; }
        public static double timeScale { get => Engine.Instance.timeScale; set => Engine.Instance.timeScale = value; }
    }
}
