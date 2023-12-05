using Ceras;
using Luminosity3D.Builtin.RenderLayers;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.PKGLoader;
using Luminosity3D.Utils;
using Luminosity3DRendering;
using Luminosity3DScening;

namespace Luminosity3D
{
    public class JModVersionInfo
    {
        public const string TitleString = "Jupe's Mod v0.0.6";
    }

    
    public static class Engine
    {
        public static SceneManager SceneManager = new SceneManager();
        public static PackageLoader PackageLoader = new PackageLoader();
        public static Renderer Renderer;
        public static DebugConsole Console { get => GetConsole(); }
        private static CerasSerializer ceras = new CerasSerializer();

        public static class Directorys
        {
            public static string Mods = "./mods";
            public static string ModsAutoLoad = "/autoload";
            public static string ModsLoaded = "/loaded";
            public static string ProjectsPath = "./projects";
            public static string LogsPath = "./logs";
            public static string ResourcesPath = "./resources";
            public static string ScenesPath = "./scenes";
            public static string TempPath = "./temp";
            
        }

        public static CerasSerializer GetSerializer()
        {
            ceras.GetConfig().PreserveReferences = true;
            
            return ceras;
        }

        public static void StartEngine()
        {
            Logger.SetupFolder();


            using (Renderer renderer = new Renderer(1280, 780, JModVersionInfo.TitleString))
            {
                Renderer = renderer;
                renderer.Run();
            }

        }

        public static void StopEngine()
        {
            Renderer.Close();








        }

        public static DebugConsole GetConsole()
        {
            if(Renderer != null)
            {
                return Renderer.Console;
            }

            return null;
        }


    }
}