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
        public Pool<Scene> Scenes = new Pool<Scene>();
        public Scene ActiveScene = new Scene();

        public Scene Next()
        {
            var nextScene = new Scene();
            var nextIndex = Scenes.GetContent().IndexOf(ActiveScene) + 1;
            if (nextIndex >= Scenes.GetContent().Count())
            {
                nextScene = Scenes.GetContent()[nextIndex];
            }
            return nextScene;
        }

        public List<Scene> GetScenes()
        {
            return Scenes.GetContent();
        }

        public Scene ActivateNext()
        {
            var nextIndex = Scenes.GetContent().IndexOf(ActiveScene) + 1;
            if (nextIndex >= Scenes.GetContent().Count())
            {
                ActiveScene = Scenes.GetContent()[nextIndex];
            }
            return ActiveScene;
        }

        public Scene AddScene(Scene scene)
        {
            Scenes.Enqueue(scene);
            return scene;
        }

        public Scene GetScene(string name)
        {
            return (Scene)Scenes.GetContent().Where(x => x.Name == name);
        }
    }
}
