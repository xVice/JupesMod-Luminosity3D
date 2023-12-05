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
using Ceras;

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

        public void ToByte(ref byte[] seribuffer)
        {
            foreach (var ent in Entities.Where(x => x.NetCode == string.Empty))
            {
                ent.NetCode = Guid.NewGuid().ToString();
            }


            // Serialize the Scene object and store it in the seribuffer array
            Engine.GetSerializer().Serialize<Scene>(this, ref seribuffer);
        }

        public static Scene FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Scene>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public string ToJson(Formatting format = Formatting.Indented)
        {
            return JsonConvert.SerializeObject(this, format, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public static void FromByte(ref Scene scene, byte[] data)
        {
            Engine.GetSerializer().Deserialize<Scene>(ref scene, data);
        }

        //Should take a scene to keep a smaller ram budget, also lower net freq which might be good for not dropping packets
        public void NetMerge()
        {
            if(Net.LocalClient != null)
            {
                var que = BehavNet.GetSceneQue();
                var count = que.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        var scene = que.Dequeue();

                        foreach (var ent in scene.Entities)
                        {
                            try
                            {
                                var localEnt = Entities.FirstOrDefault(x => x.NetCode == ent.NetCode);
                                if (localEnt != null)
                                {
                                    foreach (var netComp in localEnt.components.Values)
                                    {
                                        if (netComp is Networkable networkableComp)
                                        {
                                            networkableComp.Net(ent);
                                        }
                                    }
                                }
                                else
                                {
                                    InstantiateEntity(ent);
                                    Logger.Log("NET: Instantiated a new entity in the scene!", true, LogType.Information);
                                }



                            }
                            catch (Exception ex)
                            {
                                Logger.Log($"NET: Fatal error when trying to sync a game object: {ex.ToString()}", true, LogType.Error);
                            }
                        }
                        //Net.ResetPacketDrop();
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
