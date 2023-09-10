using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luminosity3D;
using Luminosity3D.Utils;

namespace Luminosity3DScening
{
    public class SceneManager
    {
        public List<Scene> Scenes = new List<Scene>();
        public Scene ActiveScene = new Scene();

        public Scene Next()
        {
            var nextScene = new Scene();
            var nextIndex = Scenes.IndexOf(ActiveScene) + 1;
            if (nextIndex >= Scenes.Count())
            {
                nextScene = Scenes[nextIndex];
            }
            return nextScene;
        }

        public List<Scene> GetScenes()
        {
            return Scenes;
        }

        public Scene ActivateNext()
        {
            var nextIndex = Scenes.IndexOf(ActiveScene) + 1;
            if (nextIndex >= Scenes.Count())
            {
                ActiveScene = Scenes[nextIndex];
            }
            return ActiveScene;
        }

        public Scene AddScene(Scene scene)
        {
            Scenes.Add(scene);
            return scene;
        }

        public Scene GetScene(string name)
        {
            return (Scene)Scenes.Where(x => x.Name == name);
        }
    }
}
