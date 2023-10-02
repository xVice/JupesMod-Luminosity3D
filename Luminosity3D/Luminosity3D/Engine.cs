using Luminosity3D.Builtin.RenderLayers;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.PKGLoader;
using Luminosity3D.Utils;
using Luminosity3DRendering;
using Luminosity3DScening;

namespace Luminosity3D
{
    public static class Engine
    {
        public static SceneManager SceneManager = new SceneManager();
        public static PackageLoader PackageLoader = new PackageLoader();
        public static Renderer Renderer;
        public static DebugConsole Console { get => GetConsole(); }

        public static void StartEngine()
        {

            Logger.ClearLogFile();


            using (Renderer renderer = new Renderer(1280, 780, "Jupe's Mod v0.0.5"))
            {
                double fps = 60;
                Renderer = renderer;
                renderer.Run();

            }

        }

        public static DebugConsole GetConsole()
        {
            if(Renderer != null)
            {
                return Renderer.Console;
            }

            return null;
        }


        //Add global searching shit here that is slow.
    }
}