using Luminosity3D.Builtin.RenderLayers;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.PKGLoader;
using Luminosity3D.Utils;
using Luminosity3DRendering;
using Luminosity3DScening;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Luminosity3D
{
    public class Engine
    {
        public SceneManager SceneManager { get; private set; } = new SceneManager();
        public PackageLoader PackageLoader { get; set; } = new PackageLoader();
        public Renderer Renderer { get; set; } = null;

        public DebugConsole? Console { get => GetConsole(); }

        public KeyboardState KeyboardState { get => Renderer.KeyboardState; }

        public double deltaTime { get; set; }
        public double time { get; set; }
        public double timeScale { get; set; }
        public string EngineName { get; private set; }
        public static Engine Instance { get; private set; }
        public bool isRunning = false;

        public Engine(string EngineName)
        {
            this.EngineName = EngineName;
            MakeCurrent();
            
        }

        public DebugConsole GetConsole()
        {
            if(Renderer != null)
            {
                return Renderer.Console;
            }

            return null;
        }

        /// <summary>
        /// Sets the static instance to the instanc this function was called on, used for rendering.
        /// </summary>
        public void MakeCurrent()
        {
            Engine.Instance = this;   
        }

        public void StartEngine()
        {
            Logger.ClearLogFile();
            using (Renderer renderer = new Renderer(800, 600, "Jupe's Mod v1.0.0"))
            {
                this.Renderer = renderer;
                renderer.Run();
            
            }   
        }

        public void Start()
        {
            foreach (var ent in SceneManager.ActiveScene.Entities.GetContent())
            {
                ent.Start();
            }
        }

        public void Awake()
        {
            foreach (var ent in SceneManager.ActiveScene.Entities.GetContent())
            {
                ent.Awake();
            }
        }

        public void Update()
        {
            foreach(var ent in SceneManager.ActiveScene.Entities.GetContent())
            {
                ent.Update();
            }
        }

        public void Resume()
        {

        }
        
        public void Pause()
        {

        }

        public void StopEngine()
        {
            PackageLoader.UnloadPaks();
        }


        public List<Entity> FindObjectsOfType<T>() where T : Component
        {
            return SceneManager.ActiveScene.FindObjectsOfType<T>();
        }

        public List<Component> GetComponents<T>(Entity ent) where T : Component
        {
            return ent.GetComponents<T>();
        }

        public Component GetComponent<T>(Entity ent) where T : Component
        {
            return ent.GetComponent<T>();
        }

        public Entity FindEntity(string name)
        {
            return SceneManager.ActiveScene.FindEntity(name);
        }

    }
}