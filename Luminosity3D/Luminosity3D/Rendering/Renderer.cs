using Luminosity3D.Utils;
using Luminosity3D;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using Luminosity3D.Builtin.RenderLayers;
using ImGuiNET;
using OpenTK.Windowing.Common.Input;
using BulletSharp;
using Assimp;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;
using Luminosity3D.Rendering;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using Camera = Luminosity3D.Builtin.Camera;
using System.Runtime.InteropServices;
using StbImageSharp;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;
using Face = Assimp.Face;
using Luminosity3D.Builtin;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.CompilerServices;
using BulletSharp.Math;
using Vector3 = OpenTK.Mathematics.Vector3;
using Luminosity3D.EntityComponentSystem;
using ImGuizmoNET;
using Noesis;
using Matrix4 = OpenTK.Mathematics.Matrix4;
using Path = System.IO.Path;
using Matrix = BulletSharp.Math.Matrix;
using Marshal = System.Runtime.InteropServices.Marshal;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;
using Luminosity3D.PKGLoader;
using Luminosity3DScening;

namespace Luminosity3DRendering
{

    public class AssimpModel : IDisposable
    {
        private Assimp.Scene scene;
        public List<Meshe> meshes { get; }
        public Meshe FirstMeshe => meshes[0];
        public List<float> PointsForCollision { get; }
        private string PathModel = string.Empty;

       

        public AssimpModel(string FilePath, bool FlipUVs = false)
        {
            if (!File.Exists(FilePath))
                throw new Exception($"Assimp file not found: {FilePath}..");

            PathModel = FilePath;

            scene = new Assimp.Scene();
            meshes = new List<Meshe>();
            PointsForCollision = new List<float>();

            using (var importer = new AssimpContext())
            {

                if (FlipUVs)
                    scene = importer.ImportFile(FilePath, PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.FlipUVs | PostProcessSteps.CalculateTangentSpace);
                else
                    scene = importer.ImportFile(FilePath, PostProcessSteps.Triangulate | PostProcessSteps.GenerateSmoothNormals | PostProcessSteps.CalculateTangentSpace);

            }

            processNodes(scene.RootNode);
        }
        private void processNodes(Node node)
        {
            for (int i = 0; i < node.MeshCount; i++)
            {
                ProcessMesh(scene.Meshes[node.MeshIndices[i]]);

            }
            for (int i = 0; i < node.ChildCount; i++)
            {
                processNodes(node.Children[i]);
            }
        }
        private void ProcessMesh(Mesh mesh)
        {
            var vertices = new List<Vertex>();
            var indices = new List<ushort>();

            for (int i = 0; i < mesh.VertexCount; i++)
            {
                var packed = new Vertex();

                PointsForCollision.Add(mesh.Vertices[i].X);
                PointsForCollision.Add(mesh.Vertices[i].Y);
                PointsForCollision.Add(mesh.Vertices[i].Z);

                packed.Positions = new Vector3(mesh.Vertices[i].X, mesh.Vertices[i].Y, mesh.Vertices[i].Z);
                packed.Normals = new Vector3(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
                if (mesh.HasTextureCoords(0))
                {
                    packed.TexCoords = new Vector2(mesh.TextureCoordinateChannels[0][i].X, mesh.TextureCoordinateChannels[0][i].Y);

                }
                else
                {
                    packed.TexCoords = new Vector2(0.0f, 0.0f);
                }
                packed.Tangents = new Vector3(mesh.Tangents[i].X, mesh.Tangents[i].Y, mesh.Tangents[i].Z);
                packed.Bitangents = new Vector3(mesh.BiTangents[i].X, mesh.BiTangents[i].Y, mesh.BiTangents[i].Z);

                vertices.Add(packed);
            }

            for (int i = 0; i < mesh.FaceCount; i++)
            {
                Face face = mesh.Faces[i];
                for (int j = 0; j < face.IndexCount; j++)
                {
                    indices.Add((ushort)face.Indices[j]);
                }
            }

            ModelTexturesPath texturesPath = new ModelTexturesPath();

            if (mesh.MaterialIndex >= 0)
            {
                // Texturas
                Material material = scene.Materials[mesh.MaterialIndex];
                texturesPath = ProcessTextures(material.GetAllMaterialTextures());
            }

            meshes.Add(new Meshe(vertices, indices, texturesPath));

        }
        private ModelTexturesPath ProcessTextures(TextureSlot[] slot)
        {
            ModelTexturesPath texturesPath = new ModelTexturesPath();

            foreach (var item in slot)
            {
                if (item.FilePath != null)
                {
                    string path = Path.GetDirectoryName(PathModel);
                    string filePath = Path.Combine(path, item.FilePath);
                    if (File.Exists(filePath))
                    {
                   
                        if (item.TextureType == TextureType.Diffuse)
                        {
                            texturesPath._DiffusePath = filePath;
                        }
                        else if (item.TextureType == TextureType.Specular)
                        {
                            texturesPath._SpecularPath = filePath;
                        }
                        else if (item.TextureType == TextureType.Normals)
                        {
                            texturesPath._NormalPath = filePath;
                        }
                        else if (item.TextureType == TextureType.Height)
                        {
                            texturesPath._HeightPath = filePath;
                        }
                        else if (item.TextureType == TextureType.Metalness)
                        {
                            texturesPath._MetallicPath = filePath;
                        }
                        else if (item.TextureType == TextureType.Roughness)
                        {
                            texturesPath._RoughnnesPath = filePath;
                        }
                        else if (item.TextureType == TextureType.Lightmap)
                        {
                            texturesPath._LightMap = filePath;
                        }
                        else if (item.TextureType == TextureType.Emissive)
                        {
                            texturesPath._EmissivePath = filePath;
                        }
                        else if (item.TextureType == TextureType.AmbientOcclusion)
                        {
                            texturesPath._AmbientOcclusionPath = filePath;
                        }
                    }
      
                }
            }

            return texturesPath;
        }
        private void ProcessColors(Material material)
        {
            // colors
            if (material.HasColorAmbient)
            {
                Console.WriteLine(material.ColorAmbient);
            }
            if (material.HasColorDiffuse)
            {
                Console.WriteLine(material.ColorDiffuse);
            }
            if (material.HasColorEmissive)
            {
                Console.WriteLine(material.ColorEmissive);
            }
            if (material.HasColorReflective)
            {
                Console.WriteLine(material.ColorReflective);
            }
            if (material.HasColorSpecular)
            {
                Console.WriteLine(material.ColorSpecular);
            }
            if (material.HasColorTransparent)
            {
                Console.WriteLine(material.ColorTransparent);
            }
        }
        private static void ProcessValues(Material material)
        {
            // floats
            if (material.HasOpacity)
            {
                Console.WriteLine(material.Opacity);
            }
            if (material.HasTransparencyFactor)
            {
                Console.WriteLine(material.TransparencyFactor);
            }
            if (material.HasBumpScaling)
            {
                Console.WriteLine(material.BumpScaling);
            }
            if (material.HasShininess)
            {
                Console.WriteLine(material.Shininess);
            }
            if (material.HasShininessStrength)
            {
                Console.WriteLine(material.ShininessStrength);
            }
        }
        public void Dispose()
        {
            scene.Clear();
        }
    }
    public static class LGLE
    {
        public static void CheckGLError(string location, Exception ex = null)
        {
            ErrorCode errorCode = GL.GetError();
            if (errorCode != ErrorCode.NoError)
            {
                if (ex != null)
                {
                    Logger.Log($"OpenGL Error at {location}: {errorCode}", true, LogType.Error);
                    throw new Exception($"OpenGL Error at {location}: {errorCode}");
                }
                else
                {
                    Logger.Log($"OpenGL Error at {location}: {errorCode}", true, LogType.Error);
                    Logger.Log($"OpenGL appended exception: {ex.ToString()}", true, LogType.Error);
                    throw new Exception($"OpenGL Error at {location}: {errorCode}");
                }

            }
        }
        public static Vector3 FromVector(Assimp.Vector3D vec)
        {
            Vector3 v;
            v.X = vec.X;
            v.Y = vec.Y;
            v.Z = vec.Z;
            return v;
        }

