using Luminosity3D.Builtin;
using Luminosity3D.PKGLoader;
using Luminosity3D.Utils;
using Luminosity3DScening;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Reflection;

namespace Luminosity3D.EntityComponentSystem
{
    public class DataField
    {
        public Dictionary<string, object> values = new Dictionary<string, object>();

        public T Get<T>(string name)
        {
            return (T)values[name];
        }

        public void Set(string name, object obj)
        {
            values[name] = obj;
        }
    }

    public static class GameObjectSerializer
    {

        public static void SerializeToPath(GameObject obj, string path)
        {
            SerializeObj(obj, path);
        }

        public static string SerializeToString(GameObject obj, Formatting format = Formatting.None)
        {
            return JsonConvert.SerializeObject(obj, format, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }

        private static void SerializeObj(GameObject obj, string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllText(Path.Combine(path, $"{obj.Name}-{obj.GetHashCode()}.json"), JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings{TypeNameHandling = TypeNameHandling.Auto}));
        }



        public static GameObject DeserializeFromPath(string path)
        {
            if (File.Exists(path))
            {
                GameObject rootGameObject = JsonConvert.DeserializeObject<GameObject>(File.ReadAllText(path), new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });


                return rootGameObject;

            }
            return null;
        }

        public static GameObject DeserializeFromString(string data)
        {
            GameObject rootGameObject = JsonConvert.DeserializeObject<GameObject>(data, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });


