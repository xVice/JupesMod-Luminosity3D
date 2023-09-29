using Luminosity3D.Utils;
using Luminosity3D;
using OpenTK.ImGui;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using Luminosity3D.Builtin.RenderLayers;
using Luminosity3D.Builtin;
using Luminosity3DScening;
using System.Drawing;
using System.Drawing.Imaging;
using Luminosity3D.EntityComponentSystem;
using System.Reflection;
using Assimp;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;
using static System.Net.Mime.MediaTypeNames;

namespace Luminosity3DRendering
{
    public class Renderer : GameWindow
    {
        public Renderer(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
            IMGUIController = new ImGuiController(this);
            CenterWindow(new Vector2i(1280, 720));
        }

        public ImGuiController? IMGUIController = null;

        public List<IRenderLayer> renderLayers = new List<IRenderLayer>();
        public Engine Engine { get => Engine.Instance; }
        public DebugConsole Console;

        public enum RenderLayerType
        {
            ImGui,
            GLRender,
            Entity,
            HTML // Add more types as needed
        }



        
        protected override void OnLoad()
        {
            base.OnLoad();
            var timer = new Stopwatch();
            timer.Start();

            Console = new DebugConsole(this);
            AddLayer(Console);

            Logger.Log("Starting Jupe's Mod..");
            Logger.Log("Deleting loaded lupk files..");
            Engine.PackageLoader.UnloadPaks();

            Logger.Log("Loading lupks..");

            Engine.PackageLoader.LoadPaks();
            timer.Stop();

            Logger.Log($"Jupe's Mod Loaded in {timer.ElapsedMilliseconds / 1000}sec, press any key to exit..");
            Engine.Awake();
            Engine.Start();

            
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

            GL.ClearColor(new Color4(0, 32, 48, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            //Put all this into render layers, or a single renderlayer, so its easier to layer stuff below certain objects.
            var meshBatches = Engine.FindComponents<MeshBatch>();

            Bus.Send<MeshBatch>(x => x.OnRender());
            //Engine.InvokeFunction<MeshBatch>(x => x.OnRender()); // might work better for this case

            if (renderLayers.Count != 0)
            {
                for (int i = renderLayers.Count() - 1; i >= 0; i--)
                {
                    var renderLayer = renderLayers[i];
                    renderLayer.Render();
                }
            }

            if(KeyboardState.IsKeyPressed(Keys.F5))
            {
                isgrabbed = !isgrabbed;
                if (isgrabbed)
                {
                    CursorState = CursorState.Grabbed;
                }
                else
                {
                    CursorState = CursorState.Normal;
                }
            }

            IMGUIController.Render();

            SwapBuffers();

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // Calculate delta time and assign it to Engine.DeltaTime
            Engine.DeltaTime = (float)e.Time;


            Engine.Update();

            IMGUIController.Update(this, (float)e.Time);
        }


        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            IMGUIController.MouseScroll(e.Offset);


        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);

            
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
