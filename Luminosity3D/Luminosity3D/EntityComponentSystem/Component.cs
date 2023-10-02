using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.EntityComponentSystem
{
    public abstract class Component : IEngineQueryable
    {
        private Entity Entity { get; set; }
        public int ExecutionOrder { get; set; } = 1;
        public string Name { get; set; } = "Component";
        public string Description { get; set; } = "A Components Description";
        public bool Enabled { get; set; } = true;

        public Component()
        {

        }

        public T GetComponent<T>() where T : Component
        {
            if (Entity.HasComponent(typeof(T)))
            {
                return Entity.GetComponent<T>();
            }
            return null;
        }

        public void SetEntity(Entity entity)
        {
            Entity = entity;
        }

        public Entity GetEntity()
        {
            return Entity;
        }

        public void Destroy()
        {
            OnDestroy();
            Entity.Components.Remove(this);
        }

        public void Switch()
        {
            if (Enabled)
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }

        public void Enable()
        {
            if (!Enabled)
            {
                Enabled = true;
                OnEnable();
            }
        }

        public void Disable()
        {
            if (Enabled)
            {
                Enabled = false;
                OnDisable();
            }
        }

        

        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void EarlyUpdate() { }
        public virtual void Update() { }
        public virtual void LateUpdate() { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }
    }
}
