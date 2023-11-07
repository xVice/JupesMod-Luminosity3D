using Luminosity3D.Builtin;
using Luminosity3D.Utils;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Dynamic;
using Luminosity3DScening;
using System.Reflection;
using System.IO;
using Newtonsoft.Json.Linq;

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

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class NetAttribute : Attribute
    {

        public NetAttribute()
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
        private static Thread listenThread;

        private static List<IPEndPoint> connectedClients = new List<IPEndPoint>();
        public static Queue<GameObject> objectQue = new Queue<GameObject>();

        public static bool StartServer(string ip, int port)
        {
            try
            {
                udpServer = new UdpClient(new IPEndPoint(IPAddress.Parse(ip), port));
                Logger.Log("UDP game server started on ip: " + ip + " port: " + ((IPEndPoint)udpServer.Client.LocalEndPoint).Port, true);


                listenThread = new Thread(new ThreadStart(ListenForData));
                listenThread.Start();
                IsRunning = true;
                IsConnected = true;
                return true;
            }
            catch (Exception e)
            {
                IsRunning = false;
                IsConnected = false;
                Logger.Log("NET: Error when starting the server: " + e.ToString(), true, LogType.Error);
                return false;
            }
        }


        private static void ListenForData()
        {
            try
            {
                while (true)
                {
                    IPEndPoint anyIP = null;
                    byte[] data = udpServer.Receive(ref anyIP);

                    if (!connectedClients.Contains(anyIP))
                    {
                        connectedClients.Add(anyIP);
                    }

                    string receivedData = Encoding.UTF8.GetString(data);
                    byte[] delimiter = new byte[] { 0x13, 0x37, 0x69, 0x42, 0x0 };
                    byte[] framedPacket = data;

                    foreach (byte[] packetBytes in SplitPackets(framedPacket, delimiter))
                    {
                        string packet = Encoding.UTF8.GetString(packetBytes);
                        if (!string.IsNullOrEmpty(packet))
                        {
                            

                            if (IsValidJson(packet))
                            {
                                var gameObject = GameObjectSerializer.DeserializeFromString(packet);
                                objectQue.Enqueue(gameObject);
                                //Logger.Log($"NET: Received data for: {gameObject.NetCode}", true);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log("CLIENT: Error: " + e.ToString(), true, LogType.Error);
            }
        }


        // Helper function to split framed data into individual packets using a delimiter
        private static IEnumerable<byte[]> SplitPackets(byte[] framedData, byte[] delimiter)
        {
            List<byte> currentPacket = new List<byte>();
            for (int i = 0; i < framedData.Length; i++)
            {
                bool isDelimiter = i + delimiter.Length <= framedData.Length &&
                                   delimiter.SequenceEqual(framedData.Skip(i).Take(delimiter.Length));
                if (isDelimiter)
                {
                    if (currentPacket.Count > 0)
                        yield return currentPacket.ToArray();
                    currentPacket.Clear();
                    i += delimiter.Length - 1; // Skip delimiter
                }
                else
                {
                    currentPacket.Add(framedData[i]);
                }
            }
            if (currentPacket.Count > 0)
                yield return currentPacket.ToArray();
        }


        public static bool IsValidJson(string json)
        {
            try 
            {
                JToken.Parse(json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void SendSceneToClients()
        {
            try
            {
                if (Net.IsRunning)
                {
                    foreach (var ent in SceneManager.ActiveScene.Entities)
                    {
                        if (string.IsNullOrEmpty(ent.NetCode))
                        {
                            ent.NetCode = Guid.NewGuid().ToString();
                        }

                        var packet = GameObjectSerializer.SerializeToString(ent);
                        byte[] packetBytes = Encoding.UTF8.GetBytes(packet);

                        byte[] delimiter = new byte[] { 0x13, 0x37, 0x69, 0x42, 0x0 };
                        byte[] framedPacket = delimiter.Concat(packetBytes).Concat(delimiter).ToArray();

                        SendMessageToAllClients(framedPacket);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log("NET: Error when sending the scene to clients: " + e.ToString(), true, LogType.Error);
            }

        }



        public static bool StopServer()
        {
            Logger.Log("Server shutting down", true);
            if(udpServer != null)
            {
                udpServer.Close();
            }

            if (listenThread != null && listenThread.IsAlive)
            {
                listenThread.Abort();
            }
            Logger.Log("Server shutdown", true);
            return true;
        }

        public static bool JoinServer(string serverIp, int serverPort)
        {
            try
            {
                udpServer = new UdpClient(); // Create a new UDP client for the client-side connection

                // You can specify the server IP and port you want to connect to
                IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);

                // Start a separate thread to listen for responses from the server
                listenThread = new Thread(new ThreadStart(ListenForData));
                listenThread.Start();

                IsConnected = true;
                Logger.Log($"NET: Connected to: {serverIp}:{serverPort}", true, LogType.Information);
                return true;
            }
            catch (Exception e)
            {
                Logger.Log("Error: " + e.ToString(), false, LogType.Error);
                return false;
            }
        }

        public static void SendMessageToClient(byte[] buffer, IPEndPoint clientEndpoint)
        {
            udpServer.Send(buffer, buffer.Length, clientEndpoint);
        }

        public static void SendMessageToAllClients(byte[] data)
        {
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
        public Dictionary<Type, List<LuminosityBehaviour>> caches = new Dictionary<Type, List<LuminosityBehaviour>>();
        public float lastRenderTime = 0.0f;
        public float lastPhysicsTime = 0.0f;
        public float lastUpdateTime = 0.0f;

        public bool HasCache<T>(T comp) where T : LuminosityBehaviour
        {
            if (caches.ContainsKey(comp.GetType()))
            {
                return caches[comp.GetType()].Contains(comp);

            }
            return false;
        }

        public void ClearCache()
        {
            caches.Clear();
        }

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

                if (sortedBatches != null && sortedBatches.Length > 0)
                {
                    foreach (var batch in sortedBatches)
                    {
                        var cam = SceneManager.ActiveScene.activeCam.GetComponent<Camera>();
                        if (batch.GameObject != null)
                        {
                            batch.model.RenderFrame(batch.GameObject.Transform, cam);

                        }
                        //batch.model.RenderForStencil();

                    }

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

    public interface Networkable
    {
        /// <summary>
        /// Will be called from the server with the gameobject it received from the server
        /// </summary>
        /// <param name="go"></param>
        public abstract void Net(GameObject go);
    }

    public class LuminosityBehaviour
    {
        public int ExecutionOrder = 0;

        public string NetCode = string.Empty;

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

        public T AddComponent<T>(T comp) where T : LuminosityBehaviour, new()
        {
            return GameObject.AddComponent<T>(comp);
        }

        public void RemoveComponent<T>() where T : LuminosityBehaviour
        {
            GameObject.RemoveComponent<T>();
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
