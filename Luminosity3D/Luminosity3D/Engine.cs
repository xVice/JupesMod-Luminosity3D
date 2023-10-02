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

        //Reallllllly slow
        public static void InvokeFunction<T>(Action<T> function) where T : Component
        {
            var scene = SceneManager.ActiveScene;

            var sortedList = scene.Entities.OrderBy(x => x.ExecutionOrder);

            if (sortedList.Count() != 0)
            {
                for (int i = sortedList.Count() - 1; i >= 0; i--)
                {
                    var ent = sortedList.ElementAt(i);
                    foreach(var comp in ent.Components)
                    {
                        if (comp is T typedComponent)
                        {
                            function(typedComponent);
                        }
                    }
                }
            }
        }

        public static void Update()
        {
            var scene = SceneManager.ActiveScene;

            var sortedList = scene.Entities.OrderBy(x => x.ExecutionOrder);


            if (sortedList.Count() != 0)
            {
                for (int i = sortedList.Count() - 1; i >= 0; i--)
                {
                    var ent = sortedList.ElementAt(i);
                    ent.EarlyUpdate();
                    ent.Update();
                    ent.LateUpdate();
                }
            }
        }

        public static void StopEngine()
        {
            PackageLoader.UnloadPaks();
        }

        //Add global searching shit here that is slow.

        public static List<T> FindComponents<T>() where T : Component
        {
            return SceneManager.ActiveScene.FindObjectsOfType<T>();
        }
    }
}