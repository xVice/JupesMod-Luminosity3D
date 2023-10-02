using Luminosity3DScening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Utils
{
    public static class LD
    {
        public static void StartEngine()
        {
            var scene = new Scene("Demo Scene");

            Engine.SceneManager.AddScene(scene);
            scene.Load();

            Engine.StartEngine();
        }



    }
}
