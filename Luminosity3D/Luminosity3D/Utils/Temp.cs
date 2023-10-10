using Luminosity3D.EntityComponentSystem;
using Luminosity3D.Rendering;
using Luminosity3DRendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Utils
{
    //This is purely so its all really open and easy to understand.
    public static class Temp
    {
        public const string TempPath = "./temp";

        public static void ClearTemp()
        {
            if (Directory.Exists(TempPath))
            {
                Directory.Delete(TempPath, true);
                Directory.CreateDirectory(TempPath);
            }
            else
            {
                Directory.CreateDirectory(TempPath);
            }
        }

        public static void CreateFolder(string folderName)
        {
            string folderPath = Path.Combine(TempPath, folderName);
            Directory.CreateDirectory(folderPath);
        }

        public static void DeleteFolder(string folderName)
        {
            string folderPath = Path.Combine(TempPath, folderName);
            if (Directory.Exists(folderPath))
            {
                Directory.Delete(folderPath, true);
            }
        }

        public static string Get(string name)
        {
            return File.ReadAllText(Path.Combine(TempPath, name));
        }

        public static string[] GetLines(string name)
        {
            return File.ReadAllLines(Path.Combine(TempPath, name));
        }

        public static byte[] GetFileBytes(string name)
        {
            return File.ReadAllBytes(Path.Combine(TempPath, name));
        }

        public static void WriteLines(string name, string[] lines)
        {
            File.WriteAllLines(Path.Combine(TempPath, name), lines);
        }

        public static void Write(string name, string content)
        {
            File.WriteAllText(Path.Combine(TempPath, name), content);
        }

        public static void WriteBytes(string name, byte[] bytes)
        {
            File.WriteAllBytes(Path.Combine(TempPath, name), bytes);
        }
    }

    public static class InputManager
    {
        private static KeyboardState kb = Engine.Renderer.KeyboardState;
        private static MouseState mice = Engine.Renderer.MouseState;
        private static JoystickState cont = Engine.Renderer.JoystickStates[0];

        public static JoystickState GetController()
        {
            return cont;
        }

        public static KeyboardState GetKeyboard()
        {
            return kb;
        }

        public static MouseState GetMouse()
        {
            return mice;
        }

        public static float GetMouseDeltaX()
        {
            return mice.Delta.X;
        }

        public static float GetMouseDeltaY()
        {
            return mice.Delta.Y;
        }

        public static Vector2 GetMouseDelta()
        {
            return LMath.ToVec(mice.Delta);
        }

        public static bool GetKeyDown(Keys key)
        {
            return kb.IsKeyDown(key);
        }

        public static bool GetKeyPressed(Keys key)
        {
            return kb.IsKeyPressed(key);
        }

        public static bool IsKeyReleased(Keys key)
        {
            return kb.IsKeyReleased(key);
        }

        public static bool IsAnyKeyDown()
        {
            return kb.IsAnyKeyDown;
        }
    }


    public interface IResourceLoader<T>
    {
        T Load(string path);
    }

    public class ResourceBase<T>
    {
        public string Path { get; private set; }
        private T asset;

        public ResourceBase(string path, T asset)
        {
            Path = path;
            this.asset = asset;
        }
    }

    public static class RpcMethodParameterSerializer
    {
        public static string SerializeMethodParameters(object target, MethodInfo methodInfo)
        {
            var rpcAttribute = methodInfo.GetCustomAttribute<RPCAttribute>();
            if (rpcAttribute == null)
            {
                throw new InvalidOperationException("Method is not decorated with RPCAttribute");
            }

            var parameters = methodInfo.GetParameters();
            var parameterValues = parameters.Select(p =>
            {
                var prop = target.GetType().GetProperty(p.Name);
                return prop.GetValue(target, null);
            }).ToArray();

            var json = JsonConvert.SerializeObject(parameterValues, Formatting.Indented);
            return json;
        }

        public static object InvokeMethodWithArguments(object target, string methodName, object[] arguments)
        {
            MethodInfo methodInfo = target.GetType().GetMethod(methodName);
            if (methodInfo == null)
            {
                throw new InvalidOperationException($"Method '{methodName}' not found on the target object.");
            }

            try
            {
                object result = methodInfo.Invoke(target, arguments);
                return result;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during method invocation.
                // You can log the exception or rethrow it as needed.
                throw ex;
            }
        }

        public static object[] DeserializeMethodParameters(string json, MethodInfo methodInfo)
        {
            var rpcAttribute = methodInfo.GetCustomAttribute<RPCAttribute>();
            if (rpcAttribute == null)
            {
                throw new InvalidOperationException("Method is not decorated with RPCAttribute");
            }

            var parameters = methodInfo.GetParameters();
            var parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

            try
            {
                var parameterValues = JsonConvert.DeserializeObject(json);
                if (parameterValues is object[] arrayOfObjects)
                {
                    return arrayOfObjects;
                }
            }
            catch (Exception ex)
            {
                // Handle deserialization error, log the exception, or throw a custom exception.
                // Example: Console.WriteLine("Deserialization error: " + ex.Message);
                // You might want to throw a specific exception or return a default value based on your use case.
            }

            return null; // Handle deserialization error gracefully or throw an exception.
        }
    }


    public static class Resources
    {
        public const string ResourcesPath = "./Resources";
        private static Dictionary<string, object> resourceCache = new Dictionary<string, object>();
        private static Dictionary<Type, object> resourceLoaders = new Dictionary<Type, object>();

        public static void LoadBuiltinResourceTypes()
        {
            RegisterResourceLoader<TextureProgram, TextureProgramLoader>();
            RegisterResourceLoader<Model, AssimpLoaderLoader>();
            // Register other resource types in a similar manner
        }

        public static void CreateResourcesFolder()
        {
            if (!Directory.Exists(ResourcesPath))
            {
                Directory.CreateDirectory(ResourcesPath);
            }
        }

        public static T Get<T>(string name)
        {
            if (resourceCache.TryGetValue(name, out object cachedResource) && cachedResource is T cachedValue)
            {
                return cachedValue;
            }

            if (resourceLoaders.TryGetValue(typeof(T), out var loader))
            {
                string fullPath = Path.Combine(ResourcesPath, name);
                if (File.Exists(fullPath))
                {
                    var resourceLoader = (IResourceLoader<T>)loader;
                    T loadedResource = resourceLoader.Load(fullPath);
                    resourceCache[name] = loadedResource;
                    return loadedResource;
                }
            }

            return default; // Resource not found or unsupported type
        }

        // Register a custom resource loader for a specific type
        public static void RegisterResourceLoader<T, TLoader>()
            where TLoader : IResourceLoader<T>, new()
        {
            var loader = new TLoader();
            resourceLoaders[typeof(T)] = loader;
        }

        // Get a resource by type
        public static T GetByType<T>(string name)
        {
            if (resourceLoaders.TryGetValue(typeof(T), out var loader))
            {
                string fullPath = Path.Combine(ResourcesPath, name);
                if (File.Exists(fullPath))
                {
                    var resourceLoader = (IResourceLoader<T>)loader;
                    T loadedResource = resourceLoader.Load(fullPath);
                    resourceCache[name] = loadedResource;
                    return loadedResource;
                }
            }

            return default; // Resource not found or unsupported type
        }
    }


    // Example resource loader for TextureProgram
    //i think this is good now
    public class TextureProgramLoader : IResourceLoader<TextureProgram>
    {
        public TextureProgram Load(string path)
        {
            // Implement your TextureProgram loading logic here
            // You can return an instance of TextureProgram after loading it from the file.
            return new TextureProgram(path); // Replace with actual loading logic
        }
    }

    public class AssimpLoaderLoader : IResourceLoader<Model>
    {
        public Model Load(string path)
        {
            // Implement your TextureProgram loading logic here
            // You can return an instance of TextureProgram after loading it from the file.
            return new Model(path); // Replace with actual loading logic
        }
    }

}
