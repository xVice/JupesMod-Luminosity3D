using Luminosity3D.Builtin;
using Luminosity3D.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.EntityComponentSystem
{
    public class GameObjectSerializer
    {
        public static void SerializeToFile(GameObject gameObject, string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            Directory.CreateDirectory(path);
            string rootGOPath = Path.Combine(path, gameObject.GetHashCode().ToString());

            Directory.CreateDirectory(rootGOPath);
            Directory.CreateDirectory(Path.Combine(rootGOPath, "components"));

            RecursiveBuild(rootGOPath, gameObject);

            // Serialize the GameObject to JSON
            string gameObjectJson = JsonConvert.SerializeObject(gameObject);
            File.WriteAllText(Path.Combine(rootGOPath, "gameObject.json"), gameObjectJson);
        }

        public static GameObject DeserializeFromFile(string path)
        {
            // Load the serialized GameObject from JSON
            string gameObjectJson = File.ReadAllText(Path.Combine(path, "gameObject.json"));
            return JsonConvert.DeserializeObject<GameObject>(gameObjectJson);
        }

        private static void RecursiveBuild(string rootPath, GameObject go)
        {
            string currentPath = Path.Combine(rootPath, go.GetHashCode().ToString());
            Directory.CreateDirectory(currentPath);
            Directory.CreateDirectory(Path.Combine(currentPath, "components"));

            // Serialize components here if needed
            foreach (var kvp in go.components)
            {
                // Serialize the component to JSON and save it in the components directory
                string componentJson = JsonConvert.SerializeObject(kvp.Value);
                File.WriteAllText(Path.Combine(currentPath, "components", kvp.Key.Name + ".json"), componentJson);
            }

            // Recursively serialize children
            foreach (var child in go.Childs)
            {
                RecursiveBuild(currentPath, child);
            }
        }
    }

    

    public class GameObject
    {
        public string Name { get; set; } = "New GameObject";
        public string Tag { get; set; } = string.Empty;
        public bool ActiveAndEnabled { get; set; } = true;
        
        public GameObject Parent = null;
        public List<GameObject> Childs = new List<GameObject>();
        public Dictionary<Type, LuminosityBehaviour> components = new Dictionary<Type, LuminosityBehaviour>();
        public int ExecutionOrder = 0;

        public GameObject()
        {
            Engine.SceneManager.ActiveScene.InstantiateEntity(this);
        }
        public GameObject(string name)
        {
            Name = name;
            Engine.SceneManager.ActiveScene.InstantiateEntity(this);
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

        public T AddComponent<T>(T comp) where T : LuminosityBehaviour, new()
        {
            CheckRequiredComponents<T>();


            Type type = typeof(T);
            if (!components.ContainsKey(type))
            {
                comp.Parent = this;
                components[type] = comp;
                comp.Awake();
                Engine.SceneManager.ActiveScene.cache.CacheComponent(comp);
                return comp;
            }
            return null;
        }

        public T AddComponent<T>() where T : LuminosityBehaviour, new()
        {
            CheckRequiredComponents<T>();

            Type type = typeof(T);
            if (!components.ContainsKey(type))
            {
                T component = new T();
                component.Parent = this;


                components[type] = component;
                component.Awake();

                Engine.SceneManager.ActiveScene.cache.CacheComponent(component);
                return component;
            }
            return null;
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
                            // Use reflection to create an instance of the required component type
                            var component = Activator.CreateInstance(requiredType) as LuminosityBehaviour;
                            component.Parent = this;
                            components[requiredType] = component;
                            component.Awake();

                            Engine.SceneManager.ActiveScene.cache.CacheComponent(component);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Error adding {requiredType.Name} component: {ex.Message}");
                        }
                    }
                }



            }
        }


    }

}

