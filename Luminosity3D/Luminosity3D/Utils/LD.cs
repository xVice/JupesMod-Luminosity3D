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
        public static Engine Engine { get => Engine.Instance; }

        public static List<Engine> EnginePool = new List<Engine>();

        public static Engine StartEngine()
        {
            var engine = new Engine("Luminosity Launcher");
            var scene = new Scene("Demo Scene");

            engine.SceneManager.AddScene(scene);
            scene.Load();

            engine.StartEngine();

            EnginePool.Add(engine);
            return engine;
        }

        public static Engine FindEngine(string engineName)
        {
            return EnginePool.Where(x => x.EngineName == engineName).First();
        }



    }
}
