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
using Assimp;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;
using System.Drawing.Imaging;
using System.Drawing;
using System.Reflection;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using Material = Assimp.Material;
using Luminosity3D.Rendering;
using Assimp.Configs;
using BepuPhysics.Collidables;
using Mesh = Assimp.Mesh;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using Camera = Luminosity3D.Builtin.Camera;

namespace Luminosity3DRendering
{
    public static class AssimpCache
    {
        public static Dictionary<string, AssimpModel> Cache = new Dictionary<string, AssimpModel>();


        public static void CacheModel(string fileName, AssimpModel model)
        {
            Cache.Add(fileName, model);
        }

        public static bool HasModel(string fileName)
        {
            return Cache.ContainsKey(fileName);
        }

        public static AssimpModel Get(string fileName)
        {
            if (Cache.ContainsKey(fileName))
            {
                return Cache[fileName];
            }
            var model = new AssimpModel(fileName);
            Cache[fileName] = model;
            return model;
        }
    }

    public class AssimpModel
    {
        public List<MeshModel> Meshes = new List<MeshModel>();

        private Scene scene;

        public AssimpModel(string filePath)
        {
            AssimpContext importer = new AssimpContext();
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            scene = importer.ImportFile(filePath, PostProcessPreset.TargetRealTimeMaximumQuality);
            ConstructModelNode(scene.RootNode);
            
        }


        private void ConstructModelNode(Node node)
        {

            foreach (int index in node.MeshIndices)
            {

                var mesh = scene.Meshes[index];
                var mat = scene.Materials[mesh.MaterialIndex];
                if (!ShaderCache.HasShaderForMat(mat))
                {
                    var shader = Shader.BuildFromMaterialPBR(mat);
                    Logger.Log($"Build Shader for material (name if any): {mat.Name}");
                    ShaderCache.CacheShader(mat, shader);
                }
                Meshes.Add(new MeshModel(scene, mesh));
            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                ConstructModelNode(node.Children[i]);
            }

        }
    }

    public class MeshModel
    {
        public Scene Scene { get; set; }

        public Mesh Mesh { get; set; }
        public Shader Shader { get; set; }

        private int VAO;

        private int vboVertices;
        private int vboNormals;
        private int vboTexCoords;
        
        private int ebo;

        private uint[] Indicies;
        public float[] Vertices { get; private set; }
        public float[] Normals { get; private set; }
        public float[] TexCoords { get; private set; }

        public MeshModel(Scene scene,Mesh mesh)
        {
            Scene = scene;
            Mesh = mesh;
            var material = scene.Materials[mesh.MaterialIndex];
            Shader = ShaderCache.Get(material);
            Vertices = LMath.Vector3DListToFloatArray(mesh.Vertices);
            Normals = LMath.Vector3DListToFloatArray(mesh.Normals);
            TexCoords = LMath.Vector3DListToFloatArray(mesh.TextureCoordinateChannels[0]);
            Indicies = mesh.GetUnsignedIndices();
            SetupBuffers();
        }

        public void Bind()
        {
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboVertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboNormals);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
        }

        public void Unbind()
        {
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.UseProgram(0);
        }

