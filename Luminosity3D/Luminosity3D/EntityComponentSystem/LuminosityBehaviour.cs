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

    public class SVar<T>
    {
        public bool svcheats = false;
        public bool HostOnly = true;

        public string Name = string.Empty;
        public T Value { get; set; }

        public SVar(T value, string name)
        {
            Value = value;
            Name = name;
        }
    }

    public class SVars
    {
        public Dictionary<string, object> Svars = null;

        public SVars()
        {
            Svars = new Dictionary<string, object>();
        }

        public void Set(string name, object value)
        {
            Svars.Add(name, value);
        }

        public T Get<T>(string name)
        {
            if (Svars.ContainsKey(name))
            {
                try
                {
                    return (T)Svars[name];
                }
                catch
                {
                    return default(T);
                }
            }
            return default(T);
        }
    }

    public class Client
    {
        public IPEndPoint Endpoint;

        public bool IsHost = false;

        public delegate void MessageReceivedEventHandler(byte[] message);
        public event MessageReceivedEventHandler MessageReceived;

        public SVars SVars = new SVars();


        private static Thread listenThread;

        private static TcpClient tcpServer;
        private static UdpClient udpServer;

        public Client(IPEndPoint endpoint)
        {
            Endpoint = endpoint;
            tcpServer = new TcpClient(endpoint);
            udpServer = new UdpClient(endpoint);
        }

        public Client(IPEndPoint endpoint, bool isHost)
        {
            Endpoint = endpoint;
            tcpServer = new TcpClient(endpoint);
            udpServer = new UdpClient(endpoint);
            IsHost = isHost;
        }

        public void Send(byte[] buffer)
        {
            GetUDP().Send(buffer, buffer.Length, Endpoint);
        }

        public TcpClient GetTCP()
        {
            return tcpServer;
        }

        public UdpClient GetUDP()
        {
            return udpServer;
        }

        public SVars GetClientVars()
        {
            return SVars;
        }
        public void OnMessageReceived(byte[] message)
        {
            MessageReceived?.Invoke(message);
        }

        public void StartListenerThread()
        {
            listenThread = new Thread(new ThreadStart(ListenForData));
            listenThread.Start();
        }

        public void Disconect()
        {
            CloseConnection();
        }

        public void CloseConnection()
        {
            if (udpServer != null)
            {
                udpServer.Close();
            }

            if(tcpServer != null)
            {
                tcpServer.Close();
            }

            if (listenThread != null && listenThread.IsAlive)
            {
                listenThread.Abort();
            }
            Logger.Log("Server shutdown", true);
        }

        private void ListenForData()
        {

            try
            {
                while (true)
                {
                    IPEndPoint anyIP = null;
                    byte[] data = GetUDP().Receive(ref anyIP);

                    if (Net.GetClients().FirstOrDefault(x => x.Endpoint == anyIP) != null)
                    {
                        Net.GetClients().Add(new Client(anyIP));
                    }

                    OnMessageReceived(data);


                }
            }
            catch (Exception e)
            {
                //droppedPacketCount++; // Increment the dropped packet count
                Logger.Log("CLIENT: Error: " + e.ToString(), true, LogType.Error);
            }
        }

    }

    
    public static class BehavNet
    {
        private static Queue<Scene> objectQue = new Queue<Scene>();

        public static Queue<Scene> GetSceneQue()
        {
            return objectQue;
        }

        public static void SetupNetworking()
        {
            if(Net.LocalClient != null)
            {
                Net.LocalClient.MessageReceived += HandleLumBehavs;
            }

        }

        private static void HandleLumBehavs(byte[] data)
        {
            byte[] delimiter = new byte[] { 0x13, 0x37, 0x69, 0x42, 0x0 };
            byte[] framedPacket = data;

            foreach (byte[] packetBytes in SplitPackets(framedPacket, delimiter))
            {
                var scene = Scene.FromJson(Encoding.UTF8.GetString(packetBytes));
                objectQue.Enqueue(scene);
            }

        }

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

    }


    public static class Net
    {
        public static bool IsConnected = false;
        public static bool IsRunning = false;

        public static Client LocalClient = null;
        private static List<Client> clients = new List<Client>();

        private static byte[] sceneBuf = new byte[32];



        public static bool StartServer(string ip, int port)
        {
            try
            {
                LocalClient = new Client(new IPEndPoint(IPAddress.Parse(ip), port), true);

                Logger.Log("UDP game server started on ip: " + ip + " port: " + port, true);
                LocalClient.StartListenerThread();
                BehavNet.SetupNetworking();
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


        public static void StopServer()
        {
            Logger.Log("Server shutting down", true);
            if (LocalClient.IsHost)
            {
                LocalClient.CloseConnection();
            }

        }


        public static bool JoinServer(string serverIp, int serverPort)
        {
            try
            {
                //var server = new Client(new IPEndPoint(IPAddress.Parse(serverIp), serverPort), true);
                LocalClient = new Client(new IPEndPoint(IPAddress.Parse(GetLocalIpAddress()), serverPort), false);

                string joinRequest = "Client: Joining Server";
                byte[] requestData = Encoding.ASCII.GetBytes(joinRequest);

                LocalClient.StartListenerThread();
                BehavNet.SetupNetworking();
                // Send the join request to the server
                LocalClient.Send(requestData);
                // Start a separate thread to listen for responses from the server

                IsConnected = true;
                Logger.Log($"NET: Connected to: {serverIp}:{serverPort}", true, LogType.Information);
                return true;
            }
            catch (Exception e)
            {
                Logger.Log("NET: Error: when joining a server: " + e.ToString(), false, LogType.Error);
                return false;
            }
        }

        static string GetLocalIpAddress()
        {
            string localIp = "";

            try
            {
                // Get the local machine's host name
                string hostName = Dns.GetHostName();

                // Get the IP addresses associated with the host name
                IPAddress[] localIpAddresses = Dns.GetHostAddresses(hostName);

                // Choose the first IPv4 address (assuming that's what you want)
                foreach (IPAddress ipAddress in localIpAddresses)
                {
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIp = ipAddress.ToString();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error getting local IP address: " + ex.Message);
            }

            return localIp;
        }


        public static void SendMessageToAllClients(byte[] data)
        {
            for (int i = clients.Count - 1; i >= 0; i--)
            {
                var client = clients[i];
                client.Send(data);
            }
        }


        public static List<Client> GetClients()
        {
            return clients;
        }

        // Helper function to split framed data into individual packets using a delimiter
       

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

                        byte[] packetData = Encoding.UTF8.GetBytes(SceneManager.ActiveScene.ToJson());

                        byte[] delimiter = new byte[] { 0x13, 0x37, 0x69, 0x42, 0x0 };
                        byte[] framedPacket = delimiter.Concat(packetData).Concat(delimiter).ToArray();

                        SendMessageToAllClients(framedPacket);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log("NET: Error when sending the scene to clients: " + e.ToString(), true, LogType.Error);
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
                            batch.GetModel().RenderFrame(batch.GameObject.Transform, cam);

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
