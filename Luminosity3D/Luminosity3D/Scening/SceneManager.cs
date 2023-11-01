using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luminosity3D;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;

namespace Luminosity3DScening
{
    public class SceneManager
    {
        public static List<Scene> Scenes = new List<Scene>();
        public static Scene ActiveScene = new Scene();

        public static Scene Next()
        {
            var nextScene = new Scene();
            var nextIndex = Scenes.IndexOf(ActiveScene) + 1;
            if (nextIndex >= Scenes.Count())
            {
                nextScene = Scenes[nextIndex];
            }
            return nextScene;
        }

        public static List<Scene> GetScenes()
        {
            return Scenes;
        }

        public static Scene ActivateNext()
        {
            var nextIndex = Scenes.IndexOf(ActiveScene) + 1;
            if (nextIndex >= Scenes.Count())
            {
                ActiveScene = Scenes[nextIndex];
            }
            return ActiveScene;
        }

        public static Scene AddScene(Scene scene)
        {
            Scenes.Add(scene);
            return scene;
        }

        public static Scene GetScene(string name)
        {
            return (Scene)Scenes.Where(x => x.Name == name);
        }

        public static void LoadScene(string sceneName, bool setActive = true)
        {
            var scene = new Scene(sceneName);
            var scenePath = $"./scenes/{scene.Name}";

            if (!Directory.Exists(scenePath))
            {
                Logger.Log("No scene found..");
                return;
            }


            if (setActive)
            {
                scene.Load();
            }


            foreach (var folder in Directory.GetFiles(scenePath, "*.json"))
            {
                Engine.Renderer.Title = $"{JModVersionInfo.TitleString} - Loading: ({Path.GetFileName(folder)})..";
                var go = GameObjectSerializer.DeserializeFromPath(folder);
                scene.InstantiateEntity(go);
            }
            Engine.Renderer.Title = JModVersionInfo.TitleString;



        }
    }
}
