using Assimp;
using ImGuiNET;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Rendering;
using Luminosity3D.Utils;
using Luminosity3DRendering;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using static Assimp.Metadata;
using Material = Assimp.Material;
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

    public static class ShaderCache
    {
        public static Dictionary<Material, Shader> Cache = new Dictionary<Material, Shader>();


        public static void CacheShader(Material mesh, Shader shader)
        {
            Cache.Add(mesh, shader);
        }

        public static bool HasShaderForMat(Material mat)
        {
            return Cache.ContainsKey(mat);
        }

        public static Shader Get(Material mat)
        {
            if (Cache.ContainsKey(mat))
            {
                return Cache[mat];
            }
            var shader = Shader.BuildFromMaterialPBR(mat);
            Cache.Add(mat, shader);
            return shader;
        }
    }

   


 

    [RequireComponent(typeof(TransformComponent))]
    public class MeshBatch : LuminosityBehaviour, IImguiSerialize
    {
        private TransformComponent transform = null;

        public string filePath = "./teapot.obj";
        public AssimpModel model = null;
        public Shader shader = null;

        public static MeshBatch FromPath(string path)
        {
            var batch = new MeshBatch();
            batch.filePath = path;
            return batch;
        }

        public override void Awake()
        {
            transform = GetComponent<TransformComponent>();
            model = AssimpCache.Get(filePath);
        }

        public void EditorUI()
        {
            ImGui.Text(filePath);
        }

        public static Component OnEditorCreation()
        {
            return new MeshBatch();
        }
    }   
}