        public static System.Numerics.Vector4 ToVector4(Color4D col)
        {
            return new System.Numerics.Vector4(col.R, col.G, col.B, col.A);
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
    public struct Vertex
    {
        public Vector3 Positions;
        public Vector3 Normals;
        public Vector2 TexCoords;
        public Vector3 Tangents;
        public Vector3 Bitangents;
    }
    public class ModelTexturesPath
    {
        public string _DiffusePath = string.Empty;
        public string _SpecularPath = string.Empty;
        public string _NormalPath = string.Empty;
        public string _HeightPath = string.Empty;
        public string _MetallicPath = string.Empty;
        public string _RoughnnesPath = string.Empty;
        public string _LightMap = string.Empty;
        public string _EmissivePath = string.Empty;
        public string _AmbientOcclusionPath = string.Empty;
    }
    public enum CubeMapType
    {
        /// <summary>
        /// Texture Format square: 「 」
        /// </summary>
        Type0,

        /// <summary>
        /// Texture Format texture faces: X+ X- | Y+ Y- | Z+ Z-
        /// </summary>
        Type1,

        /// <summary>
        /// Texture format cross: -|--
        /// </summary>
        Type2,


        /// <summary>
        /// Texture Format: T
        /// </summary>
        Type3,


    }
    public class CubemapDefault
    {
        private static VertexArrayObject? Vao;
        private static BufferObject<float>? vbo;
        public static void RenderCube()
        {
            if (Vao == null)
            {
                float[] vertices =
                {

                    -1.0f, -1.0f, -1.0f,  0.0f, 0.0f,
                     1.0f,  1.0f, -1.0f,  1.0f, 1.0f,
                     1.0f, -1.0f, -1.0f,  1.0f, 0.0f,
                     1.0f,  1.0f, -1.0f,  1.0f, 1.0f,
                    -1.0f, -1.0f, -1.0f,  0.0f, 0.0f,
                    -1.0f,  1.0f, -1.0f,  0.0f, 1.0f,
                    -1.0f, -1.0f,  1.0f,  0.0f, 0.0f,
                     1.0f, -1.0f,  1.0f,  1.0f, 0.0f,
                     1.0f,  1.0f,  1.0f,  1.0f, 1.0f,
                     1.0f,  1.0f,  1.0f,  1.0f, 1.0f,
                    -1.0f,  1.0f,  1.0f,  0.0f, 1.0f,
                    -1.0f, -1.0f,  1.0f,  0.0f, 0.0f,
                    -1.0f,  1.0f,  1.0f,  1.0f, 0.0f,
                    -1.0f,  1.0f, -1.0f,  1.0f, 1.0f,
                    -1.0f, -1.0f, -1.0f,  0.0f, 1.0f,
                    -1.0f, -1.0f, -1.0f,  0.0f, 1.0f,
                    -1.0f, -1.0f,  1.0f,  0.0f, 0.0f,
                    -1.0f,  1.0f,  1.0f,  1.0f, 0.0f,
                     1.0f,  1.0f,  1.0f,  1.0f, 0.0f,
                     1.0f, -1.0f, -1.0f,  0.0f, 1.0f,
                     1.0f,  1.0f, -1.0f,  1.0f, 1.0f,
                     1.0f, -1.0f, -1.0f,  0.0f, 1.0f,
                     1.0f,  1.0f,  1.0f,  1.0f, 0.0f,
                     1.0f, -1.0f,  1.0f,  0.0f, 0.0f,
                    -1.0f, -1.0f, -1.0f,  0.0f, 1.0f,
                     1.0f, -1.0f, -1.0f,  1.0f, 1.0f,
                     1.0f, -1.0f,  1.0f,  1.0f, 0.0f,
                     1.0f, -1.0f,  1.0f,  1.0f, 0.0f,
                    -1.0f, -1.0f,  1.0f,  0.0f, 0.0f,
                    -1.0f, -1.0f, -1.0f,  0.0f, 1.0f,
                    -1.0f,  1.0f, -1.0f,  0.0f, 1.0f,
                     1.0f,  1.0f , 1.0f,  1.0f, 0.0f,
                     1.0f,  1.0f, -1.0f,  1.0f, 1.0f,
                     1.0f,  1.0f,  1.0f,  1.0f, 0.0f,
                    -1.0f,  1.0f, -1.0f,  0.0f, 1.0f,
                    -1.0f,  1.0f,  1.0f,  0.0f, 0.0f
                };

                Vao = new VertexArrayObject();
                vbo = new BufferObject<float>(vertices, BufferTarget.ArrayBuffer);

                Vao.LinkBufferObject(ref vbo);
                Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5 * sizeof(float), 0 * sizeof(float));
                Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5 * sizeof(float), 3 * sizeof(float));
            }
            // render DefaultCube
            Vao.Bind();
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }
        public static void Dispose() => Vao!.Dispose();
    }
    public class CubemapT
    {
        private static VertexArrayObject? Vao;
        private static BufferObject<float>? vbo;
        public static void RenderCube()
        {
            if (Vao == null)
            {
                float[] vertices =
                {

                    -1.0f,  1.0f, -1.0f,   0.999914f, 0.750482f,
                     1.0f,  1.0f,  1.0f,   0.667046f, 0.999732f,
                     1.0f,  1.0f, -1.0f,   0.666272f, 0.750489f,
                     1.0f,  1.0f,  1.0f,   0.666289f, 0.000426f,
                    -1.0f, -1.0f,  1.0f,   0.334182f, 0.249989f,
                     1.0f, -1.0f,  1.0f,   0.334184f, 0.000426f,
                    -1.0f,  1.0f,  1.0f,   0.666288f, 0.249506f,
                    -1.0f, -1.0f, -1.0f,   0.334186f, 0.499505f,
                    -1.0f, -1.0f,  1.0f,   0.334182f, 0.249989f,
                     1.0f, -1.0f, -1.0f,   0.334179f, 0.750536f,
                    -1.0f, -1.0f,  1.0f,   0.000838f, 0.999708f,
                    -1.0f, -1.0f, -1.0f,   0.000958f, 0.750539f,
                     1.0f,  1.0f, -1.0f,   0.666272f, 0.750489f,
                     1.0f, -1.0f,  1.0f,   0.333746f, 0.999729f,
                     1.0f, -1.0f, -1.0f,   0.334179f, 0.750536f,
                    -1.0f,  1.0f, -1.0f,   0.666291f, 0.499505f,
                     1.0f, -1.0f, -1.0f,   0.334179f, 0.750536f,
                    -1.0f, -1.0f, -1.0f,   0.334186f, 0.499505f,
                    -1.0f,  1.0f, -1.0f,   0.999914f, 0.750482f,
                    -1.0f,  1.0f,  1.0f,   0.999908f, 0.999732f,
                     1.0f,  1.0f,  1.0f,   0.667046f, 0.999732f,
                     1.0f,  1.0f,  1.0f,   0.666289f, 0.000426f,
                    -1.0f,  1.0f,  1.0f,   0.666288f, 0.249506f,
                    -1.0f, -1.0f,  1.0f,   0.334182f, 0.249989f,
                    -1.0f,  1.0f,  1.0f,   0.666288f, 0.249506f,
                    -1.0f,  1.0f, -1.0f,   0.666291f, 0.499505f,
                    -1.0f, -1.0f, -1.0f,   0.334186f, 0.499505f,
                     1.0f, -1.0f, -1.0f,   0.334179f, 0.750536f,
                     1.0f, -1.0f,  1.0f,   0.333746f, 0.999729f,
                    -1.0f, -1.0f,  1.0f,   0.000838f, 0.999708f,
                     1.0f,  1.0f, -1.0f,   0.666272f, 0.750489f,
                     1.0f,  1.0f,  1.0f,   0.667046f, 0.999732f,
                     1.0f, -1.0f,  1.0f,   0.333746f, 0.999729f,
                    -1.0f,  1.0f, -1.0f,   0.666291f, 0.499505f,
                     1.0f,  1.0f, -1.0f,   0.666272f, 0.750489f,
                     1.0f, -1.0f, -1.0f,   0.334179f, 0.750536f,

                };

                Vao = new VertexArrayObject();
                vbo = new BufferObject<float>(vertices, BufferTarget.ArrayBuffer);

                Vao.LinkBufferObject(ref vbo);
                Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5 * sizeof(float), 0 * sizeof(float));
                Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5 * sizeof(float), 3 * sizeof(float));

            }

