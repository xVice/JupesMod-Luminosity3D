using ImGuiNET;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using Luminosity3DRendering;

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

    [RequireComponent(typeof(TransformComponent))]
    public class MeshBatch : LuminosityBehaviour, IImguiSerialize
    {
        public Model model = null;

        public string filePath = "./teapot.obj";


        public static MeshBatch FromPath(string path)
        {
            var batch = new MeshBatch();
            batch.filePath = path;
            return batch;
        }

        public override void Awake()
        {
            
            model = new Model(filePath);

        }

        public void EditorUI()
        {
            ImGui.Text(filePath);
        }

        public static LuminosityBehaviour OnEditorCreation()
        {
            return new MeshBatch();
        }
    }   
}


