using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.EntityComponentSystem
{
    public abstract class Component
    {
        public Entity Entity { get; set; }
        public Engine Engine { get => Entity.Engine; }

        private double deltaTime { get => Engine.DeltaTime; }

        public int ExecutionOrder { get; set; } = int.MaxValue;
        public string Name { get; set; }
        public string Description { get; set; }
        public Component() { }
        public Component(Entity entity,string name, string description, int executionOrder)
        {
            Entity = entity;
            Name = name;
            Description = description;
            ExecutionOrder = executionOrder;
 
        }

        protected Component(Entity entity)
        {
            Entity = entity;
        }

        public abstract void Awake();
        public abstract void Start();
        public abstract void EarlyUpdate();
        public abstract void Update();
        public abstract void LateUpdate();
        public abstract void OnEnable();
        public abstract void OnDisable();
        public abstract void OnDestroy();
        
    }
}
