using Assimp;
using ImGuiNET;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using Luminosity3DRendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using static Assimp.Metadata;
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
    //hoiy fuckinton it workie now  tho c: c: c: c: c: c:  C:.C . C:.C .C 
    //now makie shadercache for cahce shader becuase big program many times big slow and bad :c
    //good?

    public class ShaderCache
    {
        public Dictionary<Mesh, Shader> Cache;

        public ShaderCache()
        {
            Cache = new Dictionary<Mesh, Shader>();
        }

        public void CacheShader(Mesh mesh, Shader shader)
        {
            Cache.Add(mesh, shader);
        }

        public Shader Get(Mesh mesh)
        {
            if (Cache.ContainsKey(mesh))
            {
                return Cache[mesh];
            }
            return null;
        }
    }

    public class Material
    {
        private Shader shader;

        // Material properties
        public Vector3 AmbientColor { get; set; }
        public Vector3 DiffuseColor { get; set; }
        public Vector3 SpecularColor { get; set; }
        public Vector3 EmissiveColor { get; set; }
        public Vector3 ReflectiveColor { get; set; }

        // Other material properties
        public float BumpScaling { get; set; }
        public float Shininess { get; set; }
        public float ShininessStrength { get; set; }
        public int BlendMode { get; set; }
        public float Opacity { get; set; }
        public float Reflectivity { get; set; }

        public Material(Shader shader)
        {
            this.shader = shader;

            // Initialize material properties with fallback values or defaults
            AmbientColor = Vector3.One;
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.One;
            EmissiveColor = Vector3.Zero;
            ReflectiveColor = Vector3.Zero;

            // Initialize other material properties
            BumpScaling = 1.0f;
            Shininess = 32.0f;
            ShininessStrength = 1.0f;
            BlendMode = 0;
            Opacity = 1.0f;
            Reflectivity = 0.0f;
        }

        public static Material BasicMaterial(Shader shader)
        {
            return new Material(shader);
        }

        // Create a Lambertian material with a specified diffuse color
        public static Material LambertianMaterial(Shader shader, Vector3 diffuseColor)
        {
            var material = new Material(shader);
            material.DiffuseColor = diffuseColor;
            return material;
        }

        // Create a Phong material with diffuse, specular, and shininess properties
        public static Material PhongMaterial(Shader shader, Vector3 diffuseColor, Vector3 specularColor, float shininess)
        {
            var material = new Material(shader);
            material.DiffuseColor = diffuseColor;
            material.SpecularColor = specularColor;
            material.Shininess = shininess;
            return material;
        }

        // Create a Reflective material with a reflective color
        public static Material ReflectiveMaterial(Shader shader, Vector3 reflectiveColor)
        {
            var material = new Material(shader);
            material.ReflectiveColor = reflectiveColor;
            return material;
        }

        // Create an Emissive material with an emissive color
        public static Material EmissiveMaterial(Shader shader, Vector3 emissiveColor)
        {
            var material = new Material(shader);
            material.EmissiveColor = emissiveColor;
            return material;
        }


        public void Apply()
        {
            shader.Use();
            shader.SetUniform("mat.ambient", AmbientColor);
            shader.SetUniform("mat.diffuse", DiffuseColor);
            shader.SetUniform("mat.specular", SpecularColor);
            shader.SetUniform("mat.emissive", EmissiveColor);
            shader.SetUniform("mat.reflective", ReflectiveColor);

            shader.SetUniform("mat.bumpscaling", BumpScaling);
            shader.SetUniform("mat.shininess", Shininess);
            shader.SetUniform("mat.shininessstrength", ShininessStrength);
            shader.SetUniform("mat.blendmode", BlendMode);
            shader.SetUniform("mat.opacity", Opacity);
            shader.SetUniform("mat.reflectivity", Reflectivity);
            GL.UseProgram(0);
        }
    }


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

        public ShaderCache shaders = new ShaderCache();

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

        public void BuildCache()
        {

            foreach (var mesh in Scene.Meshes)
            {
                if(shaders.Get(mesh) == null)
                {
                    var shader = new Shader("./shaders/builtin/pbr.vert", "./shaders/builtin/pbr.frag");
                    shader.Use();

                    var material = Material.PhongMaterial(shader, new Vector3(100, 25, 14), new Vector3(255, 15, 25), 5f);

                    material.Apply();

                    CheckGLError("Mat uniforms");
                    GL.UseProgram(0);
                    shaders.CacheShader(mesh,shader);
                }
                
            }
        }
        public void Render(Matrix4 viewMatrix, Matrix4 projectionMatrix, Matrix4 modelMatrix, Vector3 viewPos)
        {
            try
            {
                Bind();
                CheckGLError("Binding");
   
                foreach (Assimp.Mesh mesh in Scene.Meshes)
                {
                    var shader = shaders.Get(mesh);
                    shader.Use();
                    shader.SetUniform("modelMatrix", modelMatrix);
                    shader.SetUniform("viewMatrix", viewMatrix);
                    shader.SetUniform("projectionMatrix", projectionMatrix);
                    shader.SetUniform("viewPos", viewPos);
                    shader.SetUniform("lightColor", new Vector3(1f, 1f, 1f));
                    shader.SetUniform("objectColor", new Vector3(0.25f, 0.1f, 0.5f));
                    shader.SetUniform("lightPos", new Vector3(10.0f, 10.0f, 10.0f));

                    // Create and begin an occlusion query
                    int queryID;
                    GL.GenQueries(1, out queryID);
                    GL.BeginQuery(QueryTarget.SamplesPassed, queryID);

                    CheckGLError("Begin occlusion query");

                    // Render the object conditionally based on occlusion query
                    GL.DrawElements(PrimitiveType.Triangles, mesh.FaceCount * 3, DrawElementsType.UnsignedInt, 0);

                    // End the occlusion query
                    GL.EndQuery(QueryTarget.SamplesPassed);

                    CheckGLError("End occlusion query");

                    int queryResult;
                    GL.GetQueryObject(queryID, GetQueryObjectParam.QueryResult, out queryResult);

                    // Delete the query object
                    GL.DeleteQueries(1, ref queryID);

                    // If the query result is greater than zero, the object is visible
                    if (queryResult > 0)
                    {
                        // Object is visible, continue with rendering
                        CheckGLError("Drawing a mesh");
                    }
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

                BuildCache();
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



    [RequireComponent(typeof(TransformComponent))]
    public class MeshBatch : LuminosityBehaviour, IRenderable, IImguiSerialize
    {
        private TransformComponent transform = null;

        public string filePath = "./teapot.obj";
        public MeshModel model = null;
        public Shader shader = null;
        public string vert = "./shaders/builtin/pbr.vert";
        public string frag = "./shaders/builtin/pbr.frag";

        public override void Awake()
        {
            transform = GetComponent<TransformComponent>();
            ComputeBuffers(filePath);
            shader = new Shader(vert, frag);
        }

        public void ComputeBuffers(string filePath)
        {
            var cachedMesh = MeshCache.Get(filePath);
            if (cachedMesh != null)
            {

                model = cachedMesh;
            }

            var meshModel = new MeshModel(filePath);
            MeshCache.Cache(meshModel);
            model = meshModel;
        }

        public void EditorUI()
        {
            ImGui.Text(filePath);
        }

        public static Component OnEditorCreation()
        {
            return new MeshBatch();
        }

        public void OnRender()
        {
            var cam = Engine.SceneManager.ActiveScene.activeCam;
            if (cam != null)
            {
                if (transform != null)
                {
                    model.Render(LMath.ToMatTk(cam.ViewMatrix), LMath.ToMatTk(cam.ProjectionMatrix), LMath.ToMatTk(transform.GetTransformMatrix()), LMath.ToVecTk(cam.Position));
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


