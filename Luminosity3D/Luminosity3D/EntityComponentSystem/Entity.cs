using Luminosity3D.Utils;
using System;
using System.Collections;
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
        public List<Component> Components { get; set; } = new List<Component>();
        public int ExecutionOrder = int.MaxValue;

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

        public Entity(string name, int executionOrder)
        {
            Name = name;
            Engine = Engine.Instance;
            ExecutionOrder = executionOrder;

        }

        public Entity(string name, Entity parent, int executionOrder)
        {
            Name = name;
            Engine = Engine.Instance;
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


        public Component AddComponent<T>(T component) where T : Component
        {
            Components.Add(component);
            component.Entity = this;
            component.Awake();
            return component;
        }



        public void Start()
        {
            var sortedComps = Components.OrderBy(x => x.ExecutionOrder).Reverse();

            for (int i = sortedComps.Count(); i < 0; i--)//Use for loop to avoid IEnumerable exception by just looping backwards and getting a refrence to it from the list
            {
                var comp = sortedComps.ElementAt(i);
                comp.Start();
            }
            
            
        }

        public void Awake()
        {
            var sortedComps = Components.OrderBy(x => x.ExecutionOrder).Reverse();

            for (int i = sortedComps.Count(); i < 0; i--)//Use for loop to avoid IEnumerable exception by just looping backwards and getting a refrence to it from the list
            {
                var comp = sortedComps.ElementAt(i);
                comp.Awake();
            }


        }

        public void Update()
        {

            var sortedComps = Components.OrderBy(x => x.ExecutionOrder).Reverse();

            for (int i = sortedComps.Count(); i < 0; i--)//Use for loop to avoid IEnumerable exception by just looping backwards and getting a refrence to it from the list
            {
                var comp = sortedComps.ElementAt(i);
                comp.Update();

            }
        }

        public void EarlyUpdate()
        {
            var sortedComps = Components.OrderBy(x => x.ExecutionOrder).Reverse();

            for (int i = sortedComps.Count(); i < 0; i--)//Use for loop to avoid IEnumerable exception by just looping backwards and getting a refrence to it from the list
            {
                var comp = sortedComps.ElementAt(i);
                comp.EarlyUpdate();

            }
        }

        public void LateUpdate()
        {
            var sortedComps = Components.OrderBy(x => x.ExecutionOrder).Reverse();

            for (int i = sortedComps.Count(); i < 0; i--)//Use for loop to avoid IEnumerable exception by just looping backwards and getting a refrence to it from the list
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
