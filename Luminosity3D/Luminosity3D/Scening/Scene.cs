using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luminosity3D;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;

namespace Luminosity3DScening
{
    public class Scene
    {
        public string Name { get; set; } = "New Scene";
        public Pool<Entity> Entities = new Pool<Entity>();

        public Scene()
        {
            Name = string.Empty;
        }

        public Scene(string name)
        {
            Name = name;
        }

        public string SerializeToJson()
        {
  

            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public List<Entity> FindObjectsOfType<T>() where T : Component
        {
            List<Entity> result = new List<Entity>();

            foreach (Entity entity in Entities.GetContent())
            {
                // Check if the entity has a component of type T attached
                if (entity.GetComponent<T>() != null)
                {
                    result.Add(entity);
                }
            }

            return result;
        }

        public void SaveToDisk()
        {
            File.WriteAllText($"./scenes/{Name}", SerializeToJson());
        }

        public Entity InstantiateEntity(Entity entity)
        {
            Entities.Enqueue(entity);
            entity.Awake();
            return entity;
        }

        public void Load()
        {
            Engine.Instance.SceneManager.ActiveScene = this;
        }

        public Entity FindEntity(string name)
        {
            return Entities.GetContent().FirstOrDefault(x => x.Name == name);
        }

        public void Update()
        {
            foreach (var entity in Entities.GetContent())
            {
                entity.Update();
            }
        }
    }
}
