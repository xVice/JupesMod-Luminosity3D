using Luminosity3D.Builtin;
using Luminosity3D.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.EntityComponentSystem
{
    public class GameObject
    {
        public string Name { get; set; } = "New GameObject";
        public string Tag { get; set; } = string.Empty;
        public bool ActiveAndEnabled { get; set; } = true;
        public GameObject Parent = null;
        public List<GameObject> Childs = new List<GameObject>();
        public Dictionary<Type, Component> components = new Dictionary<Type, Component>();
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
        public T GetComponent<T>() where T : Component
        {
            Type type = typeof(T);
            if (components.ContainsKey(type))
            {
                return components[type] as T;
            }

            return null; // Return null if the component is not found.
        }

        public List<T> GetComponents<T>() where T : Component
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


        public bool HasComponent<T>() where T : Component
        {
            Type type = typeof(T);
            return components.ContainsKey(type);
        }

        public T AddComponent<T>(T comp) where T : Component, new()
        {
            //CheckRequiredComponents<T>();


            Type type = typeof(T);
            if (!components.ContainsKey(type))
            {
                comp.Parent = this;
                components[type] = comp;
                if (comp is LuminosityBehaviour behav)
                {
                    behav.Awake();
                }
                Engine.SceneManager.ActiveScene.cache.CacheComponent(comp);
                return comp;
            }
            return null;
        }

        public T AddComponent<T>() where T : Component, new()
        {
            //CheckRequiredComponents<T>();

            Type type = typeof(T);
            if (!components.ContainsKey(type))
            {
                T component = new T();
                component.Parent = this;


                components[type] = component;

                if (component is LuminosityBehaviour behav)
                {
                    behav.Awake();
                }

                Engine.SceneManager.ActiveScene.cache.CacheComponent(component);
                return component;
            }
            return null;
        }

        public bool HasComponent(Type type)
        {
            return components.ContainsKey(type);
        }



        private void CheckRequiredComponents<T>() where T : Component
        {
            Type typeToAdd = typeof(LuminosityBehaviour); // Use the parent class type

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
                            var component = Activator.CreateInstance(requiredType) as Component;
                            component.Parent = this;
                            components[requiredType] = component;

                            if (component is LuminosityBehaviour behav)
                            {
                                behav.Awake();
                            }

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

