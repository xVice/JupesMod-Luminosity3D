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
        public List<Entity> Entities = new List<Entity>();

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
                Temp.Write($"{tempFolder}/ents/{entity.GetHashCode()}.json", entity.ToSerializedEntity().Serialize());
            }

        }

        public List<Entity> FindEntitysWithObjectsOfType<T>() where T : Component
        {
            List<Entity> result = new List<Entity>();

            foreach (Entity entity in Entities)
            {
                // Check if the entity has a component of type T attached
                if (entity.GetComponent<T>() != null)
                {
                    result.Add(entity);
                }
            }

            return result;
        }

        public List<T> FindObjectsOfType<T>() where T : Component
        {
            List<T> result = new List<T>();

            foreach (Entity entity in Entities)
            {
                // Check if the entity has a component of type T attached
                if (entity.GetComponent<T>() != null)
                {
                    result.AddRange(entity.GetComponents<T>());
                }
            }

            return result;
        }


        public Entity InstantiateEntity(Entity entity)
        {
            Entities.Add(entity);

            entity.Start();
            
            return entity;
        }

        public void Load()
        {
            Engine.SceneManager.ActiveScene = this;
        }

        public Entity FindEntity(string name)
        {
            return Entities.FirstOrDefault(x => x.Name == name);
        }

        public void Update()
        {
            foreach (var entity in Entities)
            {
                entity.Update();
            }
        }
    }
}
