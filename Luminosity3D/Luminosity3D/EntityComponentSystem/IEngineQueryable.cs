using ImGuiNET;
using Luminosity3D.Utils;
using Luminosity3DScening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.EntityComponentSystem
{

    public interface IImguiSerialize
    {
        abstract static LuminosityBehaviour OnEditorCreation();
        void EditorUI();
    }

    public interface IImguiSerializeSettings
    {
        string SettingName { get; set; }
        string SettingArea { get; set; }

        void SettingUi();
    }

    public static class Settings
    {
        private static List<IImguiSerializeSettings> SettingsList = new List<IImguiSerializeSettings>();

        public static void RegisterSetting(IImguiSerializeSettings setting)
        {
            SettingsList.Add(setting);
        }



        public static List<IImguiSerializeSettings> GetSettings()
        {
            return SettingsList;
        }
    }

    public class TestSetting : IImguiSerializeSettings
    {
        public string SettingName { get => "Test Setting"; set => SettingName = value; }
        public string SettingArea { get => "Test Setting Area"; set => SettingArea = value; }

        public void SettingUi()
        {
            ImGui.Text("Poggers");
            if(ImGui.Button("Spawn Pog"))
            {
                var activeScene = SceneManager.ActiveScene;
                
                EntitySummoner.CreatePBREntityWithRb("Test setting ent", "./fish.obj", activeScene.activeCam.Position);
            }
        }
    }

    public class TestSetting2 : IImguiSerializeSettings
    {
        public string SettingName { get => "Test Setting2"; set => SettingName = value; }
        public string SettingArea { get => "Test Setting Area"; set => SettingArea = value; }

        public void SettingUi()
        {
            ImGui.Text("Poggers2");
            if (ImGui.Button("Spawn Pog2"))
            {
                var activeScene = SceneManager.ActiveScene;

                EntitySummoner.CreatePBREntityWithRb("Test setting ent", "./fish.obj", activeScene.activeCam.Position);
            }
        }
    }

    public class TimeSetting : IImguiSerializeSettings
    {
        public string SettingName { get => "Engine Time"; set => SettingName = value; }
        public string SettingArea { get => "Time"; set => SettingArea = value; }

        public void SettingUi()
        {
            ImGui.Separator();
            ImGui.Text("Last RenderPass: " + SceneManager.ActiveScene.cache.lastRenderTime.ToString());
            ImGui.Text("Last Physics Update: " + SceneManager.ActiveScene.cache.lastPhysicsTime.ToString());
            ImGui.Text("Last General Update: " + SceneManager.ActiveScene.cache.lastUpdateTime.ToString());
            ImGui.Separator();
            ImGui.Text("Delta Time: " + Time.deltaTime.ToString());

            ImGui.Text("Time: " + Time.time.ToString());
           
            ImGui.Text("Time Scale: " + Time.timeScale.ToString());

            var newts = Time.timeScale;
            ImGui.InputFloat("##TimeScale", ref newts);
            Time.timeScale = newts;
            ImGui.Separator();

        }
    }

}
