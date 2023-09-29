using Assimp;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace Luminosity3D.Builtin
{


    //This is insanity, straight insanity, thats why i am rewriting it now.
    //
    //right now its looking way better, i didnt do any shading though c:
    //
    //its shit, need to rewrite :c
    //
    //kinda better still 50% shit, only 35% nowit j
    public class MeshModel
    {
        public string FilePath { get; set; }
        public Scene Scene { get; set; }
        private int vboVertices;
        private int vboNormals;
        private int vboTexCoords;
        private int ebo;
        private int VAO;
        public float[] Vertices { get; private set; }
        public float[] Normals { get; private set; }
        public float[] TexCoords { get; private set; }

        public MeshModel(string filePath, Scene scene = null)
        {
            FilePath = filePath;
            if (scene != null)
            {
                Scene = scene;
            }
            else
            {
                LoadScene();
            }
        }

        public Vector4 AssimpToVec(Color4D Color)
        {
            return new Vector4(Color.R, Color.G, Color.B, Color.A);
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
        }

        public void Render(Shader shader, Matrix4 viewMatrix, Matrix4 projectionMatrix, Matrix4 modelMatrix, Vector3 viewPos)
        {
            try
            {
                Bind();
                CheckGLError("Binding");

                // Set up shader uniforms
                shader.Use();
                shader.SetUniform("modelMatrix", modelMatrix);
                shader.SetUniform("viewMatrix", viewMatrix);
                shader.SetUniform("projectionMatrix", projectionMatrix);
                shader.SetUniform("viewPos", viewPos);
                shader.SetUniform("lightColor", new Vector3(1.0f, 1.0f, 1.0f));
                shader.SetUniform("objectColor", new Vector3(1.0f, 0.6f, 0.22f));
                shader.SetUniform("lightPos", new Vector3(10.0f, 10.0f, 10.0f));


                foreach (Assimp.Mesh mesh in Scene.Meshes)
                {
                    Material mat = Scene.Materials[mesh.MaterialIndex];
                    // Set up material properties in your shader
                    /*
                    shader.SetUniform("mat.ambient", AssimpToVec(mat.ColorAmbient));
                    shader.SetUniform("mat.diffuse", AssimpToVec(mat.ColorDiffuse));
                    shader.SetUniform("mat.specular", AssimpToVec(mat.ColorSpecular));
                    shader.SetUniform("mat.emissive", AssimpToVec(mat.ColorEmissive));
                    shader.SetUniform("mat.reflective", AssimpToVec(mat.ColorReflective));
                    shader.SetUniform("mat.transparent", AssimpToVec(mat.ColorTransparent));
                    shader.SetUniform("mat.bumpscaling", mat.BumpScaling);
                    shader.SetUniform("mat.shininess", mat.Shininess);
                    */
                    CheckGLError("Mat uniforms");
                    GL.DrawElements(PrimitiveType.Triangles, mesh.FaceCount * 3, DrawElementsType.UnsignedInt, 0);
                    CheckGLError("Drawing a mesh");
                }
                Unbind();
                GL.UseProgram(0);
                CheckGLError("Unbinding");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Render: " + ex.Message);
            }
        }

        private void LoadScene()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FilePath))
                {
                    throw new ArgumentException("File path cannot be empty.");
                }

                AssimpContext assimpContext = new AssimpContext();
                Scene = assimpContext.ImportFile(FilePath, PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals);

                if (Scene == null || Scene.RootNode == null)
                {
                    throw new InvalidOperationException("Failed to load the scene.");
                }

                List<Vector3D> vertices = new List<Vector3D>();
                List<Vector3D> normals = new List<Vector3D>();
                List<Vector3D> texCoords = new List<Vector3D>();
                List<uint> indices = new List<uint>(); // New list for indices

                foreach (Assimp.Mesh mesh in Scene.Meshes)
                {
                    foreach (Vector3D vertex in mesh.Vertices)
                    {
                        vertices.Add(vertex);
                    }

                    foreach (Vector3D normal in mesh.Normals)
                    {
                        normals.Add(normal);
                    }

                    foreach (Vector3D texCoord in mesh.TextureCoordinateChannels[0])
                    {
                        texCoords.Add(texCoord);
                    }

                    // Process indices
                    foreach (Face face in mesh.Faces)
                    {
                        if (face.IndexCount != 3) // Assuming triangles
                        {
                            throw new NotSupportedException("Only triangles are supported.");
                        }

                        indices.AddRange(face.Indices.Select(index => (uint)index));
                    }
                }

                // Convert the lists to arrays and set the properties
                Vertices = vertices.SelectMany(v => new float[] { (float)v.X, (float)v.Y, (float)v.Z }).ToArray();
                Normals = normals.SelectMany(n => new float[] { (float)n.X, (float)n.Y, (float)n.Z }).ToArray();
                TexCoords = texCoords.SelectMany(t => new float[] { (float)t.X, (float)t.Y }).ToArray();
                uint[] indicesArray = indices.ToArray(); // Convert the indices list to an array

                SetupVAO(Vertices, Normals, TexCoords, indicesArray); // Pass indices to SetupVAO
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in LoadScene: " + ex.Message);
            }
        }

        private void SetupVAO(float[] vertices, float[] normals, float[] texCoords, uint[] indices)
        {
            try
            {
                // Create a VAO and bind it
                GL.GenVertexArrays(1, out VAO);
                GL.BindVertexArray(VAO);
                CheckGLError("VAO creation");

                // Create and bind a VBO for vertex positions
                GL.GenBuffers(1, out vboVertices);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboVertices);
                GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                CheckGLError("VBO for vertices");

                // Create and bind a VBO for normals if needed
                GL.GenBuffers(1, out vboNormals);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboNormals);
                GL.BufferData(BufferTarget.ArrayBuffer, normals.Length * sizeof(float), normals, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
                CheckGLError("VBO for normals");

                // Create and bind a VBO for texture coordinates if needed
                GL.GenBuffers(1, out vboTexCoords);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
                GL.BufferData(BufferTarget.ArrayBuffer, texCoords.Length * sizeof(float), texCoords, BufferUsageHint.StaticDraw);
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);
                CheckGLError("VBO for texCoords");

                // Create and bind an EBO for indices
                GL.GenBuffers(1, out ebo);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);
                CheckGLError("EBO for indices");

                // Unbind the VAO (not the VBOs or EBO)
                GL.BindVertexArray(0);
                CheckGLError("VAO unbinding");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SetupVAO: " + ex.Message);
            }
        }

        private void CheckGLError(string location)
        {
            ErrorCode errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                throw new Exception($"OpenGL Error at {location}: {errorCode}");
            }
        }

        public void Cleanup()
        {
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(vboVertices);
            GL.DeleteBuffer(vboNormals);
            GL.DeleteBuffer(vboTexCoords);
            GL.DeleteBuffer(ebo);
        }
    }




    public class MeshBatch : Component, IRenderable
    {
        public string filePath = string.Empty;
        public MeshModel model = null;
        public Shader shader;

        public MeshBatch(string filePath)
        {
            var meshBatch = Compute(filePath);
            model = meshBatch;
            shader = new Shader("./shaders/builtin/pbr.vert", "./shaders/builtin/pbr.frag");
        }

        public MeshBatch(string filePath, string vert, string frag)
        {
            var meshBatch = Compute(filePath);
            model = meshBatch;
            shader = new Shader(vert, frag);
        }

        public MeshModel Compute(string filePath)
        {
            var cachedMesh = MeshCache.Get(filePath);
            if (cachedMesh != null)
            {
                Logger.Log("Using a cached mesh..");
                return cachedMesh;
            }

            Logger.Log("Loading a mesh from its file..");
            var meshModel = new MeshModel(filePath);
            MeshCache.Cache(meshModel);
            return meshModel;
        }

        public override void Start()
        {

        }

        public override void Update()
        {

        }

        public override void OnDestroy()
        {


        }

        public override void Awake()
        {
          
        }

        public override void EarlyUpdate()
        {
         
        }

        public override void LateUpdate()
        {
          
        }

        public override void OnEnable()
        {
          
        }

        public override void OnDisable()
        {
         
        }

        public void OnRender() // called by OnRenderFrame function from opentk rather then the OnUpdateFrame function. useful for .... rendering
        {
            var cam = Engine.FindComponents<Camera>().FirstOrDefault();
            if(cam != null)
            {
                var transform = GetComponent<TransformComponent>();
                if(transform != null)
                {
                    model.Render(shader, cam.ViewMatrix, cam.ProjectionMatrix, transform.GetTransformMatrix() , cam.Position);
                }
            }
        }
    }

    public interface IRenderable
    {
        public void OnRender();
    }

    public static class MeshCache
    {
        public static HashSet<MeshModel> MeshCacheSet = new HashSet<MeshModel>();

        public static MeshModel Get(string filePath)
        {
            return MeshCacheSet.Where(x => x.FilePath == filePath).FirstOrDefault();
        }

        public static HashSet<MeshModel> GetAllCaches()
        {
            return MeshCacheSet;
        }

        public static MeshModel Cache(MeshModel batch)
        {
            var cachedMesh = Get(batch.FilePath);
            if (cachedMesh != null)
            {
                return cachedMesh;
            }
            MeshCacheSet.Add(batch);
            return batch;
        }
    }

    public class Shader
    {

        public enum LumShaderType { Program, VertexShader, FragmentShader }
        public int ProgramId { get; private set; }

        public Shader(string vertexShaderPath, string fragmentShaderPath)
        {
            ProgramId = LoadShaderProgram(vertexShaderPath, fragmentShaderPath);
        }

        private int LoadShaderProgram(string vertexShaderPath, string fragmentShaderPath)
        {
            int vertexShader, fragmentShader, shaderProgram;

            // Compile vertex shader
            vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, File.ReadAllText(vertexShaderPath));
            GL.CompileShader(vertexShader);
            CheckCompileErrors(vertexShader, LumShaderType.VertexShader);

            // Compile fragment shader
            fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, File.ReadAllText(fragmentShaderPath));
            GL.CompileShader(fragmentShader);
            CheckCompileErrors(fragmentShader, LumShaderType.FragmentShader);

            // Create shader program and link shaders
            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vertexShader);
            GL.AttachShader(shaderProgram, fragmentShader);
            GL.LinkProgram(shaderProgram);
            CheckCompileErrors(shaderProgram, LumShaderType.Program);

            // Delete the shaders as they're linked into our program now and no longer necessary
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return shaderProgram;
        }

        private void CheckCompileErrors(int shader, LumShaderType type)
        {
            int success;
            string infoLog;

            if (type == LumShaderType.Program)
            {
                GL.GetProgram(shader, GetProgramParameterName.LinkStatus, out success);
                if (success == 0)
                {
                    GL.GetProgramInfoLog(shader, out infoLog);

                    Logger.Log($"Shader program compilation failed:\n{infoLog}"); // Log the error

                    throw new Exception($"Shader program compilation failed:\n{infoLog}");
                }
            }
            else
            {
                GL.GetShader(shader, ShaderParameter.CompileStatus, out success);
                if (success == 0)
                {
                    GL.GetShaderInfoLog(shader, out infoLog);
                    Logger.Log($"Shader ({type}) compilation failed:\n{infoLog}"); // Log the error
                    throw new Exception($"Shader ({type}) compilation failed:\n{infoLog}");
                }
            }
        }

        public void Use()
        {
            GL.UseProgram(ProgramId);
        }

        public void SetUniform(string name, int value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            if (location != -1)
            {
                GL.Uniform1(location, value);
            }
            else
            {
                // Handle uniform not found error
            }
        }

        public void SetUniform(string name, float value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            if (location != -1)
            {
                GL.Uniform1(location, value);
            }
            else
            {
                // Handle uniform not found error
            }
        }

        public void SetUniform(string name, Vector3 value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            if (location != -1)
            {
                GL.Uniform3(location, value);
            }
            else
            {
                // Handle uniform not found error
            }
        }

        public void SetUniform(string name, Vector4 value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            if (location != -1)
            {
                GL.Uniform4(location, value);
            }
            else
            {
                // Handle uniform not found error
            }
        }

        public void SetUniform(string name, Matrix4 value)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            if (location != -1)
            {
                GL.UniformMatrix4(location, false, ref value);
            }
            else
            {
                // Handle uniform not found error
            }
        }

        public void BindTexture(string name, int textureUnit, int textureId)
        {
            int location = GL.GetUniformLocation(ProgramId, name);
            if (location != -1)
            {
                GL.ActiveTexture(TextureUnit.Texture0 + textureUnit);
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                GL.Uniform1(location, textureUnit);
            }
            else
            {
                // Handle uniform not found error
            }
        }

        public void EnableAttribute(string name)
        {
            int location = GL.GetAttribLocation(ProgramId, name);
            if (location != -1)
            {
                GL.EnableVertexAttribArray(location);
            }
            else
            {
                // Handle attribute not found error
            }
        }

        public void DisableAttribute(string name)
        {
            int location = GL.GetAttribLocation(ProgramId, name);
            if (location != -1)
            {
                GL.DisableVertexAttribArray(location);
            }
            else
            {
                // Handle attribute not found error
            }
        }

        public void Dispose()
        {
            GL.DeleteProgram(ProgramId);
        }
    }
}


