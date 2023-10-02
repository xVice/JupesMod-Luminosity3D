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
    public class SerializedEntity
    {
        public string Name { get; set; }
        public SerializedEntity Parent { get; set; }
        [JsonIgnore]
        public List<Component> Components { get; set; }

        public string Serialize()
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.SerializeObject(this, settings);
        }

        public static SerializedEntity Deserialize(string json)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            return JsonConvert.DeserializeObject<SerializedEntity>(json, settings);
        }
    }



    public class Entity
    {
        public string Name { get; set; }    

        [JsonIgnore]
        public Entity Parent = null;
        [JsonIgnore]
        public List<Component> Components { get; set; } = new List<Component>();
        public int ExecutionOrder = int.MaxValue;

        public Entity()
        {

        }

        public Entity(string name)
        {
            Name = name;
         
        }

        public Entity(string name, Entity parent)
        {
            Name = name;
            Parent = parent;

        }

        public Entity(string name, int executionOrder)
        {
            Name = name;
            ExecutionOrder = executionOrder;

        }

        public Entity(string name, Entity parent, int executionOrder)
        {
            Name = name;
            ExecutionOrder = executionOrder;

        }

        public List<T> GetComponents<T>() where T : Component
        {
            return Components.OfType<T>().ToList();
        }

        public T GetComponent<T>() where T : Component
        {
            return Components.OfType<T>().ToList().FirstOrDefault();
        }

        public T AddComponent<T>() where T : Component, new()
        {
            CheckRequiredComponents<T>();

            // Create a new instance of T
            T component = new T();

            // Set the entity for the component (assuming SetEntity is a method in Component)
            component.SetEntity(this);

            // Call Awake method on the component (assuming Awake is a method in Component)
            component.Awake();

            // Add the component to your collection (Components)
            Components.Add(component);

            return component;
        }



        public T AddComponent<T>(T component) where T : Component
        {
            CheckRequiredComponents<T>();
            Components.Add(component);
            component.SetEntity(this);
            component.Awake();
            return component;
        }

        protected void CheckRequiredComponents<T>() where T : Component
        {
            var typeToAdd = typeof(T);
            var requiredAttributes = typeToAdd.GetCustomAttributes(typeof(RequireComponentAttribute), true);

            foreach (var attribute in requiredAttributes)
            {
                if (attribute is RequireComponentAttribute requireComponentAttribute)
                {
                    var requiredType = requireComponentAttribute.RequiredComponentType;
                    if (!HasComponent(requiredType))
                    {
                        try
                        {
                            // Create a new instance of requiredType
                            Component component = Activator.CreateInstance(requiredType) as Component;

                            if (component != null)
                            {
                                // Set the entity for the component
                                component.SetEntity(this);

                                // Call Awake method on the component
                                component.Awake();

                                // Add the component to your collection (Components)
                                Components.Add(component);

                                Logger.LogToFile($"{typeToAdd.Name} requires a {requiredType.Name} component and has been added.");
                            }
                            else
                            {
                                Logger.Log($"Error creating an instance of {requiredType.Name}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Error adding {typeToAdd.Name} component: {ex.Message}");
                        }
                    }
                }
            }
        }



        public bool HasComponent(Type componentType)
        {
            return Components.Any(c => componentType.IsAssignableFrom(c.GetType()));
        }

        public void Start()
        {
            var enabledComps = Components.Where(x => x.Enabled == true);
            var sortedComps = enabledComps.OrderBy(x => x.ExecutionOrder).Reverse();

            for (int i = sortedComps.Count() - 1; i >= 0; i--)
            {
                var comp = sortedComps.ElementAt(i);
                comp.Start();
            }
            
            
        }

        public void Awake()
        {
            var enabledComps = Components.Where(x => x.Enabled == true);
            var sortedComps = enabledComps.OrderBy(x => x.ExecutionOrder).Reverse();

            for (int i = sortedComps.Count() - 1; i >= 0; i--)
            {
                var comp = sortedComps.ElementAt(i);
                comp.Awake();
            }


        }

        public void Kill()
        {
            var enabledComps = Components.Where(x => x.Enabled == true);
            var sortedComps = enabledComps.OrderBy(x => x.ExecutionOrder).Reverse();

            for (int i = sortedComps.Count() - 1; i >= 0; i--)
            {
                var comp = sortedComps.ElementAt(i);
                comp.Destroy();
            }
            Engine.SceneManager.ActiveScene.Entities.Remove(this);
        }

        public void Update()
        {
            var enabledComps = Components.Where(x => x.Enabled == true);
            var sortedComps = enabledComps.OrderBy(x => x.ExecutionOrder).Reverse();

            for (int i = sortedComps.Count() - 1; i >= 0; i--)
            {
                var comp = sortedComps.ElementAt(i);
                comp.Update();
            }
        }


        public void EarlyUpdate()
        {
            var enabledComps = Components.Where(x => x.Enabled == true);
            var sortedComps = enabledComps.OrderBy(x => x.ExecutionOrder).Reverse();

            for (int i = sortedComps.Count() - 1; i >= 0; i--)
            {
                var comp = sortedComps.ElementAt(i);
                comp.EarlyUpdate();

            }
        }

        public void LateUpdate()
        {
            var enabledComps = Components.Where(x => x.Enabled == true);
            var sortedComps = enabledComps.OrderBy(x => x.ExecutionOrder).Reverse();

            for (int i = sortedComps.Count() - 1; i >= 0; i--)
            {
                var comp = sortedComps.ElementAt(i);
                comp.LateUpdate();
            }
        }

        // Serialize an Entity to a SerializedEntity
        public SerializedEntity ToSerializedEntity()
        {
            var serializedEntity = new SerializedEntity
            {
                Name = this.Name,
                Parent = this.Parent?.ToSerializedEntity(),
                Components = this.Components.ToList()
            };

            return serializedEntity;
        }

        // Deserialize a SerializedEntity to an Entity
        public static Entity FromSerializedEntity(SerializedEntity serializedEntity, Entity parent)
        {
            var entity = new Entity(serializedEntity.Name, parent);

            if (serializedEntity.Parent != null)
            {
                entity.Parent = FromSerializedEntity(serializedEntity.Parent, parent);
            }

            foreach (var component in serializedEntity.Components)
            {
                entity.Components.Add(component);
            }

            return entity;
        }



    }
}
