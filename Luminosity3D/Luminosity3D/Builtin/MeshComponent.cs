using glTFLoader;
using glTFLoader.Schema;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Builtin
{
    public class ShaderCache<T>
    {
        public Pool<IShader<T>> Shaders { get; set; }

        public Dictionary<T, int> ShaderIndexer;  

        public ShaderCache()
        {
            Shaders = new Pool<IShader<T>>();
            ShaderIndexer = new Dictionary<T, int>();
        }

        public void AddShader(IShader<T> shader)
        {
            Shaders.Enqueue(shader);
        }

        

        public void CompileShaders()
        {
            foreach (var shader in Shaders.GetContent())
            {
                if (!shader.ShaderBuild)
                {
                    shader.Compile();
                    if (shader.ShaderBuild)
                    {
                        ShaderIndexer.Add(shader.MeshData, shader.ShaderIndex);
                    }
                }

                
            }
        }

        public void UnloadShaders()
        {
            foreach(var shader in Shaders.GetContent())
            {
                if (shader.ShaderBuild)
                {
                    shader.Unload();
                }
            }
        }

        
    }

    public interface IShader<T>
    {

        public string VertPath { get; set; }
        public string FragPath { get; set; }
        
        public bool ShaderBuild { get; set; }
        public int ShaderIndex { get; set; }

        public T MeshData { get; set; }

        public void Compile();

        public void Unload();
    }

    public class PBRShader : IShader<Mesh>
    {
        public int ShaderProgram { get; private set; }
        public string VertPath { get => "shaders/builtin/pbr.vert"; set => throw new NotImplementedException(); }
        public string FragPath { get => "shaders/builtin/pbr.frag"; set => throw new NotImplementedException(); }
        public int ShaderIndex { get => _shaderInt; set => throw new NotImplementedException(); }
        public bool ShaderBuild { get => _shaderBuild; set => throw new NotImplementedException(); }
        public Mesh MeshData { get => _mesh; set => _mesh = value; }

        


        private Mesh _mesh;
        private bool _shaderBuild = false;
        private int _shaderInt;

        public PBRShader(Mesh mesh)
        {
            _mesh = mesh;
        }

       //finally, use any datatype in any mesh with any shader.... if you really want too
        public void Compile()
        {
            // Load and compile vertex and fragment shaders
            int vertexShader = LoadShader(ShaderType.VertexShader, VertPath);
            int fragmentShader = LoadShader(ShaderType.FragmentShader, FragPath);

            // Create shader program
            ShaderProgram = GL.CreateProgram();

            // Attach the vertex and fragment shaders to the program
            GL.AttachShader(ShaderProgram, vertexShader);
            GL.AttachShader(ShaderProgram, fragmentShader);

            // Link and validate the shader program
            GL.LinkProgram(ShaderProgram);
            GL.ValidateProgram(ShaderProgram);

            // Delete the individual shaders as they are no longer needed
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            _shaderInt = ShaderProgram;
            _shaderBuild = true;

        }

        public void Unload()
        {

        }

        private int LoadShader(ShaderType type, string path)
        {
            // Load the shader source code from file
            string sourceCode = File.ReadAllText(path);

            // Create and compile the shader
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, sourceCode);
            GL.CompileShader(shader);

            // Check for shader compilation errors
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shader);
                throw new ApplicationException($"Shader compilation error: {infoLog}");
            }

            return shader;
        }


    }

    public class Mesh
    {
        // Vertex data
        public Vector3[] Vertices { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Vector2[] TexCoords { get; private set; }
        public int[] Indices { get; private set; }

        // OpenGL handles
        private int vao;
        private int vboVertices;
        private int vboNormals;
        private int vboTexCoords;
        private int ebo;

        public Mesh(Vector3[] vertices, Vector3[] normals, Vector2[] texCoords, int[] indices)
        {
            Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
            Normals = normals ?? throw new ArgumentNullException(nameof(normals));
            TexCoords = texCoords ?? throw new ArgumentNullException(nameof(texCoords));
            Indices = indices ?? throw new ArgumentNullException(nameof(indices));

            // Initialize OpenGL buffers and VAO
            InitializeBuffers();
        }

        private void InitializeBuffers()
        {
            // Create and bind Vertex Array Object (VAO)
            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            // Create and bind Vertex Buffer Object (VBO) for vertices
            GL.GenBuffers(1, out vboVertices);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboVertices);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * Vector3.SizeInBytes, Vertices, BufferUsageHint.StaticDraw);

            // Specify vertex attribute pointers for vertices
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Create and bind Vertex Buffer Object (VBO) for normals
            GL.GenBuffers(1, out vboNormals);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboNormals);
            GL.BufferData(BufferTarget.ArrayBuffer, Normals.Length * Vector3.SizeInBytes, Normals, BufferUsageHint.StaticDraw);

            // Specify vertex attribute pointers for normals
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Create and bind Vertex Buffer Object (VBO) for texture coordinates
            GL.GenBuffers(1, out vboTexCoords);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords);
            GL.BufferData(BufferTarget.ArrayBuffer, TexCoords.Length * Vector2.SizeInBytes, TexCoords, BufferUsageHint.StaticDraw);

            // Specify vertex attribute pointers for texture coordinates
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);

            // Create and bind Element Buffer Object (EBO)
            GL.GenBuffers(1, out ebo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(int), Indices, BufferUsageHint.StaticDraw);

            // Unbind VAO
            GL.BindVertexArray(0);
        }

        public void Render()
        {
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(ebo);
            GL.DeleteBuffer(vboTexCoords);
            GL.DeleteBuffer(vboNormals);
            GL.DeleteBuffer(vboVertices);
            GL.DeleteVertexArray(vao);
        }
    }

    //Eventually reworkt to be easiyl extendable by doing the funny code c:
    /// <summary>
    /// Represents a .gltf/glb file
    /// Includes a shader cached for each mesh and a list of Meshes that hold the shader, the shadercache is essentially a wrapper for the list.
    /// </summary>
    public class MeshBatchComponent : Component
    {
        public ShaderCache<Mesh> ShaderCache { get; set; }
        public List<Mesh> Meshes { get; set; }


        public Assimp.Scene? scene { get; set; }

        public static MeshBatchComponent LoadFromFile(string path)
        {
            var meshBatch = new MeshBatchComponent();

            if (File.Exists(path))
            {
                Assimp.Scene model;
                Assimp.AssimpContext importer = new Assimp.AssimpContext();
                importer.SetConfig(new Assimp.Configs.NormalSmoothingAngleConfig(66.0f));
                model = importer.ImportFile(path, Assimp.PostProcessPreset.TargetRealTimeMaximumQuality);

                meshBatch.scene = model;
                meshBatch.LoadMeshList();
                return meshBatch;
            }
            else
            {
                return null;
            }
            
        }

        public void LoadMeshList()
        {




            foreach (var mesh in scene.Meshes)
            {

                if (mesh.HasFaces)
                {
                    if (mesh.HasFaces)
                    {
                        var vertices = new List<Vector3>();
                        var normals = new List<Vector3>();
                        var texCoords = new List<Vector2>();
                        var indices = new List<int>();

                        foreach (var face in mesh.Faces)
                        {
                            if (face.HasIndices)
                            {
                                indices.AddRange(face.Indices);
                            }
                        }

                        for (int i = 0; i < mesh.VertexCount; i++)
                        {
                            vertices.Add(new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z));

                            if (mesh.HasNormals)
                            {
                                normals.Add(new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z));
                            }

                            if (mesh.HasTextureCoords(0)) // Assuming only one texture coordinate channel
                            {
                                texCoords.Add(new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y));
                            }
                        }

                        var internalMesh = new Mesh(vertices.ToArray(), normals.ToArray(), texCoords.ToArray(), indices.ToArray());

                        PBRShader pbrShader = new PBRShader(internalMesh);
                        ShaderCache.AddShader(pbrShader);


                        Meshes.Add(internalMesh);
                    }
                }
            }
            ShaderCache.CompileShaders();

        }

        
        public override void Awake()
        {
            
        }


        public override void OnDisable()
        {
            ShaderCache.UnloadShaders();
        }

        public override void OnEnable()
        {
            ShaderCache.CompileShaders();
        }

        public override void EarlyUpdate()
        {
         
        }

        public override void LateUpdate()
        {
       
        }

        public override void OnDestroy()
        {
       
        }



        public override void Start()
        {
        
        }

        public override void Update()
        {
         
        }
    }
}