            return rootGameObject;
        }

    }

    


    public class LuminosityProject
    {
        public string ProjectName = string.Empty;
        public string ProjectVersion = string.Empty;
        public string ProjectAuthor = string.Empty;
        public string ProjectDescription = string.Empty;

        private string ProjectPath = string.Empty;
        private string ScenesPath = string.Empty;
        private string ModsPath = string.Empty;

        public LuminosityProject(string projectName, string projectVersion, string projectAuthor, string projectDescription)
        {
            ProjectName = projectName;
            ProjectVersion = projectVersion;
            ProjectAuthor = projectAuthor;
            ProjectDescription = projectDescription;
            
            ProjectPath = Path.Combine(Engine.Directorys.ProjectsPath, ProjectName);
            ScenesPath = Path.Combine(ProjectPath, "scenes");
            ModsPath = Path.Combine(ProjectPath, "mods");
        }
        //They are literally the same thing, this is a common theme through the engine to make it easier/harder because unity basically is these two things + some more stupid af shit(static func that returns the constructor)
        public static LuminosityProject CreateProject(string ProjectName = "New Project", string ProjectVersion = "0.0.1", string ProjectAuthor = "Gabe Newell", string ProjectDescription = "Half Life 3")
        {
            return new LuminosityProject(ProjectName, ProjectVersion, ProjectAuthor, ProjectDescription);
        }
        public static LuminosityProject Load(string name)
        {
            if (!Directory.Exists(Path.Combine(Engine.Directorys.ProjectsPath, name)))
            {
                return null;
            }

            var proj = JsonConvert.DeserializeObject<LuminosityProject>(Path.Combine(Engine.Directorys.ProjectsPath, name, "project.json"));
            foreach (var mod in Directory.GetFiles(proj.ModsPath, "*.lupk"))
            {
                //TODO: add PackageLoader.LoadPackageFromPath() to load mods/logic/gameplay whatever from a projects mods(i am going crazy)
            }

            return proj;
        }

        public void Save()
        {
            if(Directory.Exists(ProjectPath))
            {
                Directory.Delete(ProjectPath, true);

            }

            Directory.CreateDirectory(ProjectPath);
            Directory.CreateDirectory(ScenesPath);

            File.WriteAllText(Path.Combine(ProjectPath, "project.json"), JsonConvert.SerializeObject(this));

            foreach(var scene in SceneManager.Scenes)
            {
                File.WriteAllText(Path.Combine(ScenesPath, $"{scene.Name}.json"), scene.ToJson());
            }


        }


        public Scene[] GetScene()
        {
            List<Scene> sceneList = new List<Scene>();
            foreach(var sceneJson in Directory.GetFiles(ScenesPath, "*.json"))
            {
                sceneList.Add(Scene.FromJson(sceneJson));
            }
            return sceneList.ToArray();
        }


    }

    public class ScriptableObjectCache
    {
        public ScriptableObject[] cache = new ScriptableObject[0];

    }

    public class ScriptableObject
    {
        
    }


    public class GameObject
    {
        public string Name { get; set; } = "New GameObject";
        public string Tag { get; set; } = string.Empty;

        public string NetCode = string.Empty;
        public bool ActiveAndEnabled { get; set; } = true;
        public int ExecutionOrder = 0;

        public DataField dataField = new DataField();


        //[JsonIgnore]
        public GameObject Parent = null;

        [JsonIgnore]
        public Scene Scene { get; set; }

        //[JsonIgnore]
        public List<GameObject> Childs = new List<GameObject>();

        //[JsonIgnore]
        public Dictionary<Type, LuminosityBehaviour> components = new Dictionary<Type, LuminosityBehaviour>();

        [JsonIgnore]
        public TransformComponent Transform
        {
            get
            {
                if (HasComponent<TransformComponent>())
                {
                    return GetComponent<TransformComponent>();
                }
                else
                {
                    return AddComponent<TransformComponent>();
                }
            }
        }



        public string GetSerializedString()
        {
            return GameObjectSerializer.SerializeToString(this);
        }

        public GameObject()
        {
            Childs = new List<GameObject>();
            components = new Dictionary<Type, LuminosityBehaviour>();
        }

        public void FixNetGo()
        {
            foreach (var comp in components.Values)
            {
                comp.GameObject = this;
                

            }

        }

        public void Awake()
        {


            foreach (var comp in components.Values)
            {
                comp.Awake();

            }
        }

        public GameObject(bool createInScene = true)
        {
            if (createInScene)
            {
                SceneManager.ActiveScene.InstantiateEntity(this);

            }
        }

        
        public GameObject(string name)
        {
            Name = name;
            SceneManager.ActiveScene.InstantiateEntity(this);
        }

        public void SavePrefab(string name)
        {
            var path = Resources.ResourcesPath + "/prefabs/" + SceneManager.ActiveScene.Name + "/";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllText(path + $"/{name}-prefab.json", GameObjectSerializer.SerializeToString(this));
        }

        public static GameObject FromPrefab(string path, string name)
        {
            var finalpath = Resources.ResourcesPath + "/prefabs/" + SceneManager.ActiveScene.Name + "/";

            if (!Directory.Exists(finalpath))
            {
                return null;
            }

            return GameObjectSerializer.DeserializeFromString(File.ReadAllText(finalpath + $"/{name}-prefab.json"));
        }


        public bool CompareTag(string tag)
        {
            return Tag.Equals(tag);
        }
        public T GetComponent<T>() where T : LuminosityBehaviour
        {
            Type type = typeof(T);
            if (components.ContainsKey(type))
            {
                return components[type] as T;
            }

            return null; // Return null if the component is not found.
        }

        public void Kill()
        {
            foreach(var comp in components.Values)
            {
                comp.Remove();
            }
            SceneManager.ActiveScene.Entities.Remove(this);
        }

        public List<T> GetComponents<T>() where T : LuminosityBehaviour
        {
            Type type = typeof(T);
            List<T> result = new List<T>();
            foreach (var component in components.Values)
            {
                if (type.IsAssignableFrom(component.GetType()))
                {
                    result.Add(component as T);
                }
            }
            return result;
        }


        public bool HasComponent<T>() where T : LuminosityBehaviour
        {
            Type type = typeof(T);
            return components.ContainsKey(type);
        }


       




        public T AddComponent<T>(T comp) where T : LuminosityBehaviour
        {
            CheckRequiredComponents<T>();

            Type type = typeof(T);
            if (!components.ContainsKey(type))
            {
                
                components[type] = comp;
                comp.GameObject = this;
                comp.Awake();
                SceneManager.ActiveScene.cache.CacheComponent(comp);
                return comp; // Return the newly added component.
            }
            else
            {
                // Component of type T already exists, so return the existing component.
                return components[type] as T;
            }
        }


        public void RemoveComponent<T>() where T : LuminosityBehaviour
        {
            Type type = typeof(T);
            if (components[type] != null)
            {
                components[type] = null;
            }
        }

        public void RemoveComponent<T>(T comp) where T : LuminosityBehaviour
        {
            Type type = comp.GetType();
            if (components[type] != null)
            {
                components[type] = null;
            }
        }


        public T AddComponent<T>() where T : LuminosityBehaviour, new()
        {
            CheckRequiredComponents<T>();

            Type type = typeof(T);
            if (!components.ContainsKey(type))
            {
                T component = new T();
                component.GameObject = this;


                components[type] = component;
                component.Awake();

                SceneManager.ActiveScene.cache.CacheComponent(component);
                return component;
            }
            return components[type] as T;
        }

        public bool HasComponent(Type type)
        {
            return components.ContainsKey(type);
        }



        private void CheckRequiredComponents<T>() where T : LuminosityBehaviour
        {
            var typeToAdd = typeof(T);
            var requiredAttributes = typeToAdd.GetCustomAttributes(typeof(RequireComponentAttribute), true);

            foreach (var attribute in requiredAttributes)
            {
                if (attribute is RequireComponentAttribute requireComponentAttribute)
                {
                    var requiredType = requireComponentAttribute.RequiredComponentType;
                    if (!components.ContainsKey(requiredType))
                    {
                        try
                        {
                            if (!HasComponent(requiredType))
                            {
                                // Use reflection to create an instance of the required component type
                                var component = Activator.CreateInstance(requiredType) as LuminosityBehaviour;
                                component.GameObject = this;
                                components[requiredType] = component;
                                component.Awake();

                                SceneManager.ActiveScene.cache.CacheComponent(component);
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Error adding {requiredType.Name} component: {ex.Message}",true ,LogType.Error);
                        }
                    }
                }
            }
        }
    }
}

