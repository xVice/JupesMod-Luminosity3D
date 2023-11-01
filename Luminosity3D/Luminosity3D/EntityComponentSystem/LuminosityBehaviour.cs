using Luminosity3D.Builtin;
using Luminosity3D.Utils;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Dynamic;
using Luminosity3DScening;

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


    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class RPCAttribute : Attribute
    {
        public string MethodName { get; }

        public RPCAttribute(string methodName)
        {
            MethodName = methodName;
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
        public static bool IsConnected = false;
        public static bool IsRunning = false;

        private static UdpClient udpServer;
        private static IPEndPoint anyIP;
        private static Thread listenThread;

        private static List<IPEndPoint> connectedClients = new List<IPEndPoint>();

        public static bool StartServer(string ip, int port)
        {
            try
            {
                udpServer = new UdpClient(new IPEndPoint(IPAddress.Parse(ip), port));
                Logger.LogToFile("UDP game server started on port " + ((IPEndPoint)udpServer.Client.LocalEndPoint).Port, false);

                listenThread = new Thread(new ThreadStart(ListenForData));
                listenThread.Start();
                return true;
            }
            catch (Exception e)
            {
                Logger.LogToFile("Error: " + e.ToString(), false, LogType.Error);
                return false;
            }
        }

        private static void ListenForData()
        {
            try
            {
                while (true)
                {
                    byte[] data = udpServer.Receive(ref anyIP);
                    string message = Encoding.ASCII.GetString(data);
                    Logger.LogToFile("Received: " + message, false);

                    // Add the client to the list of connected clients if not already in the list
                    if (!connectedClients.Contains(anyIP))
                    {
                        connectedClients.Add(anyIP);
                    }

                    // Process game logic, handle client data here

                    // Send a response back to the client if needed
                    string response = "Server: Received - " + message;
                    Logger.Log(response);
                }
            }
            catch (Exception e)
            {
                Logger.LogToFile("Error: " + e.ToString(), false, LogType.Error);
            }
        }

        public static bool StopServer()
        {
            Logger.LogToFile("Server shutting down", false);
            if(udpServer != null)
            {
                udpServer.Close();
            }

            if (listenThread != null && listenThread.IsAlive)
            {
                listenThread.Abort();
            }
            Logger.LogToFile("Server shutdown", false);
            return true;
        }

        public static bool JoinServer(string serverIp, int serverPort)
        {
            try
            {
                udpServer = new UdpClient(); // Create a new UDP client for the client-side connection

                // You can specify the server IP and port you want to connect to
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

                // Send a message to the server to establish the connection
                string joinRequest = "Client: Joining Server";
                byte[] requestData = Encoding.ASCII.GetBytes(joinRequest);

                // Send the join request to the server
                udpServer.Send(requestData, requestData.Length, serverEndPoint);

                // Start a separate thread to listen for responses from the server
                listenThread = new Thread(new ThreadStart(ListenForData));
                listenThread.Start();

                IsConnected = true;
                return true;
            }
            catch (Exception e)
            {
                Logger.LogToFile("Error: " + e.ToString(), false, LogType.Error);
                return false;
            }
        }

        public static void SendMessageToClient(string message, IPEndPoint clientEndpoint)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            udpServer.Send(data, data.Length, clientEndpoint);
        }

        public static void SendMessageToAllClients(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            for (int i = connectedClients.Count - 1; i >= 0; i--)
            {
                var clientEndpoint = connectedClients[i];
                udpServer.Send(data, data.Length, clientEndpoint);
            }
        }
    }


    public class ComponentCache
    {
        // Use a dictionary to map component types to their respective caches
        private Dictionary<Type, List<LuminosityBehaviour>> caches = new Dictionary<Type, List<LuminosityBehaviour>>();
        public float lastRenderTime = 0.0f;
        public float lastPhysicsTime = 0.0f;
        public float lastUpdateTime = 0.0f;

        public void RemoveCachedComponent(LuminosityBehaviour behav)
        {
            var type = behav.GetType();

            if (caches.ContainsKey(type))
            {
                caches[type].Remove(behav);
            }
        }

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

            if (SceneManager.ActiveScene.activeCam == null)
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
                    var cam = SceneManager.ActiveScene.activeCam.GetComponent<Camera>();

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

        [JsonIgnore]
        public GameObject GameObject { get; set; }

        [JsonIgnore]
        public TransformComponent Transform
        {
            get
            {
                return GameObject.Transform;
            }
        }

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
                return null;
            }
            return GameObject.GetComponent<T>();
        }

        public T AddComponent<T>() where T : LuminosityBehaviour, new()
        {
            return GameObject.AddComponent<T>();
        }

        public void Remove()
        {
            SceneManager.ActiveScene.cache.RemoveCachedComponent(this);
            GameObject.components.Remove(this.GetType());
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
