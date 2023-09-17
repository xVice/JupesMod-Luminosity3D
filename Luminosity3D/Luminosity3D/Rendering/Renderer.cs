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
        
        public int LoadTexture(string path)
        {
            Bitmap bitmap = new Bitmap(path);

            // Generate a texture ID
            int textureId;
            GL.GenTextures(1, out textureId);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            // Prepare the image data and upload it to the GPU
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                              ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                          OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);

            // Set texture parameters (optional)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            return textureId;
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

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
          
            GL.ClearColor(new Color4(0, 32, 48, 255));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            List<MeshBatch> meshBatches = null;
            if(Engine.FindComponents<MeshBatch>().Count != 0)
            {
                meshBatches = Engine.FindComponents<MeshBatch>();
            }

            if(meshBatches != null)
            {
                //use active cam instead here.
                Entity cam = null;
                if (Engine.FindComponents<CameraController>().FirstOrDefault() != null)
                {
                    cam = Engine.FindComponents<CameraController>().First().Entity;
                }


                Matrix4 viewMatrix = cam.GetComponent<Camera>().ViewMatrix;
                Matrix4 projectionMatrix = cam.GetComponent<Camera>().ProjectionMatrix;

                foreach (var meshBatch in meshBatches)
                {
                    var transformComponent = meshBatch.Entity.GetComponent<TransformComponent>();

                    if (transformComponent != null)
                    {
                        // Calculate the model matrix
                        Matrix4 modelMatrix = transformComponent.GetTransformMatrix() * Matrix4.Identity;

                        // Calculate the Model-View-Projection (MVP) matrix
                        Matrix4 MVP = modelMatrix * viewMatrix * projectionMatrix;

                        foreach (var mesh in meshBatch.meshes)
                        {
                            var shader = mesh.shader;
                            shader.SetUniform("mvpMatrix", MVP);
                            shader.SetUniform("modelMatrix", modelMatrix);
                        }


                        meshBatch.OnRender();
                    }

               
                }
            }
            



            if (renderLayers.Count != 0)
            {
                for (int i = renderLayers.Count() - 1; i >= 0; i--)

                {
                    var renderLayer = renderLayers[i];
                    renderLayer.Render();
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