        public void Cleanup()
        {
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(vboVertices);
            GL.DeleteBuffer(vboNormals);
            GL.DeleteBuffer(vboTexCoords);
            GL.DeleteBuffer(ebo);
        }
        private void SetupBuffers()
        {
            try
            {
                Logger.Log("In buffers creation");
                // Create a VAO and bind it
                GL.GenVertexArrays(1, out VAO);
                GL.BindVertexArray(VAO);
                LGLE.CheckGLError("VAO creation");

                // Create and bind a VBO for vertex positions
                GL.GenBuffers(1, out vboVertices);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboVertices);
                GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                LGLE.CheckGLError("VBO for vertices");

                // Create and bind a VBO for normals if needed
                GL.GenBuffers(1, out vboNormals);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboNormals);
                GL.BufferData(BufferTarget.ArrayBuffer, Normals.Length * sizeof(float), Normals, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
                LGLE.CheckGLError("VBO for normals");

                // Create and bind a VBO for texture coordinates if needed
                GL.GenBuffers(1, out vboTexCoords);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
                GL.BufferData(BufferTarget.ArrayBuffer, TexCoords.Length * sizeof(float), TexCoords, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);
                LGLE.CheckGLError("VBO for texCoords");

                // Create and bind an EBO for indices
                GL.GenBuffers(1, out ebo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, Indicies.Length * sizeof(uint), Indicies, BufferUsageHint.StaticDraw);
                LGLE.CheckGLError("EBO for indices");

                // Unbind the VAO (not the VBOs or EBO)
                GL.BindVertexArray(0);
                LGLE.CheckGLError("VAO unbinding");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SetupVAO: " + ex.Message);
            }
        }




        



    }

    public static class LGLE
    {
        public static void CheckGLError(string location, Exception ex = null)
        {
            ErrorCode errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                if(ex != null)
                {
                    Logger.LogToFile($"OpenGL Error at {location}: {errorCode}");
                    throw new Exception($"OpenGL Error at {location}: {errorCode}");
                }
                else
                {
                    Logger.LogToFile($"OpenGL Error at {location}: {errorCode}");
                    Logger.LogToFile($"OpenGL appended exception: {ex.ToString()}");
                    throw new Exception($"OpenGL Error at {location}: {errorCode}");
                }

            }
        }
        public static Vector3 FromVector(Vector3D vec)
        {
            Vector3 v;
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
            return v;
        }

        public static Vector4 ToVector4(Color4D col)
        {
            return new Vector4(col.R, col.G, col.B, col.A);
        }

        public static Color4 FromColor(Color4D color)
        {
            Color4 c;
            c.R = color.R;
            c.G = color.G;
            c.B = color.B;
            c.A = color.A;
            return c;
        }

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

            var meshBatches = Engine.SceneManager.ActiveScene.cache.GetComponents<MeshBatch>();

            if(Engine.SceneManager.ActiveScene.activeCam != null)
            {
                var cam = Engine.SceneManager.ActiveScene.activeCam.GetComponent<Camera>();
                var viewMatrix = LMath.ToMatTk(cam.ViewMatrix);
                var projectionMatrix = LMath.ToMatTk(cam.ProjectionMatrix);
                var viewPos = LMath.ToVecTk(cam.Position);


                foreach (var model in AssimpCache.Cache.Values)
                {
                    foreach (var batch in meshBatches.Where(x => x.model == model))
                    {
                        var transform = batch.GetComponent<TransformComponent>();
                        foreach (var mesh in model.Meshes)
                        {
                            try
                            {
                                var shader = mesh.Shader;
                                mesh.Bind();
                                shader.Use();
                                shader.SetUniform("modelMatrix", LMath.ToMatTk(transform.GetTransformMatrix()));
                                shader.SetUniform("viewMatrix", viewMatrix);
                                shader.SetUniform("projectionMatrix", projectionMatrix);
                                shader.SetUniform("viewPos", viewPos);
                                shader.SetUniform("lightColor", new Vector3(1f, 1f, 1f));
                                shader.SetUniform("objectColor", new Vector3(0.25f, 0.1f, 0.5f));
                                shader.SetUniform("lightPos", new Vector3(10.0f, 10.0f, 10.0f));

                                // Render the object conditionally based on occlusion query
                                GL.DrawElements(PrimitiveType.Triangles, mesh.Mesh.FaceCount * 3, DrawElementsType.UnsignedInt, 0);
                                mesh.Unbind();
                            }
                            catch (Exception ex)
                            {
                                LGLE.CheckGLError("Render Exception", ex);
                            }
                        }

                    }
                }
            }

           

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
