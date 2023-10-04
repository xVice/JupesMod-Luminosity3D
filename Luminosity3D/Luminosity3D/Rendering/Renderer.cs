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
using ImGuiNET;
using OpenTK.Windowing.Common.Input;
using BulletSharp;
using Assimp;
using ErrorCode = OpenTK.Graphics.OpenGL4.ErrorCode;
using Luminosity3D.Rendering;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using Camera = Luminosity3D.Builtin.Camera;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using StbImageSharp;
using TextureWrapMode = OpenTK.Graphics.OpenGL4.TextureWrapMode;
using Face = Assimp.Face;
using Luminosity3D.Builtin;

namespace Luminosity3DRendering
{

    public class AssimpModel : IDisposable
    {
        private Scene scene;
        public List<Meshe> meshes { get; }
        public Meshe FirstMeshe => meshes[0];
        public List<double> PointsForCollision { get; }
        private string PathModel = string.Empty;

        public AssimpModel(string FilePath, bool FlipUVs = false)
        {
            if (!File.Exists(FilePath))
                throw new Exception($"ERROR::ASSIMP:: Arquivo nao encontrado: {FilePath}..");

            PathModel = Path.GetDirectoryName(FilePath)!;

            scene = new Scene();
            meshes = new List<Meshe>();
            PointsForCollision = new List<double>();

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
                    if (item.TextureType == TextureType.Diffuse)
                    {
                        texturesPath._DiffusePath = new string(Path.Combine(PathModel, item.FilePath));
                    }
                    else if (item.TextureType == TextureType.Specular)
                    {
                        texturesPath._SpecularPath = new string(Path.Combine(PathModel, item.FilePath));
                    }
                    else if (item.TextureType == TextureType.Normals)
                    {
                        texturesPath._NormalPath = new string(Path.Combine(PathModel, item.FilePath));
                    }
                    else if (item.TextureType == TextureType.Height)
                    {
                        texturesPath._HeightPath = new string(Path.Combine(PathModel, item.FilePath));
                    }
                    else if (item.TextureType == TextureType.Metalness)
                    {
                        texturesPath._MetallicPath = new string(Path.Combine(PathModel, item.FilePath));
                    }
                    else if (item.TextureType == TextureType.Roughness)
                    {
                        texturesPath._RoughnnesPath = new string(Path.Combine(PathModel, item.FilePath));
                    }
                    else if (item.TextureType == TextureType.Lightmap)
                    {
                        texturesPath._LightMap = new string(Path.Combine(PathModel, item.FilePath));
                    }
                    else if (item.TextureType == TextureType.Emissive)
                    {
                        texturesPath._EmissivePath = new string(Path.Combine(PathModel, item.FilePath));
                    }
                    else if (item.TextureType == TextureType.AmbientOcclusion)
                    {
                        texturesPath._AmbientOcclusionPath = new string(Path.Combine(PathModel, item.FilePath));
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
                throw new Exception($"Não foi possivel encontrar a Textura HDR: {path}");


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
            var activeCam = Engine.SceneManager.ActiveScene.activeCam.GetComponent<Camera>();
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

    public class Model : IDisposable
    {
        public AssimpModel assimpModel;
        public List<Meshe> meshes;
        private ShaderProgram ShaderPBR;
        private Dictionary<string, TextureProgram> TexturesMap = new Dictionary<string, TextureProgram>();

        public Model(string modelPath)
        {
            assimpModel = new AssimpModel(modelPath);
            meshes = new List<Meshe>(assimpModel.meshes);

            ShaderPBR = new ShaderProgram("./shaders/builtin/pbr.vert", "./shaders/builtin/pbr.frag");

            foreach (var index in meshes)
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

        public void RenderFrame(TransformComponent transform)
        {

            var cam = Engine.SceneManager.ActiveScene.activeCam.GetComponent<Camera>();
            ShaderPBR.Use();
            ShaderPBR.SetUniform("model", transform.GetTransformMatrix());
            ShaderPBR.SetUniform("view", cam.ViewMatrix);
            ShaderPBR.SetUniform("projection", cam.ProjectionMatrix);

            ShaderPBR.SetUniform("viewPos", cam.Position);
            ShaderPBR.SetUniform("lightPositions", Vector3.UnitY * 15.0f);
            ShaderPBR.SetUniform("lightColors", Vector4.One);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, UseTexCubemap.Irradiance);
            ShaderPBR.SetUniform("irradianceMap", 1);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.TextureCubeMap, UseTexCubemap.Background);
            ShaderPBR.SetUniform("backgroundMap", 1);

            ShaderPBR.SetUniform("gammaCubemap", 0.5f);
            ShaderPBR.SetUniform("interpolation", 0.9f);

            ShaderPBR.SetUniform("emissiveStrength", 15.0f);

            ShaderPBR.SetUniform("gamma", 1.5f);
            ShaderPBR.SetUniform("luminousStrength", 1.0f);
            ShaderPBR.SetUniform("specularStrength", 1.5f);


            GL.Enable(EnableCap.CullFace);

            foreach (var item in meshes)
            {

                ShaderPBR.SetUniform("AlbedoMap", TexturesMap[item.DiffusePath].Use);
                ShaderPBR.SetUniform("NormalMap", TexturesMap[item.NormalPath].Use);
                ShaderPBR.SetUniform("AmbienteRoughnessMetallic", TexturesMap[item.LightMap].Use);
                ShaderPBR.SetUniform("EmissiveMap", TexturesMap[item.EmissivePath].Use);


                item.RenderFrame();
            }
            GL.Disable(EnableCap.CullFace);



        }
        public void RenderForStencil()
        {
            if (Stencil.RenderStencil)
            {
                foreach (var item in meshes)
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

        private VertexArrayObject Vao;
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

    public class VertexArrayObject : IDisposable
    {
        private int Handle;
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



    public class Renderer : GameWindow
    {
        public DiscreteDynamicsWorld dynamicsWorld;

        public CubeMap cubeMap;
        private ViewPort crossHair;
        private Bloom bloom;

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

            crossHair = new ViewPort("./resources/img/crosshair.png");
            cubeMap = new CubeMap("./resources/Cubemap/industrial_sunset_puresky_4k.hdr", CubeMapType.Type1);
            bloom = new Bloom();

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
            if(Engine.SceneManager.ActiveScene.activeCam != null)
            {
                //bloom.BindBloom();
                cubeMap.RenderFrame();
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
                //bloom.RenderFrame();
                crossHair.RenderFrame(Vector2.Zero, 0.03f);
               
            }
            else
            {
                ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

                if (renderLayers.Count != 0)
                {
                    for (int i = renderLayers.Count() - 1; i >= 0; i--)
                    {
                        var renderLayer = renderLayers[i];
                        renderLayer.Render();
                    }
                }
            }




            IMGUIController.Render();
            
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
    }
}
