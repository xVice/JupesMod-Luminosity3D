using Assimp;
using glTFLoader;
using glTFLoader.Schema;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using OpenTK.Graphics.Egl;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace Luminosity3D.Builtin
{
    public interface IRenderable
    {
        public void OnRender();
    }

    //This is insanity, straight insanity, thats why i am rewriting it now.
    //
    //right now its looking way better, i didnt do any shading though c:
    public class MeshBatch : Component, IRenderable
    {
        public List<Mesh> meshes;

        public MeshBatch(string filePath)
        {
            meshes = LoadMeshes(filePath);
        }

        public override void Start()
        {
            foreach (var mesh in meshes)
            {
                mesh.InitializeBuffers();
            }
        }

        public override void Update()
        {
            // Render all loaded meshes

        }

        public override void OnDestroy()
        {
            // Dispose of all loaded meshes
            foreach (var mesh in meshes)
            {
                mesh.Dispose();
            }
        }


        private List<Mesh> LoadMeshes(string filePath)
        {
            var meshes = new List<Mesh>();

            using (var importer = new AssimpContext())
            {
                var scene = importer.ImportFile(filePath, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

                if (scene == null || scene.Meshes.Count == 0)
                {
                    Logger.Log($"Failed to load mesh from file: {filePath}");
                    return meshes;
                }

                foreach (var assimpMesh in scene.Meshes)
                {
                    var vertices = new List<Vector3>();
                    var normals = new List<Vector3>();
                    var texCoords = new List<Vector2[]>();
                    var tangents = new List<Vector3>();
                    var bitangents = new List<Vector3>();
                    var indices = new List<int>();

                    foreach (var vector in assimpMesh.Vertices)
                    {
                        vertices.Add(new Vector3(vector.X, vector.Y, vector.Z));
                    }

                    foreach (var vector in assimpMesh.Normals)
                    {
                        normals.Add(new Vector3(vector.X, vector.Y, vector.Z));
                    }

                    // Support multiple sets of texture coordinates
                    for (int i = 0; i < assimpMesh.TextureCoordinateChannelCount; i++)
                    {
                        var texCoordsSet = new List<Vector2>();
                        foreach (var vector in assimpMesh.TextureCoordinateChannels[i])
                        {
                            texCoordsSet.Add(new Vector2(vector.X, vector.Y));
                        }
                        texCoords.Add(texCoordsSet.ToArray());
                    }


                    foreach (var vector in assimpMesh.Tangents)
                    {
                        tangents.Add(new Vector3(vector.X, vector.Y, vector.Z));
                    }

                    foreach (var vector in assimpMesh.BiTangents)
                    {
                        bitangents.Add(new Vector3(vector.X, vector.Y, vector.Z));
                    }

                    foreach (var face in assimpMesh.Faces)
                    {
                        indices.AddRange(face.Indices);
                    }

                    meshes.Add(new Mesh(vertices.ToArray(), normals.ToArray(), texCoords.ToArray(), tangents.ToArray(), bitangents.ToArray(), indices.ToArray(), new Shader("./shaders/builtin/pbr.vert", "./shaders/builtin/pbr.frag")));
                }
            }

            return meshes;
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
            foreach (var mesh in meshes)
            {
                mesh.Render();
            }
        }
    }



    public class Mesh
    {
        public Shader shader; // Add a private field for the Shader

        // Vertex data
        public Vector3[] Vertices { get; private set; }
        public Vector3[] Normals { get; private set; }
        public Vector2[][] TexCoords { get; private set; } // Support for multiple sets of texture coordinates
        public Vector3[] Tangents { get; private set; } // Tangents for normal mapping
        public Vector3[] Bitangents { get; private set; } // Bitangents for normal mapping
        public int[] Indices { get; private set; }

        // OpenGL handles
        private int vao;
        private int vboVertices;
        private int vboNormals;
        private int[] vboTexCoords; // Array of VBOs for multiple sets of texture coordinates
        private int vboTangents;
        private int vboBitangents;
        private int ebo;

        public Mesh(Vector3[] vertices, Vector3[] normals, Vector2[][] texCoords, Vector3[] tangents, Vector3[] bitangents, int[] indices, Shader shader)
        {
            Vertices = vertices ?? throw new ArgumentNullException(nameof(vertices));
            Normals = normals ?? throw new ArgumentNullException(nameof(normals));
            TexCoords = texCoords ?? throw new ArgumentNullException(nameof(texCoords));
            Tangents = tangents ?? throw new ArgumentNullException(nameof(tangents));
            Bitangents = bitangents ?? throw new ArgumentNullException(nameof(bitangents));
            Indices = indices ?? throw new ArgumentNullException(nameof(indices));

            this.shader = shader; // Store the Shader instance
        }

        public void InitializeBuffers()
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

            shader.Use();

            // Specify vertex attribute pointers for normals
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Create and bind Vertex Buffer Objects (VBOs) for multiple sets of texture coordinates
            vboTexCoords = new int[TexCoords.Length];
            for (int i = 0; i < TexCoords.Length; i++)
            {
                GL.GenBuffers(1, out vboTexCoords[i]);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vboTexCoords[i]);
                GL.BufferData(BufferTarget.ArrayBuffer, TexCoords[i].Length * Vector2.SizeInBytes, TexCoords[i], BufferUsageHint.StaticDraw);

                // Specify vertex attribute pointers for texture coordinates
                GL.EnableVertexAttribArray(2 + i);
                GL.VertexAttribPointer(2 + i, 2, VertexAttribPointerType.Float, false, 0, 0);
            }

            // Create and bind Vertex Buffer Object (VBO) for tangents
            GL.GenBuffers(1, out vboTangents);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboTangents);
            GL.BufferData(BufferTarget.ArrayBuffer, Tangents.Length * Vector3.SizeInBytes, Tangents, BufferUsageHint.StaticDraw);

            // Specify vertex attribute pointers for tangents
            GL.EnableVertexAttribArray(2 + TexCoords.Length);
            GL.VertexAttribPointer(2 + TexCoords.Length, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Create and bind Vertex Buffer Object (VBO) for bitangents
            GL.GenBuffers(1, out vboBitangents);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboBitangents);
            GL.BufferData(BufferTarget.ArrayBuffer, Bitangents.Length * Vector3.SizeInBytes, Bitangents, BufferUsageHint.StaticDraw);

            // Specify vertex attribute pointers for bitangents
            GL.EnableVertexAttribArray(3 + TexCoords.Length);
            GL.VertexAttribPointer(3 + TexCoords.Length, 3, VertexAttribPointerType.Float, false, 0, 0);

            // Create and bind Element Buffer Object (EBO)
            GL.GenBuffers(1, out ebo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Indices.Length * sizeof(int), Indices, BufferUsageHint.StaticDraw);

            // Unbind VAO
            GL.BindVertexArray(0);
            Logger.Log("Buffer initialized");
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
            GL.DeleteBuffer(vboBitangents);
            GL.DeleteBuffer(vboTangents);
            for (int i = 0; i < vboTexCoords.Length; i++)
            {
                GL.DeleteBuffer(vboTexCoords[i]);
            }
            GL.DeleteBuffer(vboNormals);
            GL.DeleteBuffer(vboVertices);
            GL.DeleteVertexArray(vao);
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
                    throw new Exception($"Shader program compilation failed:\n{infoLog}");
                }
            }
            else
            {
                GL.GetShader(shader, ShaderParameter.CompileStatus, out success);
                if (success == 0)
                {
                    GL.GetShaderInfoLog(shader, out infoLog);
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


