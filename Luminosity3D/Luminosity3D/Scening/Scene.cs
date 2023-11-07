using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Luminosity3D;
using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Utils;
using Luminosity3D.Builtin;
using System.Reflection;
using Salar.Bois;
using System.Runtime.Serialization.Formatters;

namespace Luminosity3DScening
{
    public class Scene
    {
        public string Name { get; set; } = "New Scene";
        public List<GameObject> Entities = new List<GameObject>();

        public ComponentCache cache = new Luminosity3D.EntityComponentSystem.ComponentCache();

        [JsonIgnore]
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

        public string ToJson(Formatting format = Formatting.None)
        {
            foreach (var ent in Entities.Where(x => x.NetCode == string.Empty))
            {
                ent.NetCode = Guid.NewGuid().ToString();
            }

            return JsonConvert.SerializeObject(this, format, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });

        }

        public static Scene FromJson(string data)
        {
            return JsonConvert.DeserializeObject<Scene>(data, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto});
        }

        //Should take a scene to keep a smaller ram budget, also lower net freq which might be good for not dropping packets
        public void NetMerge()
        {
            if (Net.objectQue.Count > 0)
            {
                for (int i = 0; i < Net.objectQue.Count; i++)
                {
                    var netEnt = Net.objectQue.Dequeue();

                    try
                    {
                        var targetEntInActiveScene = Entities.FirstOrDefault(x => x.NetCode == netEnt.NetCode);
                        if (targetEntInActiveScene != null)
                        {  
                            foreach(var comp in targetEntInActiveScene.components.Values)
                            {
                                
                                if(comp is Networkable netComp)
                                {
                                    netComp.Net(netEnt);
                                }
                            }

                            /*
                            foreach (var netComp in netEnt.components.Values)
                            {
                                var targetComp = targetEntInActiveScene.components[netComp.GetType()];

                                if(targetComp != null)
                                {
                                    var netProperties = targetComp.GetType()
                                        .GetProperties();

                                    foreach (var netProperty in netProperties)
                                    {
                                        if(netProperty.IsDefined(typeof(NetAttribute), false))
                                        {
                                            Logger.Log($"NET: {netProperty.Name}");
                                            var targetProp = targetComp.GetType().GetProperty(netProperty.Name);
                                            if (targetProp != null)
                                            {
                                                try
                                                {
                                                    var value = netProperty.GetValue(netComp);

                                                    if (targetProp.CanWrite)
                                                    {
                                                        targetProp.SetValue(targetEntInActiveScene, value);
                                                        Logger.Log("NET: Synced an entity!", true);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Logger.Log($"NET: Error syncing entity: {ex.Message}", true);
                                                }
                                            }
                                        }   
                      
                                           
                                        
                                       
                                    }
                                }
                            }
                            */
                        }
                        else
                        {
                            var newGo = InstantiateEntity(netEnt);
                            foreach(var comp in newGo.components.Values)
                            {
                                if(comp is Networkable netComp)
                                {
                                    netComp.Net(netEnt);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"NET: Fatal error when trying to sync a game object: {ex.ToString()}", true, LogType.Error);
                    }
                }
            }
        }

        public void SerializeToFile(string filePath)
        {
            if(!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            foreach(var entity in Entities)
            {
                GameObjectSerializer.SerializeToPath(entity, filePath);
                //Temp.Write($"{tempFolder}/ents/{entity.GetHashCode()}.json", entity.ToSerializedEntity().Serialize());
            }

        }

        public GameObject InstantiateEntity(GameObject entity, bool awake = true)
        {
            
            foreach(var comp in entity.components.Values)
            {
                comp.GameObject = entity;
                if(cache.HasCache(comp) == false)
                {
                    cache.CacheComponent(comp);
                }
            }
            if (awake)
            {
                entity.Awake();

            }
            Entities.Add(entity);
            return entity;
        }

        public void Load()
        {
            SceneManager.ActiveScene = this;
        }
    }
}