            Vao.Bind();
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }
        public static void Dispose() => Vao!.Dispose();
    }
    public class CubeMapCross
    {
        private static VertexArrayObject? Vao;
        private static BufferObject<float>? vbo;
        public static void RenderCube()
        {
            if (Vao == null)
            {
                float[] vertices =
                {

                    -1.0f,  1.0f, -1.0f,     0.499858f, 0.999974f,
                     1.0f,  1.0f,  1.0f,     0.250174f, 0.666554f,
                     1.0f,  1.0f, -1.0f,     0.499842f, 0.666538f,
                     1.0f,  1.0f,  1.0f,     0.250174f, 0.666554f,
                    -1.0f, -1.0f,  1.0f,     0.000161f, 0.333634f,
                     1.0f, -1.0f,  1.0f,     0.250158f, 0.333594f,
                    -1.0f,  1.0f,  1.0f,     0.999946f, 0.666505f,
                    -1.0f, -1.0f, -1.0f,     0.750488f, 0.333561f,
                    -1.0f, -1.0f,  1.0f,     0.99993f,  0.333545f,
                     1.0f, -1.0f, -1.0f,     0.499826f, 0.333578f,
                    -1.0f, -1.0f,  1.0f,     0.250142f, 0.000504f,
                    -1.0f, -1.0f, -1.0f,     0.49981f,  0.000488f,
                     1.0f,  1.0f, -1.0f,     0.499842f, 0.666538f,
                     1.0f, -1.0f,  1.0f,     0.250158f, 0.333594f,
                     1.0f, -1.0f, -1.0f,     0.499826f, 0.333578f,
                    -1.0f,  1.0f, -1.0f,     0.750504f, 0.666521f,
                     1.0f, -1.0f, -1.0f,     0.499826f, 0.333578f,
                    -1.0f, -1.0f, -1.0f,     0.750488f, 0.333561f,
                    -1.0f,  1.0f, -1.0f,     0.499858f, 0.999974f,
                    -1.0f,  1.0f,  1.0f,     0.250191f, 0.99999f,
                     1.0f,  1.0f,  1.0f,     0.250174f, 0.666554f,
                     1.0f,  1.0f,  1.0f,     0.250174f, 0.666554f,
                    -1.0f,  1.0f,  1.0f,     0.000139f, 0.666522f,
                    -1.0f, -1.0f,  1.0f,     0.000161f, 0.333634f,
                    -1.0f,  1.0f,  1.0f,     0.999946f, 0.666505f,
                    -1.0f,  1.0f, -1.0f,     0.750504f, 0.666521f,
                    -1.0f, -1.0f, -1.0f,     0.750488f, 0.333561f,
                     1.0f, -1.0f, -1.0f,     0.499826f, 0.333578f,
                     1.0f, -1.0f,  1.0f,     0.250158f, 0.333594f,
                    -1.0f, -1.0f,  1.0f,     0.250142f, 0.000504f,
                     1.0f,  1.0f, -1.0f,     0.499842f, 0.666538f,
                     1.0f,  1.0f,  1.0f,     0.250174f, 0.666554f,
                     1.0f, -1.0f,  1.0f,     0.250158f, 0.333594f,
                    -1.0f,  1.0f, -1.0f,     0.750504f, 0.666521f,
                     1.0f,  1.0f, -1.0f,     0.499842f, 0.666538f,
                     1.0f, -1.0f, -1.0f,     0.499826f, 0.333578f,

                };

                Vao = new VertexArrayObject();
                vbo = new BufferObject<float>(vertices, BufferTarget.ArrayBuffer);

                Vao.LinkBufferObject(ref vbo);
                Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5 * sizeof(float), 0 * sizeof(float));
                Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5 * sizeof(float), 3 * sizeof(float));

            }

            Vao.Bind();
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }
        public static void Dispose() => Vao!.Dispose();
    }
    public class Faces
    {
        public string PathFaces = string.Empty;
        public List<string> Textures = new List<string>();
    }
    public struct TexturesCBMaps
    {
        public int Background;
        public int Irradiance;
        public int PreFilter;
    }
    public class CubeMap
    {
        public struct Handler
        {
            public int HDR_Texture;
            public int captureFrameBO;
            public int captureRenderBO;
            public int size;
            public PixelInternalFormat internalFormat;
            public CubeMapType type;
        }
        private static Matrix4 captureProjection = Matrix4.CreatePerspectiveFieldOfView((float)OpenTK.Mathematics.MathHelper.DegreesToRadians(90.0), 1.0f, 0.1f, 10.0f);
        private static Matrix4[] captureViews =
        {
            Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3( 1.0f,  0.0f,  0.0f),  new  Vector3(0.0f, -1.0f,  0.0f)),
            Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-1.0f,  0.0f,  0.0f),  new  Vector3(0.0f, -1.0f,  0.0f)),
            Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3( 0.0f,  1.0f,  0.0f),  new  Vector3(0.0f,  0.0f,  1.0f)),
            Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3( 0.0f, -1.0f,  0.0f),  new  Vector3(0.0f,  0.0f, -1.0f)),
            Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3( 0.0f,  0.0f,  1.0f),  new  Vector3(0.0f, -1.0f,  0.0f)),
            Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.0f), new Vector3( 0.0f,  0.0f, -1.0f),  new  Vector3(0.0f, -1.0f,  0.0f))
        };
        private ShaderProgram shaderRender = new ShaderProgram("./shaders/builtin/renderFinal.vert", "./shaders/builtin/renderFinal.frag");
        public TexturesCBMaps UseTextures = new TexturesCBMaps();
        public Handler handler;

        public CubeMap(Faces faces)
        {

            handler = new Handler()
            {
                size = 1920,
                internalFormat = PixelInternalFormat.Rgba32f,
                type = CubeMapType.Type1,
            };

            LoadFaces loadFaces = new LoadFaces(ref faces, ref handler, ref UseTextures);
            IrradianceMap irradianceMap = new IrradianceMap(ref handler, ref UseTextures);
            PreFilterMap preFilterMap = new PreFilterMap(ref handler, ref UseTextures, ref irradianceMap.shaderIrradiance);

            irradianceMap.Dispose();
            preFilterMap.Dispose();

            GL.DeleteFramebuffer(handler.captureFrameBO);
            GL.DeleteRenderbuffer(handler.captureRenderBO);

            GL.DeleteTexture(handler.HDR_Texture);
        }
        public CubeMap(string path, CubeMapType Type)
        {

            if (!File.Exists(path))
                throw new Exception($"Konnte die HDR Textur nicht laden: {path}");


            handler = new Handler()
            {
                size = 1920,
                internalFormat = PixelInternalFormat.Rgba32f,
                type = Type,
            };

            UseTextures = new TexturesCBMaps();

            LoadRectangularTexture rectangularTexture = new LoadRectangularTexture(path, ref handler);
            RetangularToCubemap retangularToCubemap = new RetangularToCubemap(ref handler, ref UseTextures);
            IrradianceMap irradianceMap = new IrradianceMap(ref handler, ref UseTextures);
            PreFilterMap preFilterMap = new PreFilterMap(ref handler, ref UseTextures, ref irradianceMap.shaderIrradiance);

            retangularToCubemap.Dispose();
            irradianceMap.Dispose();
            preFilterMap.Dispose();

            GL.DeleteFramebuffer(handler.captureFrameBO);
            GL.DeleteRenderbuffer(handler.captureRenderBO);

            GL.DeleteTexture(handler.HDR_Texture);
        }
        public void RenderFrame()
        {

            shaderRender.Use();
            var activeCam = SceneManager.ActiveScene.activeCam.GetComponent<Camera>();
           
            shaderRender.SetUniform("projection", activeCam.ProjectionMatrix);
            shaderRender.SetUniform("view", activeCam.ViewMatrix);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, UseTextures.Background);
            shaderRender.SetUniform("environmentMap", 0);

            shaderRender.SetUniform("gamma", 1.0f);
            shaderRender.SetUniform("interpolation", 0.9f);


            GL.Disable(EnableCap.CullFace);
            GL.DepthFunc(DepthFunction.Lequal);
            RenderCube(handler.type);
            GL.DepthFunc(DepthFunction.Less);
            GL.Enable(EnableCap.CullFace);
        }
        private static void RenderCube(CubeMapType type)
        {
            if (type == CubeMapType.Type0 | type == CubeMapType.Type1)
            {
                CubemapDefault.RenderCube();
            }
            else if (type == CubeMapType.Type2)
            {
                CubeMapCross.RenderCube();
            }
            else if (type == CubeMapType.Type3)
            {
                CubemapT.RenderCube();
            }
        }
        public void Dispose()
        {
            shaderRender.Dispose();


            GL.DeleteTexture(UseTextures.Background);
            GL.DeleteTexture(UseTextures.Irradiance);
            GL.DeleteTexture(UseTextures.PreFilter);

            CubemapDefault.Dispose();
            CubemapT.Dispose();
            CubeMapCross.Dispose();
        }

        public struct LoadFaces
        {
            public LoadFaces(ref Faces faces, ref Handler handler, ref TexturesCBMaps useTextures)
            {
                handler.captureFrameBO = GL.GenFramebuffer();
                handler.captureRenderBO = GL.GenRenderbuffer();

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, handler.captureFrameBO);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, handler.captureRenderBO);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, handler.size, handler.size);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, handler.captureRenderBO);


                useTextures.Background = GL.GenTexture();
                GL.BindTexture(TextureTarget.TextureCubeMap, useTextures.Background);

                StbImage.stbi_set_flip_vertically_on_load(1);

                for (int i = 0; i < 6; i++)
                {
                    var path = Path.Combine(faces.PathFaces, faces.Textures[i]);

                    if (!File.Exists(path))
                        throw new Exception($"Não foi possivel encontrar a Textura: {path}");

                    using (Stream stream = File.OpenRead(path))
                    {
                        var image = ImageResultFloat.FromStream(stream, ColorComponents.RedGreenBlue);

                        GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, handler.internalFormat,
                        image.Width, image.Height, 0, PixelFormat.Rgb, PixelType.Float, image.Data);
                    }
                }

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

            }
        }
        public struct LoadRectangularTexture
        {
            public LoadRectangularTexture(string path, ref Handler handler)
            {
                handler.captureFrameBO = GL.GenFramebuffer();
                handler.captureRenderBO = GL.GenRenderbuffer();

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, handler.captureFrameBO);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, handler.captureRenderBO);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, handler.size, handler.size);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, handler.captureRenderBO);


                handler.HDR_Texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, handler.HDR_Texture);

                StbImage.stbi_set_flip_vertically_on_load(1);
                using (Stream stream = File.OpenRead(path))
                {
                    ImageResultFloat image = ImageResultFloat.FromStream(stream, ColorComponents.RedGreenBlue);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, handler.internalFormat,
                        image.Width, image.Height, 0, PixelFormat.Rgb, PixelType.Float, image.Data);
                }

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            }
        }
        public struct RetangularToCubemap
        {
            public ShaderProgram shader;
            public void Dispose() => shader.Dispose();
            public RetangularToCubemap(ref Handler handler, ref TexturesCBMaps CBMapsUse)
            {
                shader = new ShaderProgram("./shaders/builtin/cubemap.vert", "./shaders/builtin/rectangular_to_cubemap.frag");

                CBMapsUse.Background = GL.GenTexture();

                GL.BindTexture(TextureTarget.TextureCubeMap, CBMapsUse.Background);

                for (int i = 0; i < 6; i++)
                {
                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, handler.internalFormat,
                    handler.size, handler.size, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
                }

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);


                shader.Use();
                shader.SetUniform("projection", captureProjection);
                shader.SetUniform("UseTexCoord", (int)handler.type);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, handler.HDR_Texture);
                shader.SetUniform("equirectangularMap", 0);

                GL.Viewport(0, 0, handler.size, handler.size);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, handler.captureFrameBO);

                for (int i = 0; i < 6; i++)
                {
                    shader.SetUniform("view", captureViews[i]);
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                        TextureTarget.TextureCubeMapPositiveX + i, CBMapsUse.Background, 0);

                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    RenderCube(handler.type);
                }
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
        }
        public struct IrradianceMap
        {
            public ShaderProgram shaderIrradiance;
            public void Dispose() => shaderIrradiance.Dispose();
            public IrradianceMap(ref Handler handler, ref TexturesCBMaps texturesCBMaps)
            {
                shaderIrradiance = new ShaderProgram("./shaders/builtin/cubemap.vert", "./shaders/builtin/irradiance.frag");

                texturesCBMaps.Irradiance = GL.GenTexture();
                GL.BindTexture(TextureTarget.TextureCubeMap, texturesCBMaps.Irradiance);

                for (int i = 0; i < 6; i++)
                {
                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, handler.internalFormat,
                    32, 32, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
                }

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, handler.captureFrameBO);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, handler.captureRenderBO);
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, 32, 32);

                shaderIrradiance.Use();
                shaderIrradiance.SetUniform("projection", captureProjection);

                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.TextureCubeMap, texturesCBMaps.Background);
                shaderIrradiance.SetUniform("environmentMap", 0);

                GL.Viewport(0, 0, 32, 32);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, handler.captureFrameBO);
                for (int i = 0; i < 6; i++)
                {
                    shaderIrradiance.SetUniform("view", captureViews[i]);
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                        TextureTarget.TextureCubeMapPositiveX + i, texturesCBMaps.Irradiance, 0);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    RenderCube(handler.type);

                }
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
        }
        public struct PreFilterMap
        {
            public ShaderProgram prefilterShader;
            public void Dispose() => prefilterShader.Dispose();
            public PreFilterMap(ref Handler handler, ref TexturesCBMaps texturesCBMaps, ref ShaderProgram shaderIrradiance)
            {
                prefilterShader = new ShaderProgram("./shaders/builtin/cubemap.vert", "./shaders/builtin/prefilter.frag");

                texturesCBMaps.PreFilter = GL.GenTexture();
                GL.BindTexture(TextureTarget.TextureCubeMap, texturesCBMaps.PreFilter);
                for (int i = 0; i < 6; i++)
                {
                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, handler.internalFormat,
                    128, 128, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
                }


                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);


                prefilterShader.Use();
                prefilterShader.SetUniform("projection", captureProjection);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.TextureCubeMap, texturesCBMaps.Background);
                shaderIrradiance.SetUniform("environmentMap", 0);

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, handler.captureFrameBO);
                int maxMipLevels = 5;
                for (int mip = 0; mip < maxMipLevels; mip++)
                {
                    int mipWidth = (int)(128 * OpenTK.Mathematics.MathHelper.Pow(0.5, mip));
                    int mipHeight = (int)(128 * OpenTK.Mathematics.MathHelper.Pow(0.5, mip));

                    GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, handler.captureRenderBO);
                    GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, mipWidth, mipHeight);
                    GL.Viewport(0, 0, mipWidth, mipHeight);

                    float roughness = (float)mip / (float)(maxMipLevels - 1);
                    prefilterShader.SetUniform("roughness", roughness);
                    for (int i = 0; i < 6; i++)
                    {
                        prefilterShader.SetUniform("view", captureViews[i]);
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                                                TextureTarget.TextureCubeMapPositiveX + i, texturesCBMaps.PreFilter, mip);

                        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                        RenderCube(handler.type);
                    }
                }
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
        }
    }
    public struct Quad
    {
        private static VertexArrayObject? Vao;
        private static BufferObject<float>? vbo;
        public static void RenderQuad()
        {
            if (Vao == null)
            {
                float[] vertices =
                {
                    // positions        // texture Coords
                    -1.0f,  1.0f, 0.0f, 0.0f, 1.0f,
                    -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
                     1.0f,  1.0f, 0.0f, 1.0f, 1.0f,
                     1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
                };

                Vao = new VertexArrayObject();
                vbo = new BufferObject<float>(vertices, BufferTarget.ArrayBuffer);
                Vao.LinkBufferObject(ref vbo);

                Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5 * sizeof(float), 0 * sizeof(float));
                Vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5 * sizeof(float), 3 * sizeof(float));
            }
            // render Cube
            Vao.Bind();
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }
        public static void Dispose() => Vao!.Dispose();
    }
    
    public class Physics
    {
        private static CollisionConfiguration collisionConfig = new DefaultCollisionConfiguration();
        private static CollisionDispatcher collisiondispatcher = new CollisionDispatcher(collisionConfig);
        private static DbvtBroadphase broadphase = new DbvtBroadphase();

        // External uses
        public static DiscreteDynamicsWorld World { get; } = new DiscreteDynamicsWorld(collisiondispatcher, broadphase, null, collisionConfig);
        public static AlignedCollisionObjectArray ObjectsArray { get => World.CollisionObjectArray; }


        private static float _gravity = -9.807f;
        public static float Gravity
        {
            get => _gravity;
            set
            {
                _gravity = -value;
                World.Gravity = new BulletSharp.Math.Vector3(0.0f, _gravity, 0.0f);
            }
        }

        public static void Step()
        {
            World.StepSimulation(Time.deltaTime);
        }

        public static void MakePlane()
        {
            CollisionShape groundShape = new StaticPlaneShape(new BulletSharp.Math.Vector3(0, 1, 0), -50);
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(), groundShape);
            RigidBody groundBody = new RigidBody(rbInfo);
            World.AddRigidBody(groundBody);
        }

        public static RigidBody CreateStaticRigidBody(Model model, BulletSharp.Math.Matrix transform, string name)
        {
            // Create the ConvexHullShape from the model's points
            ConvexHullShape shape = new ConvexHullShape(model.assimpModel.PointsForCollision.ToArray());

            // Apply the scale transformation to the shape
            shape.LocalScaling = Matrix.Transpose(transform).ScaleVector;

            DefaultMotionState myMotionState = new DefaultMotionState(Matrix.Transpose(transform));

            // Set the mass to zero to create a static rigid body
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0.0f, myMotionState, shape);

            RigidBody body = new RigidBody(rbInfo);

            body.UserObject = name;

            World.AddRigidBody(body);

            return body;
        }

        public static CollisionObject CreateStaticCollider(Model model, BulletSharp.Math.Matrix transform, GameObject name)
        {
            // Create a collision shape from the model (adjust as needed)
            CollisionShape shape = new ConvexHullShape(model.assimpModel.PointsForCollision.ToArray());
            shape.LocalScaling = Matrix.Transpose(transform).ScaleVector;

            // Create a DefaultMotionState with the provided transform
            DefaultMotionState motionState = new DefaultMotionState(Matrix.Transpose(transform));

            // Create a RigidBodyConstructionInfo with zero mass (for a static object)
            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0f, motionState, shape);

            // Create a RigidBody
            RigidBody rigidBody = new RigidBody(rbInfo);

            // Set the collision flags to indicate it's a static object
            rigidBody.CollisionFlags |= CollisionFlags.StaticObject;

            // Set the user object (if needed)
            rigidBody.UserObject = name;

            // Create a CollisionObject using the RigidBody
            CollisionObject collider = rigidBody;
            World.AddCollisionObject(collider);
            return collider;
        }





        public static RigidBody CreateRigidBody(Model model, float mass, BulletSharp.Math.Matrix transform, GameObject name)
        {
            BulletSharp.Math.Vector3 localInertia = BulletSharp.Math.Vector3.Zero;
            var shape = new ConvexHullShape(model.assimpModel.PointsForCollision.ToArray());
            shape.LocalScaling = Matrix.Transpose(transform).ScaleVector;

            if (mass > 0.0)
            {
                localInertia = shape.CalculateLocalInertia(mass);
            }

            DefaultMotionState myMotionState = new DefaultMotionState(Matrix.Transpose(transform));

            RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, myMotionState, shape, localInertia);

            RigidBody body = new RigidBody(rbInfo);

            body.UserObject = name;

            World.AddRigidBody(body);
            rbInfo.Dispose();
            return body;
        }

        public static bool Raycast(System.Numerics.Vector3 start, System.Numerics.Vector3 direction, out System.Numerics.Vector3 hitPoint, out System.Numerics.Vector3 hitNormal, out CollisionObject hitObject)
        {
            var bsStart = LMath.ToVecBs(start);
            var bsDirection = LMath.ToVecBs(direction);

            var bspd = bsStart + bsDirection;

            ClosestRayResultCallback rayCallback = new ClosestRayResultCallback(ref bsStart, ref bspd);

            World.RayTest(bsStart, bspd, rayCallback);

            if (rayCallback.HasHit)
            {
                hitPoint = LMath.ToVec(rayCallback.HitPointWorld);
                hitNormal = LMath.ToVec(rayCallback.HitNormalWorld);
                hitObject = rayCallback.CollisionObject;
                return true;
            }
            else
            {
                hitPoint = LMath.ToVec(Vector3.Zero);
                hitNormal = LMath.ToVec(Vector3.Zero);
                hitObject = null;
                return false;
            }
        }

        public static RigidBody GetRigidBodyPositionByUserObjectName(GameObject userObjectName)
        {
            foreach (CollisionObject obj in World.CollisionObjectArray)
            {
                // Check if the CollisionObject has a RigidBody and a matching user object name
                if (obj is RigidBody rigidBody && obj.UserObject != null && obj.UserObject == userObjectName)
                {
                    // Get the position of the RigidBody
                    return rigidBody;
                }
            }

            // If the user object name is not found, return a default position or handle the case accordingly
            return null;
        }

       

    }
    
    public class Model : IDisposable
    {
        public AssimpModel assimpModel;
        public List<Meshe> meshes;
        public ShaderProgram ShaderPBR;
        private Dictionary<string, TextureProgram> TexturesMap = new Dictionary<string, TextureProgram>();

        public Model(string modelPath)
        {
            assimpModel = new AssimpModel(modelPath);
            meshes = new List<Meshe>(assimpModel.meshes);

            //ShaderPBR = new ShaderProgram("./shaders/builtin/pbr.vert", "./shaders/builtin/colored.frag");
            ShaderPBR = new ShaderProgram("./shaders/builtin/pbr.vert", "./shaders/builtin/pbr.frag");

            foreach (var index in meshes.OrderBy(x => x.Vao))
            {
                LoadTextures(index.DiffusePath, PixelInternalFormat.SrgbAlpha, TextureUnit.Texture4);
                LoadTextures(index.NormalPath, PixelInternalFormat.Rgba, TextureUnit.Texture5);
                LoadTextures(index.LightMap, PixelInternalFormat.Rgba, TextureUnit.Texture6);
                LoadTextures(index.EmissivePath, PixelInternalFormat.SrgbAlpha, TextureUnit.Texture7);
                LoadTextures(index.SpecularPath, PixelInternalFormat.Rgba, TextureUnit.Texture8);
                LoadTextures(index.HeightMap, PixelInternalFormat.Rgba, TextureUnit.Texture9);
                LoadTextures(index.MetallicPath, PixelInternalFormat.Rgba, TextureUnit.Texture10);
                LoadTextures(index.RoughnnesPath, PixelInternalFormat.Rgba, TextureUnit.Texture11);
                LoadTextures(index.AmbientOcclusionPath, PixelInternalFormat.Rgba, TextureUnit.Texture12);
            }

        }

        public TexturesCBMaps UseTexCubemap;

        public void RenderFrame(TransformComponent trans, Camera cam)
        {
            ShaderPBR.Use();
            ShaderPBR.SetUniform("model", trans.GetTransformMatrix());
            ShaderPBR.SetUniform("view", cam.ViewMatrix);
            ShaderPBR.SetUniform("projection", cam.ProjectionMatrix);
            
            //von hier mit pbr
            
            ShaderPBR.SetUniform("viewPos", cam.Position);

            ShaderPBR.SetUniform("lightPositions", new Vector3(0,5,0));
            ShaderPBR.SetUniform("lightColors", new Vector3(1f,0f,0f));

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, UseTexCubemap.Irradiance);
            ShaderPBR.SetUniform("irradianceMap", 1);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.TextureCubeMap, UseTexCubemap.Background);
            ShaderPBR.SetUniform("backgroundMap", 1);

            ShaderPBR.SetUniform("gammaCubemap", 25.0f);
            ShaderPBR.SetUniform("interpolation", 25f);

            ShaderPBR.SetUniform("emissiveStrength", 15f);

            //ShaderPBR.SetUniform("gamma", 1.5f);
            ShaderPBR.SetUniform("luminousStrength", 25.0f);
            ShaderPBR.SetUniform("specularStrength", 35.5f);
            
            //bis hier für ohne pbr

            GL.Enable(EnableCap.CullFace);


            foreach (var item in meshes)
            {

                if (TexturesMap.ContainsKey(item.DiffusePath))
                {
                    ShaderPBR.SetUniform("AlbedoMap", TexturesMap[item.DiffusePath].Use);
                }

                if (TexturesMap.ContainsKey(item.NormalPath))
                {
                    ShaderPBR.SetUniform("NormalMap", TexturesMap[item.NormalPath].Use); //das
                }

                if (TexturesMap.ContainsKey(item.LightMap))
                {
                    ShaderPBR.SetUniform("AmbienteRoughnessMetallic", TexturesMap[item.LightMap].Use); //das
                }

                if (TexturesMap.ContainsKey(item.EmissivePath))
                {
                    ShaderPBR.SetUniform("EmissiveMap", TexturesMap[item.EmissivePath].Use); //das
                }



                item.RenderFrame();
            }
            GL.Disable(EnableCap.CullFace);



        }


        public void RenderForStencil()
        {
            //bloom scheiße
            if (Stencil.RenderStencil)
            {

                foreach (var item in meshes.OrderBy(x => x.Vao))
                {
                    item.RenderFrame();
 
                }

            }
        }

        public void Dispose()
        {
            for (int i = 0; i < meshes.Count; i++)
                meshes[i].Dispose();

            foreach (var index in TexturesMap.Keys)
                TexturesMap[index].Dispose();

            ShaderPBR.Dispose();
        }
        private void LoadTextures(string tex_path, PixelInternalFormat pixelFormat, TextureUnit unit)
        {
            if (!TexturesMap.ContainsKey(tex_path))
            {
                if (tex_path != string.Empty)
                {
                    TextureProgram _texture_map = new TextureProgram(tex_path, pixelFormat, unit);
                    TexturesMap.Add(tex_path, _texture_map);
                }
            }

        }
    }
    public class Meshe : IDisposable
    {
        // private BuffersVertex buffers;
        private int indicesCount;
        public string DiffusePath, SpecularPath, NormalPath, HeightMap, MetallicPath, RoughnnesPath, LightMap, EmissivePath, AmbientOcclusionPath;

        public VertexArrayObject Vao;
        private BufferObject<Vertex> Vbo;
        private BufferObject<ushort> Ebo;
        public unsafe Meshe(List<Vertex> Vertices, List<ushort> Indices, ModelTexturesPath texturesPath)
        {

            indicesCount = Indices.Count;


            DiffusePath = texturesPath._DiffusePath;
            SpecularPath = texturesPath._SpecularPath;
            NormalPath = texturesPath._NormalPath;
            HeightMap = texturesPath._HeightPath;
            MetallicPath = texturesPath._MetallicPath;
            RoughnnesPath = texturesPath._RoughnnesPath;
            LightMap = texturesPath._LightMap;
            EmissivePath = texturesPath._EmissivePath;
            AmbientOcclusionPath = texturesPath._AmbientOcclusionPath;

            Vao = new VertexArrayObject();
            Vbo = new BufferObject<Vertex>(Vertices.ToArray(), BufferTarget.ArrayBuffer);
            Vao.LinkBufferObject(ref Vbo);

            Vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, sizeof(Vertex), IntPtr.Zero);
            Vao.VertexAttributePointer(1, 3, VertexAttribPointerType.Float, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "Normals"));
            Vao.VertexAttributePointer(2, 2, VertexAttribPointerType.Float, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "TexCoords"));
            Vao.VertexAttributePointer(3, 3, VertexAttribPointerType.Float, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "Tangents"));
            Vao.VertexAttributePointer(4, 3, VertexAttribPointerType.Float, sizeof(Vertex), Marshal.OffsetOf(typeof(Vertex), "Bitangents"));

            Ebo = new BufferObject<ushort>(Indices.ToArray(), BufferTarget.ElementArrayBuffer);
            Vao.LinkBufferObject(ref Ebo);

            Vertices.Clear();
            Indices.Clear();

        }
        public void RenderFrame()
        {
            Vao.Bind();

            GL.DrawElements(PrimitiveType.Triangles, indicesCount, DrawElementsType.UnsignedShort, 0);
        }
        public void Dispose() => Vao.Dispose();
        public void PrintTexturesMap()
        {
            Console.WriteLine("Meshes Maps Contains...");
            _Print("DiffusePath", DiffusePath);
            _Print("SpecularPath", SpecularPath);
            _Print("NormalPath", NormalPath);
            _Print("HeightMap", HeightMap);
            _Print("MetallicPath", MetallicPath);
            _Print("RoughnnesPath", RoughnnesPath);
            _Print("LightMap", LightMap);
            _Print("EmissivePath", EmissivePath);
            _Print("AmbientOcclusionPath", AmbientOcclusionPath);
            Console.WriteLine("\n -------------------------------------------------------------- \n");
        }
        private void _Print(string TypeTexture, string pathTex)
        {
            if (pathTex != string.Empty)
                Console.WriteLine($"{TypeTexture} : {pathTex}");
        }
    }
    public class VertexArrayObject : IDisposable, IComparable<VertexArrayObject>
    {
        public int Handle;
        private List<int> buffersLinked;
        public VertexArrayObject()
        {
            Handle = GL.GenVertexArray();
            buffersLinked = new List<int>();
        }
        public void LinkBufferObject<TDataType>(ref BufferObject<TDataType> bufferObject) where TDataType : unmanaged
        {
            Bind();
            bufferObject.Bind();
            buffersLinked.Add(bufferObject.Handle);
        }
        public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, int vertexSize, int offSet)
        {
            GL.VertexAttribPointer(index, count, type, false, vertexSize, offSet);
            GL.EnableVertexAttribArray(index);
        }
        public unsafe void VertexAttributePointer(uint index, int count, VertexAttribPointerType type, int vertexSize, IntPtr offSet)
        {
            GL.VertexAttribPointer(index, count, type, false, vertexSize, (int)offSet);
            GL.EnableVertexAttribArray(index);
        }
        public void Bind() => GL.BindVertexArray(Handle);

        public void Dispose()
        {
            GL.DeleteVertexArray(Handle);
            GL.DeleteBuffers(buffersLinked.Count, buffersLinked.ToArray());
        }

        public int CompareTo(VertexArrayObject? obj)
        {
            if(obj != null)
            {
                if (Handle <= obj.Handle)
                {
                    return 1;
                }
            }
            
            return 0;
        }
    }
    public class BufferObject<TDataType>
   where TDataType : unmanaged
    {
        public int Handle { get; private set; }
        private BufferTarget bufferTarget;

        public unsafe BufferObject(Span<TDataType> data, BufferTarget bufferTarget)
        {
            this.bufferTarget = bufferTarget;

            Handle = GL.GenBuffer();
            Bind();
            GL.BufferData(bufferTarget, data.Length * sizeof(TDataType), data.ToArray(), BufferUsageHint.StaticDraw);
        }
        public unsafe BufferObject(int amount, BufferTarget bufferTarget)
        {
            this.bufferTarget = bufferTarget;

            Handle = GL.GenBuffer();
            Bind();
            GL.BufferData(bufferTarget, amount * sizeof(TDataType), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }
        public unsafe void SuberData(Span<TDataType> data)
        {
            Bind();
            GL.BufferSubData(bufferTarget, IntPtr.Zero, data.Length * sizeof(TDataType), data.ToArray());

        }
        public unsafe void SuberData(TDataType[,] data)
        {
            Bind();
            GL.BufferSubData(bufferTarget, IntPtr.Zero, data.Length * sizeof(TDataType), data);

        }
        public void Bind() => GL.BindBuffer(bufferTarget, Handle);
    }
    public class ImGuiController : IDisposable
    {
        private bool _frameBegun;

        private int _vertexArray;
        private int _vertexBuffer;
        private int _vertexBufferSize;
        private int _indexBuffer;
        private int _indexBufferSize;

        //private Texture _fontTexture;

        private int _fontTexture;

        private int _shader;
        private int _shaderFontTextureLocation;
        private int _shaderProjectionMatrixLocation;

        private int _windowWidth;
        private int _windowHeight;

        private System.Numerics.Vector2 _scaleFactor = System.Numerics.Vector2.One;

        private static bool KHRDebugAvailable = false;

        private int GLVersion;
        private bool CompatibilityProfile;

        /// <summary>
        /// Constructs a new ImGuiController.
        /// </summary>
        public ImGuiController(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;

            int major = GL.GetInteger(GetPName.MajorVersion);
            int minor = GL.GetInteger(GetPName.MinorVersion);

            GLVersion = major * 100 + minor * 10;

            KHRDebugAvailable = (major == 4 && minor >= 3) || IsExtensionSupported("KHR_debug");

            CompatibilityProfile = (GL.GetInteger((GetPName)All.ContextProfileMask) & (int)All.ContextCompatibilityProfileBit) != 0;

            IntPtr context = ImGui.CreateContext();
            //ImGuizmo.SetImGuiContext(context);
            ImGuizmo.SetImGuiContext(context);
            
            ImGui.SetCurrentContext(context);

            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();

            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            CreateDeviceResources();
            SetKeyMappings();

            SetPerFrameImGuiData(1f / 60f);

            //ImGuizmo.BeginFrame();
            ImGui.NewFrame();
            //ImGuizmo.BeginFrame();
            _frameBegun = true;
        }

        public void WindowResized(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources()
        {
            _vertexBufferSize = 10000;
            _indexBufferSize = 2000;

            int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);

            _vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArray);
            LabelObject(ObjectLabelIdentifier.VertexArray, _vertexArray, "ImGui");

            _vertexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            LabelObject(ObjectLabelIdentifier.Buffer, _vertexBuffer, "VBO: ImGui");
            GL.BufferData(BufferTarget.ArrayBuffer, _vertexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            _indexBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indexBuffer);
            LabelObject(ObjectLabelIdentifier.Buffer, _indexBuffer, "EBO: ImGui");
            GL.BufferData(BufferTarget.ElementArrayBuffer, _indexBufferSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            RecreateFontDeviceTexture();

            string VertexSource = @"#version 330 core

uniform mat4 projection_matrix;

layout(location = 0) in vec2 in_position;
layout(location = 1) in vec2 in_texCoord;
layout(location = 2) in vec4 in_color;

out vec4 color;
out vec2 texCoord;

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = in_color;
    texCoord = in_texCoord;
}";
            string FragmentSource = @"#version 330 core

uniform sampler2D in_fontTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture(in_fontTexture, texCoord);
}";

            _shader = CreateProgram("ImGui", VertexSource, FragmentSource);
            _shaderProjectionMatrixLocation = GL.GetUniformLocation(_shader, "projection_matrix");
            _shaderFontTextureLocation = GL.GetUniformLocation(_shader, "in_fontTexture");

            int stride = Unsafe.SizeOf<ImDrawVert>();
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 8);
            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.UnsignedByte, true, stride, 16);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.BindVertexArray(prevVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);

            CheckGLError("End of ImGui setup");
        }

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public void RecreateFontDeviceTexture()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixels, out int width, out int height, out int bytesPerPixel);

            int mips = (int)Math.Floor(Math.Log(Math.Max(width, height), 2));

            int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);

            _fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
            GL.TexStorage2D(TextureTarget2d.Texture2D, mips, SizedInternalFormat.Rgba8, width, height);
            LabelObject(ObjectLabelIdentifier.Texture, _fontTexture, "ImGui Text Atlas");

            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, pixels);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, mips - 1);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

            // Restore state
            GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
            GL.ActiveTexture((TextureUnit)prevActiveTexture);

            io.Fonts.SetTexID((IntPtr)_fontTexture);

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public void Render()
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
  
                RenderImDrawData(ImGui.GetDrawData());
          
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(GameWindow wnd, float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
            UpdateImGuiInput(wnd);

            _frameBegun = true;
            ImGui.NewFrame();
            
           
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new System.Numerics.Vector2(
                _windowWidth / _scaleFactor.X,
                _windowHeight / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        readonly List<char> PressedChars = new List<char>();

        private void UpdateImGuiInput(GameWindow wnd)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            MouseState MouseState = wnd.MouseState;
            KeyboardState KeyboardState = wnd.KeyboardState;

            io.MouseDown[0] = MouseState[MouseButton.Left];
            io.MouseDown[1] = MouseState[MouseButton.Right];
            io.MouseDown[2] = MouseState[MouseButton.Middle];
            io.MouseDown[3] = MouseState[MouseButton.Button4];
            io.MouseDown[4] = MouseState[MouseButton.Button5];

            var screenPoint = new Vector2i((int)MouseState.X, (int)MouseState.Y);
            var point = screenPoint;//wnd.PointToClient(screenPoint);
            io.MousePos = new System.Numerics.Vector2(point.X, point.Y);

            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                if (key == Keys.Unknown)
                {
                    continue;
                }
                io.KeysDown[(int)key] = KeyboardState.IsKeyDown(key);
            }

            foreach (var c in PressedChars)
            {
                io.AddInputCharacter(c);
            }
            PressedChars.Clear();

            io.KeyCtrl = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
            io.KeyAlt = KeyboardState.IsKeyDown(Keys.LeftAlt) || KeyboardState.IsKeyDown(Keys.RightAlt);
            io.KeyShift = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
            io.KeySuper = KeyboardState.IsKeyDown(Keys.LeftSuper) || KeyboardState.IsKeyDown(Keys.RightSuper);
        }

        internal void PressChar(char keyChar)
        {
            PressedChars.Add(keyChar);
        }

        internal void MouseScroll(Vector2 offset)
        {
            ImGuiIOPtr io = ImGui.GetIO();

            io.MouseWheel = offset.Y;
            io.MouseWheelH = offset.X;
        }

        private static void SetKeyMappings()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab;
            io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left;
            io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right;
            io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up;
            io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down;
            io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp;
            io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown;
            io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home;
            io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End;
            io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete;
            io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Backspace;
            io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter;
            io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape;
            io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A;
            io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C;
            io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V;
            io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X;
            io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y;
            io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z;
        }

        private void RenderImDrawData(ImDrawDataPtr draw_data)
        {
            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            // Get intial state.
            int prevVAO = GL.GetInteger(GetPName.VertexArrayBinding);
            int prevArrayBuffer = GL.GetInteger(GetPName.ArrayBufferBinding);
            int prevProgram = GL.GetInteger(GetPName.CurrentProgram);
            bool prevBlendEnabled = GL.GetBoolean(GetPName.Blend);
            bool prevScissorTestEnabled = GL.GetBoolean(GetPName.ScissorTest);
            int prevBlendEquationRgb = GL.GetInteger(GetPName.BlendEquationRgb);
            int prevBlendEquationAlpha = GL.GetInteger(GetPName.BlendEquationAlpha);
            int prevBlendFuncSrcRgb = GL.GetInteger(GetPName.BlendSrcRgb);
            int prevBlendFuncSrcAlpha = GL.GetInteger(GetPName.BlendSrcAlpha);
            int prevBlendFuncDstRgb = GL.GetInteger(GetPName.BlendDstRgb);
            int prevBlendFuncDstAlpha = GL.GetInteger(GetPName.BlendDstAlpha);
            bool prevCullFaceEnabled = GL.GetBoolean(GetPName.CullFace);
            bool prevDepthTestEnabled = GL.GetBoolean(GetPName.DepthTest);
            int prevActiveTexture = GL.GetInteger(GetPName.ActiveTexture);
            GL.ActiveTexture(TextureUnit.Texture0);
            int prevTexture2D = GL.GetInteger(GetPName.TextureBinding2D);
            Span<int> prevScissorBox = stackalloc int[4];
            unsafe
            {
                fixed (int* iptr = &prevScissorBox[0])
                {
                    GL.GetInteger(GetPName.ScissorBox, iptr);
                }
            }
            Span<int> prevPolygonMode = stackalloc int[2];
            unsafe
            {
                fixed (int* iptr = &prevPolygonMode[0])
                {
                    GL.GetInteger(GetPName.PolygonMode, iptr);
                }
            }

            if (GLVersion <= 310 || CompatibilityProfile)
            {
                GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                GL.PolygonMode(MaterialFace.Back, PolygonMode.Fill);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            }

            // Bind the element buffer (thru the VAO) so that we can resize it.
            GL.BindVertexArray(_vertexArray);
            // Bind the vertex buffer so that we can resize it.
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[i];

                int vertexSize = cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                if (vertexSize > _vertexBufferSize)
                {
                    int newSize = (int)Math.Max(_vertexBufferSize * 1.5f, vertexSize);

                    GL.BufferData(BufferTarget.ArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _vertexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui vertex buffer to new size {_vertexBufferSize}");
                }

                int indexSize = cmd_list.IdxBuffer.Size * sizeof(ushort);
                if (indexSize > _indexBufferSize)
                {
                    int newSize = (int)Math.Max(_indexBufferSize * 1.5f, indexSize);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, newSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);
                    _indexBufferSize = newSize;

                    Console.WriteLine($"Resized dear imgui index buffer to new size {_indexBufferSize}");
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4 mvp = Matrix4.CreateOrthographicOffCenter(
                0.0f,
                io.DisplaySize.X,
                io.DisplaySize.Y,
                0.0f,
                -1.0f,
                1.0f);

            GL.UseProgram(_shader);
            GL.UniformMatrix4(_shaderProjectionMatrixLocation, false, ref mvp);
            GL.Uniform1(_shaderFontTextureLocation, 0);
            CheckGLError("Projection");

            GL.BindVertexArray(_vertexArray);
            CheckGLError("VAO");

            draw_data.ScaleClipRects(io.DisplayFramebufferScale);

            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.ScissorTest);
            GL.BlendEquation(BlendEquationMode.FuncAdd);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);

            // Render command lists
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdListsRange[n];

                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, cmd_list.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>(), cmd_list.VtxBuffer.Data);
                CheckGLError($"Data Vert {n}");

                GL.BufferSubData(BufferTarget.ElementArrayBuffer, IntPtr.Zero, cmd_list.IdxBuffer.Size * sizeof(ushort), cmd_list.IdxBuffer.Data);
                CheckGLError($"Data Idx {n}");

                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, (int)pcmd.TextureId);
                        CheckGLError("Texture");

                        // We do _windowHeight - (int)clip.W instead of (int)clip.Y because gl has flipped Y when it comes to these coordinates
                        var clip = pcmd.ClipRect;
                        GL.Scissor((int)clip.X, _windowHeight - (int)clip.W, (int)(clip.Z - clip.X), (int)(clip.W - clip.Y));
                        CheckGLError("Scissor");

                        if ((io.BackendFlags & ImGuiBackendFlags.RendererHasVtxOffset) != 0)
                        {
                            GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (IntPtr)(pcmd.IdxOffset * sizeof(ushort)), unchecked((int)pcmd.VtxOffset));
                        }
                        else
                        {
                            GL.DrawElements(BeginMode.Triangles, (int)pcmd.ElemCount, DrawElementsType.UnsignedShort, (int)pcmd.IdxOffset * sizeof(ushort));
                        }
                        CheckGLError("Draw");
                    }
                }
            }

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.ScissorTest);

            // Reset state
            GL.BindTexture(TextureTarget.Texture2D, prevTexture2D);
            GL.ActiveTexture((TextureUnit)prevActiveTexture);
            GL.UseProgram(prevProgram);
            GL.BindVertexArray(prevVAO);
            GL.Scissor(prevScissorBox[0], prevScissorBox[1], prevScissorBox[2], prevScissorBox[3]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, prevArrayBuffer);
            GL.BlendEquationSeparate((BlendEquationMode)prevBlendEquationRgb, (BlendEquationMode)prevBlendEquationAlpha);
            GL.BlendFuncSeparate(
                (BlendingFactorSrc)prevBlendFuncSrcRgb,
                (BlendingFactorDest)prevBlendFuncDstRgb,
                (BlendingFactorSrc)prevBlendFuncSrcAlpha,
                (BlendingFactorDest)prevBlendFuncDstAlpha);
            if (prevBlendEnabled) GL.Enable(EnableCap.Blend); else GL.Disable(EnableCap.Blend);
            if (prevDepthTestEnabled) GL.Enable(EnableCap.DepthTest); else GL.Disable(EnableCap.DepthTest);
            if (prevCullFaceEnabled) GL.Enable(EnableCap.CullFace); else GL.Disable(EnableCap.CullFace);
            if (prevScissorTestEnabled) GL.Enable(EnableCap.ScissorTest); else GL.Disable(EnableCap.ScissorTest);
            if (GLVersion <= 310 || CompatibilityProfile)
            {
                GL.PolygonMode(MaterialFace.Front, (PolygonMode)prevPolygonMode[0]);
                GL.PolygonMode(MaterialFace.Back, (PolygonMode)prevPolygonMode[1]);
            }
            else
            {
                GL.PolygonMode(MaterialFace.FrontAndBack, (PolygonMode)prevPolygonMode[0]);
            }
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteVertexArray(_vertexArray);
            GL.DeleteBuffer(_vertexBuffer);
            GL.DeleteBuffer(_indexBuffer);

            GL.DeleteTexture(_fontTexture);
            GL.DeleteProgram(_shader);
        }

        public static void LabelObject(ObjectLabelIdentifier objLabelIdent, int glObject, string name)
        {
            if (KHRDebugAvailable)
                GL.ObjectLabel(objLabelIdent, glObject, name.Length, name);
        }

        static bool IsExtensionSupported(string name)
        {
            int n = GL.GetInteger(GetPName.NumExtensions);
            for (int i = 0; i < n; i++)
            {
                string extension = GL.GetString(StringNameIndexed.Extensions, i);
                if (extension == name) return true;
            }

            return false;
        }

        public static int CreateProgram(string name, string vertexSource, string fragmentSoruce)
        {
            int program = GL.CreateProgram();
            LabelObject(ObjectLabelIdentifier.Program, program, $"Program: {name}");

            int vertex = CompileShader(name, ShaderType.VertexShader, vertexSource);
            int fragment = CompileShader(name, ShaderType.FragmentShader, fragmentSoruce);

            GL.AttachShader(program, vertex);
            GL.AttachShader(program, fragment);

            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetProgramInfoLog(program);
                Debug.WriteLine($"GL.LinkProgram had info log [{name}]:\n{info}");
            }

            GL.DetachShader(program, vertex);
            GL.DetachShader(program, fragment);

            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);

            return program;
        }

        private static int CompileShader(string name, ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);
            LabelObject(ObjectLabelIdentifier.Shader, shader, $"Shader: {name}");

            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string info = GL.GetShaderInfoLog(shader);
                Debug.WriteLine($"GL.CompileShader for shader '{name}' [{type}] had info log:\n{info}");
            }

            return shader;
        }

        public static void CheckGLError(string title)
        {
            ErrorCode error;
            int i = 1;
            while ((error = GL.GetError()) != ErrorCode.NoError)
            {
                Debug.Print($"{title} ({i++}): {error}");
            }
        }
    }

    public abstract class NoesisUI : IRenderLayer
    {
        public Renderer.RenderLayerType LayerType => Renderer.RenderLayerType.GLRender; //just use gl layer for "other stuff"

        public Noesis.View view = null;

        private string xml = string.Empty;


        public NoesisUI(string xml)
        {
            this.xml = xml;
            InitView();
        }

        public Noesis.View GetView()
        {
            return view;
        }

        private void InitView()
        {
            Noesis.Log.SetLogCallback((level, channel, message) =>
            {
                if (channel == "")
                {
                    // [TRACE] [DEBUG] [INFO] [WARNING] [ERROR]
                    string[] prefixes = new string[] { "T", "D", "I", "W", "E" };
                    string prefix = (int)level < prefixes.Length ? prefixes[(int)level] : " ";
                    Logger.Log("[NOESIS/" + prefix + "] " + message);
                }
            });

            Noesis.GUI.SetLicense("JupesMod", "zRS01y1YtNeiVPBdnxxcJT2NEhJU4fzspf2DItbz0iURquRG");

            Noesis.GUI.Init();

            Noesis.Grid xaml = (Noesis.Grid)Noesis.GUI.ParseXaml(xml);

            // View creation to render and interact with the user interface
            // We transfer the ownership to a global pointer instead of a Ptr<> because there is no way
            // in GLUT to do shutdown and we don't want the Ptr<> to be released at global time
            view = Noesis.GUI.CreateView(xaml);
            //view.SetIs

            // Renderer initialization with an OpenGL device
            view.Renderer.Init(new Noesis.RenderDeviceGL());
        }

        public void Render()
        {
            view.SetSize(Engine.Renderer.Size.X, Engine.Renderer.Size.Y);
            view.Update(Time.time);
            view.Renderer.UpdateRenderTree();
            view.Renderer.RenderOffscreen();
            view.Renderer.Render();
        }
    }


    public class Renderer : GameWindow
    {
        public CubeMap cubeMap;
        private ViewPort crossHair;
        private Bloom bloom;

        public Renderer(int width, int height, string title) : base(GameWindowSettings.Default, new NativeWindowSettings() { Size = (width, height), Title = title })
        {
            IMGUIController = new ImGuiController(Size.X, Size.Y);
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

            // Load the image using StbImageSharp
            using (var stream = File.OpenRead(imagePath))
            {
                var result = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                var windowIcon = new WindowIcon(new OpenTK.Windowing.Common.Input.Image(result.Width, result.Height, result.Data));

                return windowIcon;
            }
        }


        protected override void OnLoad()
        {
            base.OnLoad();

            Icon = CreateWindowIcon();
            var timer = new Stopwatch();
            timer.Start();

            Console = new DebugConsole(this);
            AddLayer(Console);

            Logger.Log("Starting Jupe's Mod..");
            Logger.Log("Deleting loaded lupk files..");
            PackageLoader.UnloadPaks();

            Logger.Log("Loading lupks..");

            Engine.PackageLoader.LoadPaks();
            timer.Stop();

            crossHair = new ViewPort("./resources/img/crosshair.png");
            cubeMap = new CubeMap("./resources/Cubemap/industrial_sunset_puresky_4k.hdr", CubeMapType.Type1);
            bloom = new Bloom();
            Physics.MakePlane();
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.FramebufferSrgb);
            GL.Enable(EnableCap.Multisample);
            GL.Enable(EnableCap.TextureCubeMapSeamless);

            GL.Enable(EnableCap.ProgramPointSize);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.ClearColor(Color4.Black);

            GL.Enable(EnableCap.LineSmooth);
            GL.Enable(EnableCap.ColorSum);
            Logger.Log($"Jupe's Mod Loaded in {timer.ElapsedMilliseconds / 1000}sec, press any key to exit..");
            
            Logger.Log("Loading scene..");
            SceneManager.LoadScene("Demo Scene");
            Logger.Log("Scene loaded!");
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            // Update the opengl viewport
            GL.Viewport(0, 0, ClientSize.X, ClientSize.Y);

            IMGUIController.WindowResized(ClientSize.X, ClientSize.Y);
        }

        bool isgrabbed = false;



        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            SceneManager.ActiveScene.NetMerge();
            IMGUIController.Update(this, (float)e.Time);
            // Calculate Delta Time (time between frames).
            float deltaTime = (float)e.Time;
            Time.deltaTime = deltaTime;
            Time.time += deltaTime * Time.timeScale;

            Physics.Step();

            SceneManager.ActiveScene.cache.PhysicsPass();

            SceneManager.ActiveScene.cache.UpdatePass();




        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);


            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            if (SceneManager.ActiveScene.activeCam != null && SceneManager.ActiveScene.activeCam.GetComponent<Camera>() != null)
            {
  

                cubeMap.RenderFrame();
                SceneManager.ActiveScene.cache.RenderPass();

    
                crossHair.RenderFrame(Vector2.Zero, 0.03f);
               
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






            SwapBuffers();
            Net.SendSceneToClients();
        }

        protected override void OnTextInput(TextInputEventArgs e)
        {
            base.OnTextInput(e);
            IMGUIController.PressChar(e.AsString[0]);
           
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            var mouseState = InputManager.GetMouse();
            if (renderLayers.Count != 0)
            {
                for (int i = renderLayers.Count() - 1; i >= 0; i--)
                {
                    
                    var renderLayer = renderLayers[i];
                    if(renderLayer is NoesisUI ngui)
                    {
                     
                        
                        ngui.GetView().MouseMove((int)mouseState.Delta.X, (int)mouseState.Delta.Y);
                    }

           
                }
            }
        }

        protected override void OnMouseDown(OpenTK.Windowing.Common.MouseButtonEventArgs e)
        {
            var mouseState = InputManager.GetMouse();

            for (int i = renderLayers.Count() - 1; i >= 0; i--)
            {

                var renderLayer = renderLayers[i];
                if (renderLayer is NoesisUI ngui)
                {
                    if (e.Button == MouseButton.Left)
                    {
                        ngui.GetView().MouseButtonDown((int)mouseState.Delta.X, (int)mouseState.Delta.Y, Noesis.MouseButton.Left);

                    }
                    if (e.Button == MouseButton.Right)
                    {
                        ngui.GetView().MouseButtonDown((int)mouseState.Delta.X, (int)mouseState.Delta.Y, Noesis.MouseButton.Right);

                    }
                }
            }
        }

        protected override void OnMouseUp(OpenTK.Windowing.Common.MouseButtonEventArgs e)
        {
            var mouseState = InputManager.GetMouse();

            for (int i = renderLayers.Count() - 1; i >= 0; i--)
            {

                var renderLayer = renderLayers[i];
                if (renderLayer is NoesisUI ngui)
                {
                    if (e.Button == MouseButton.Left)
                    {
                        ngui.GetView().MouseButtonUp((int)mouseState.Delta.X, (int)mouseState.Delta.Y, Noesis.MouseButton.Left);

                    }
                    if (e.Button == MouseButton.Right)
                    {
                        ngui.GetView().MouseButtonUp((int)mouseState.Delta.X, (int)mouseState.Delta.Y, Noesis.MouseButton.Right);

                    }
                }
            }
        }

        public override void Close()
        {
            base.Close();
            Net.StopServer();
            Environment.Exit(0);
        }



        public void AddLayer(IRenderLayer layer)
        {
            renderLayers.Add(layer);
        }
    }
}
