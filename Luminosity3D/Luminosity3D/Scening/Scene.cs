using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luminosity3D;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using Luminosity3D.Builtin;

namespace Luminosity3DScening
{
    public class Scene
    {
        public string Name { get; set; } = "New Scene";
        public List<GameObject> Entities = new List<GameObject>();

        public ComponentCache cache = new Luminosity3D.EntityComponentSystem.ComponentCache();

        public Camera activeCam = null;

        public Scene()
        {
            Name = string.Empty;
        }

        public Scene(string name)
        {
            Name = name;
        }

        public Scene LoadSceneFromFile(string filePath)
        {
            if(File.Exists(filePath))
            {
                var scene = new Scene();

                //unzip and read file data in reverse like below

                return scene;
            }
            return null;
        }

        public void SerializeToFile(string filePath)
        {
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            var tempFolder = $"temp-scene-{Name}";

            Temp.CreateFolder(tempFolder);
            Temp.CreateFolder(tempFolder + "/ents/");

            Temp.Write(tempFolder + "/metadata.json", Name);

            foreach(var entity in Entities)
            {
                //Temp.Write($"{tempFolder}/ents/{entity.GetHashCode()}.json", entity.ToSerializedEntity().Serialize());
            }

        }

        public GameObject InstantiateEntity(GameObject entity, bool awake = true)
        {
            Entities.Add(entity);
            entity.FixNetGo();
            if (awake)
            {
                entity.Awake();

            }
            return entity;
        }

        public void Load()
        {
            SceneManager.ActiveScene = this;
        }
    }
}
