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

namespace Luminosity3DRendering
{
    public class Renderer : GameWindow
    {
        
        public enum RenderLayerType
        {
            ImGui,
            GLRender,
            Entity,
            HTML // Add more types as needed
        }

        public Renderer(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title }) {
            IMGUIController = new ImGuiController(this);
            CenterWindow(new Vector2i(1280, 720));
        }

        public ImGuiController? IMGUIController = null;

        public List<IRenderLayer> renderLayers = new List<IRenderLayer>();
        public Engine Engine { get => Engine.Instance; }
        public DebugConsole Console;

        public void AddLayer(IRenderLayer layer)
        {
            renderLayers.Add(layer);
        }

        protected override void OnRenderThreadStarted()
        {
            base.OnRenderThreadStarted();

        }

        protected override void OnLoad()
        {
            base.OnLoad();
            var timer = new Stopwatch();
            timer.Start();

            Console = new DebugConsole(this);
            AddLayer(Console);

            Logger.Log("Starting Jupe's Mod..");
            Engine.PackageLoader.UnloadPaks();

            Logger.Log("Loading lupks");

            Engine.PackageLoader.LoadPaks();
            timer.Stop();


            /*
            var ent = new Luminosity3D.EntityComponentSystem.Entity("Test3dobj");
            var comp = ent.AddComponent<MeshBatchComponent>(MeshBatchComponent.LoadFromFile("./teapot.obj"));
            Engine.Instance.SceneManager.ActiveScene.InstantiateEntity(ent);
            */
            Logger.Log($"Jupe's Mod Loaded in {timer.ElapsedMilliseconds / 1000}sec, press any key to exit..");

            Engine.Start();
      
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            IMGUIController.WindowResized(ClientSize.X, ClientSize.Y);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.ClearColor(new Color4(0, 32, 48, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            lock (renderLayers)
            {
                foreach (var renderLayer in renderLayers)
                {
                    renderLayer.Render();
                }

            }



            IMGUIController.Render();

            SwapBuffers();
        }


        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

         
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
                Environment.Exit(0);
            }

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
            //IMGUIController.PressChar((char)e.Unicode);

        }

    }
}
