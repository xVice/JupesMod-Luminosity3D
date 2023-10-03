﻿using ImGuiNET;
using Luminosity3D.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.EntityComponentSystem
{

    public interface IImguiSerialize
    {
        abstract static Component OnEditorCreation();
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
                var activeScene = Engine.SceneManager.ActiveScene;
                
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
                var activeScene = Engine.SceneManager.ActiveScene;

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
            ImGui.Text("Last RenderPass: " + Engine.SceneManager.ActiveScene.cache.lastRenderTime.ToString());
            ImGui.Text("Last Physics Update: " + Engine.SceneManager.ActiveScene.cache.lastPhysicsTime.ToString());
            ImGui.Text("Last General Update: " + Engine.SceneManager.ActiveScene.cache.lastUpdateTime.ToString());
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
