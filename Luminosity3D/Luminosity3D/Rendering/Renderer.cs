using Luminosity3D.Utils;
using Luminosity3D;
using OpenTK.ImGui;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using Luminosity3D.Builtin.RenderLayers;
using Luminosity3D.Builtin;
using ImGuiNET;
using OpenTK.Windowing.Common.Input;
using BulletSharp;
using Luminosity3D.EntityComponentSystem;

namespace Luminosity3DRendering
{

    public class Cache<T>
    {
        private List<T> cache = new List<T>();
        private readonly Func<T, object>[] sortingFunctions;

        public Cache(Func<T, object>[] sortingFunctions)
        {
            this.sortingFunctions = sortingFunctions;
        }

        public void Add(T item)
        {
            // Insert the item at the correct position based on sorting functions
            int index = cache.BinarySearch(item, new CacheItemComparer<T>(sortingFunctions));
            if (index < 0) index = ~index; // Adjust the index if not found
            cache.Insert(index, item);
        }

        public List<T> GetSortedCache()
        {
            return cache;
        }
    }

    public class CacheItemComparer<T> : IComparer<T>
    {
        private readonly Func<T, object>[] sortingFunctions;

        public CacheItemComparer(Func<T, object>[] sortingFunctions)
        {
            this.sortingFunctions = sortingFunctions;
        }

        public int Compare(T x, T y)
        {
            foreach (var func in sortingFunctions)
            {
                var result = Comparer<object>.Default.Compare(func(x), func(y));
                if (result != 0)
                {
                    return result;
                }
            }
            return 0;
        }
    }

    public class RenderCache
    {
        
    }


    public class Renderable
    {

    }


    public class Renderer : GameWindow
    {
        public DiscreteDynamicsWorld dynamicsWorld;


        public Renderer(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
            IMGUIController = new ImGuiController(this);
            ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;

            CenterWindow(new Vector2i(1280, 720));
        }

        public ImGuiController? IMGUIController = null;

        public List<IRenderLayer> renderLayers = new List<IRenderLayer>();
        public DebugConsole Console;

        public enum RenderLayerType
        {
            ImGui,
            GLRender,
            Entity,
            HTML // Add more types as needed
        }

        //I really do be loading a texture now though!
        public static WindowIcon CreateWindowIcon()
        {
            var imagePath = "./icons/jmodicon.png"; // Specify the correct image path

            using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imagePath))
            {
                var imageBytes = new byte[image.Width * image.Height * 4];

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        var pixel = image[x, y];
                        var index = (y * image.Width + x) * 4;

                        imageBytes[index] = pixel.R;
                        imageBytes[index + 1] = pixel.G;
                        imageBytes[index + 2] = pixel.B;
                        imageBytes[index + 3] = pixel.A;
                    }
                }

                var windowIcon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(image.Width, image.Height, imageBytes));

                return windowIcon;
            }
        }


        protected override void OnLoad()
        {
            base.OnLoad();

            Icon = CreateWindowIcon();
            var timer = new Stopwatch();
            timer.Start();

            // Initialize BulletSharp components
            CollisionConfiguration collisionConfig = new DefaultCollisionConfiguration();
            CollisionDispatcher dispatcher = new CollisionDispatcher(collisionConfig);
            BroadphaseInterface broadphase = new DbvtBroadphase();
            ConstraintSolver solver = new SequentialImpulseConstraintSolver();

            dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, collisionConfig);
            dynamicsWorld.Gravity = new BulletSharp.Math.Vector3(0, -9.81f, 0); // Set the gravity
            
            // Create a large plane at -50
            CollisionShape groundShape = new StaticPlaneShape(new BulletSharp.Math.Vector3(0, 1, 0), -50);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(), groundShape);
            RigidBody groundBody = new RigidBody(rbInfo);
            dynamicsWorld.AddRigidBody(groundBody);

            Console = new DebugConsole(this);
            AddLayer(Console);

            Logger.Log("Starting Jupe's Mod..");
            Logger.Log("Deleting loaded lupk files..");
            Engine.PackageLoader.UnloadPaks();

            Logger.Log("Loading lupks..");

            Engine.PackageLoader.LoadPaks();
            timer.Stop();

            Logger.Log($"Jupe's Mod Loaded in {timer.ElapsedMilliseconds / 1000}sec, press any key to exit..");



        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            IMGUIController.WindowResized(ClientSize.X, ClientSize.Y);
        }

        bool isgrabbed = false;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.ClearColor(new Color4(0, 0, 0, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal); // Adjust the depth function as needed
            GL.Enable(EnableCap.Multisample);
            Engine.SceneManager.ActiveScene.cache.RenderPass();

            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

            if (renderLayers.Count != 0)
            {
                for (int i = renderLayers.Count() - 1; i >= 0; i--)
                {
                    var renderLayer = renderLayers[i];
                    renderLayer.Render();
                }
            }

            IMGUIController.Render();

            
            //Will be removed soon
            if (KeyboardState.IsKeyPressed(Keys.F5))
            {
                var cam = Engine.SceneManager.ActiveScene.activeCam.Parent.GetComponent<CameraController>();

                if (cam != null)
                {
                    isgrabbed = !isgrabbed;
                    if (isgrabbed)
                    {
                        cam.lockMovement = false;
                        CursorState = CursorState.Grabbed;
                    }
                    else
                    {
                        cam.lockMovement = true;
                        CursorState = CursorState.Normal;
                    }
                }

            }
            
            SwapBuffers();

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            // Calculate Delta Time (time between frames).
            float deltaTime = (float)e.Time;
            Time.deltaTime = deltaTime;
            Time.time += deltaTime * Time.timeScale;
            Engine.SceneManager.ActiveScene.cache.UpdatePass();
            dynamicsWorld.StepSimulation(Time.deltaTime);
            Engine.SceneManager.ActiveScene.cache.PhysicsPass();

            IMGUIController.Update(this, (float)e.Time);
        }

        public void AddLayer(IRenderLayer layer)
        {
            renderLayers.Add(layer);
        }

        protected override void OnRenderThreadStarted()
        {
            base.OnRenderThreadStarted();

        }
    }
}
