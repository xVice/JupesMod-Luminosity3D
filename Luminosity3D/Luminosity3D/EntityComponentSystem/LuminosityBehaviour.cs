using Luminosity3D.Builtin;
using Luminosity3D.Rendering;
using Luminosity3D.Utils;
using Luminosity3DRendering;
using Luminosity3DScening;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.EntityComponentSystem
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireComponentAttribute : Attribute
    {
        public Type RequiredComponentType { get; }

        public RequireComponentAttribute(Type requiredComponentType)
        {
            RequiredComponentType = requiredComponentType;
        }
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RPCAttribute : Attribute
    {

        public RPCAttribute()
        {
            
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class PredictedAttribute : Attribute
    {

        public PredictedAttribute()
        {

        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true)]
    public class SerializeFieldAttribute : Attribute
    {

        public SerializeFieldAttribute()
        {

        }
    }

    public static class Net
    {
        private static string ConnectedToIp { get; set; } = string.Empty;
        private static string ConnectedToPort { get; set; } = string.Empty;
        private static bool Connected { get; set; } = false;

        public static bool ConnectToServer(string ip, int port)
        {


            return true;
        }

       // public static bool SendToServer()


    }



    public class ComponentCache
    {
        // Use a dictionary to map component types to their respective caches
        private Dictionary<Type, List<LuminosityBehaviour>> caches = new Dictionary<Type, List<LuminosityBehaviour>>();
        public float lastRenderTime = 0.0f;
        public float lastPhysicsTime = 0.0f;
        public float lastUpdateTime = 0.0f;

        public List<T> GetComponents<T>() where T : LuminosityBehaviour
        {
            var type = typeof(T);
            if (caches.ContainsKey(type))
            {
                return caches[type].Cast<T>().ToList();
            }
            return null;
        }

        
        public void UpdatePass()
        {
            float currentTime = Time.time * 1000.0f; // Convert to milliseconds
            float deltaTime = currentTime - lastUpdateTime; // Calculate time elapsed since the last pass in ms
            lastUpdateTime = currentTime;


            foreach (var cache in caches)
            {
                Type componentType = cache.Key;
                if (componentType != typeof(RigidBodyComponent))
                {
                    List<LuminosityBehaviour> components = cache.Value;
                    foreach (var comp in components)
                    {
                        comp.LateUpdate();
                        comp.Update();
                        comp.LateUpdate();

                    }
                }
            }
        }

        public void PhysicsPass()
        {
            float currentTime = Time.time * 1000.0f; // Convert to milliseconds
            float deltaTime = currentTime - lastPhysicsTime; // Calculate time elapsed since the last pass in ms
            lastPhysicsTime = currentTime;


            if (caches.ContainsKey(typeof(RigidBodyComponent)))
            {
                foreach (var comp in caches[typeof(RigidBodyComponent)])
                {
                    if (comp is LuminosityBehaviour behav)
                    {
                        behav.LateUpdate();
                        behav.Update();
                        behav.LateUpdate();
                    }
                }
            }

        }

        public void RenderPass()
        {   

            if (Engine.SceneManager.ActiveScene.activeCam == null)
                return;
            float currentTime = Time.time * 1000.0f; // Convert to milliseconds
            float deltaTime = currentTime - lastRenderTime; // Calculate time elapsed since the last pass in ms
            lastRenderTime = currentTime;


            if (caches.ContainsKey(typeof(MeshBatch)))
            {
                var sortedBatches = caches[typeof(MeshBatch)]
                    .Cast<MeshBatch>()
                    .ToArray();

                foreach(var batch in sortedBatches)
                {
                    var cam = Engine.SceneManager.ActiveScene.activeCam.GetComponent<Camera>();

                    batch.model.RenderFrame(batch.GameObject.Transform, cam);
                    //batch.model.RenderForStencil();
                }
            }
        }

        public void CacheComponent(LuminosityBehaviour comp)
        {

            var compType = comp.GetType();

            if (!caches.ContainsKey(compType))
            {
                caches[compType] = new List<LuminosityBehaviour>();
            }
            caches[compType].Add(comp);

        }



    }




    public class LuminosityBehaviour
    {
        public int ExecutionOrder = 0;
       
        public string Name = string.Empty;

        public GameObject GameObject { get; set; }
        public TransformComponent Transform
        {
            get
            {
                return GameObject.Transform;
            }
        }
        public List<GameObject> Children { get; set; }

        public LuminosityBehaviour()
        {

        }

        public LuminosityBehaviour(string name)
        {
            Name = name;
        }

        public bool HasComponent<T>() where T : LuminosityBehaviour
        {
            return GameObject.HasComponent<T>();
        }

        public List<T> GetComponents<T>() where T : LuminosityBehaviour
        {
            List<T> result = new List<T>();
            foreach (var component in GameObject.GetComponents<T>())
            {
                result.Add(component);
            }
            return result;
        }

        public T GetComponent<T>() where T : LuminosityBehaviour
        {
            if(GameObject.GetComponent<T>() == null)
            {
                Logger.Log("Couldnt find " + typeof(T).FullName);
            }
            return GameObject.GetComponent<T>();
        }

        public T AddComponent<T>() where T : LuminosityBehaviour, new()
        {
            return GameObject.AddComponent<T>();
        }

        public virtual void OnStart()
        {

        }

        public virtual void OnLoad()
        {

        }

        public virtual void Awake()
        {

        }

        public virtual void Start()
        {

        }

        public virtual void EarlyUpdate()
        {

        }

        public virtual void Update()
        {

        }

        public virtual void LateUpdate()
        {

        }
    }
}
