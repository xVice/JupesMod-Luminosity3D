using Luminosity3D.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.EntityComponentSystem
{
    [DataContract]
    public class SerializedEntity
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public SerializedEntity Parent { get; set; }


        [DataMember]
        public List<Component> Components { get; set; }
    }

    public class Entity
    {
        public string Name { get; set; }    

        public Engine Engine;

        public Entity Parent = null;
        public Pool<Component> Components { get; set; } = new Pool<Component>();

        public Entity(string name)
        {
            Name = name;
            Engine = Engine.Instance;
          
        }

        public Entity(string name, Entity parent)
        {
            Name = name;
            Engine = Engine.Instance;
            Parent = parent;
    
        }


        public List<Component> GetComponents<T>() where T : Component
        {
            return Components.GetContent().Where(x => x.GetType() == typeof(T)).ToList();
        }

        public Component GetComponent<T>() where T : Component
        {
            return Components.GetContent().Where(x => x.GetType() == typeof(T)).ToList().First();
        }

        public Component AddComponent<T>(T component) where T : Component
        {
            Components.Enqueue(component);
            component.Entity = this;
            return component;
        }



        public void Start()
        {
            foreach(var comp in Components.GetContent())
            {
                comp.Start();
            }
            
            
        }

        public void Awake()
        {
            foreach (var comp in Components.GetContent())
            {
                comp.Awake();
            }


        }

        public void Update()
        {
            foreach (var comp in Components.GetContent())
            {
                comp.EarlyUpdate();
                comp.Update();
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
                Components = this.Components.GetContent().ToList()
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
                entity.Components.Enqueue(component);
            }

            return entity;
        }



    }
}
