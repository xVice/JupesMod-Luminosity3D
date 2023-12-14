using Ceras.Formatters;
using ImGuiNET;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using Luminosity3DRendering;
using MyGame;
using Newtonsoft.Json;
using Noesis;

namespace Luminosity3D.Builtin
{




    //This is insanity, straight insanity, thats why i am rewriting it now.
    //
    //right now its looking way better, i didnt do any shading though c:
    //
    //its shit, need to rewrite :c
    //
    //kinda better still 50% shit, only 35% nowit j
    //hoiy fuckintoncity it workie now  tho c: c: c: c: c: c:  C:.C . C:.C .C 
    //now makie shadercache for cahce shader becuase big program many times big slow and bad :c
    //good?
    //
    // fuck you its not good, how does one learn opengl??????????????????????????????????????????????????????????????????????????????????????????????????????????????
    //fucking learning chinese is more straightforward smh

    [AddComponentMenu("Rendering/3D/Mesh Batch")]
    [RequireComponent(typeof(TransformComponent))]
    public class MeshBatch : LuminosityBehaviour, IImguiSerialize
    {
        private Model model = null;

        public string filePath = string.Empty;


        public Model GetModel() { return model; }

        public static MeshBatch FromPath(string path)
        {
            var batch = new MeshBatch();
            batch.filePath = path;
            return batch;
        }

        public override void Awake()
        {
            if(model == null)
            {
                model = new Model(ResourcesManager.GetResource("game").Get<AssimpModel>(filePath));
                model.SetGameObject(GameObject);
            }

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


