using MyGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Utils
{
    public class ModelLoader : ResourceLoader<AssimpModel>
    {
        public override AssimpModel OnLoad(string path)
        {
            if (HasCache(path))
            {
                return Get(path);
            }
            return CacheRes(path, new AssimpModel(path));
        }
    }

    public interface IResourceLoader
    {

    }

    public abstract class ResourceLoader<T> : IResourceLoader
    {
        public Dictionary<string ,T> Cache = new Dictionary<string, T>();

        internal T CacheRes(string path,T item)
        {
            Cache.Add(path, item);
            return item;
        }

        internal bool HasCache(string path)
        {
            return Cache.ContainsKey(path);
        }

        internal T Get(string path)
        {
            return (T)Cache[path];
        }


        public abstract T OnLoad(string path);
    }

    public class Resource
    {
        public string Name { get; private set; }
        public string PathInMemory { get; private set; }
        public List<Resource> SubDirectories { get; private set; }



        public List<string> Resources { get; private set; }

        public Resource(string name, string pathInMemory)
        {
            Name = name;
            PathInMemory = pathInMemory;
            SubDirectories = new List<Resource>();
        }

        public Resource(string name, Resource[] subresoucres, string pathInMemory, string[] resources)
        {
            Name = name;

            Resources = resources.ToList();
            PathInMemory = pathInMemory;
            SubDirectories = subresoucres.ToList();
        }

        public Resource(string name, List<Resource> subDirectories)
        {
            Name = name;
            PathInMemory = null;
            SubDirectories = subDirectories;
        }

        public T Get<T>(string fileName)
        {
            if (PathInMemory != null)
            {
                try
                {
                    // Assuming the content is in-memory (e.g., a byte array)
                    var loader = ResourcesManager.HasLoader<T>();
                    if(loader == null)
                    {
                        return default(T);
                    }
                    return loader.OnLoad(fileName);

                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to read content for resource '{Name}/{fileName}': {ex.Message}", type: LogType.Error);
                    return default(T);
                }
            }
            else
            {
                Logger.Log($"Resource '{Name}' is a directory, not a file.", type: LogType.Error);
                return default(T);
            }
        }

        public string Get(string fileName)
        {
            if (PathInMemory != null)
            {
                try
                {
                    // Assuming the content is in-memory (e.g., a byte array)
                    return File.ReadAllText(Path.Combine(PathInMemory, fileName));

                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to read content for resource '{Name}/{fileName}': {ex.Message}", type: LogType.Error);
                    return null;
                }
            }
            else
            {
                Logger.Log($"Resource '{Name}' is a directory, not a file.", type: LogType.Error);
                return null;
            }
        }

        public string GetPath(string fileName)
        {
            if (PathInMemory != null)
            {
                try
                {
                    // Assuming the content is in-memory (e.g., a byte array)
                    return Path.Combine(PathInMemory, fileName);

                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to read content for resource '{Name}/{fileName}': {ex.Message}", type: LogType.Error);
                    return null;
                }
            }
            else
            {
                Logger.Log($"Resource '{Name}' is a directory, not a file.", type: LogType.Error);
                return null;
            }
        }

        public Resource GetSubResource(string subDirectoryName)
        {
            foreach (var subDirectory in SubDirectories)
            {
                if (subDirectory.Name == subDirectoryName)
                {
                    return subDirectory;
                }
            }

            // Sub-directory not found
            Logger.Log($"Sub-resource '{subDirectoryName}' not found in directory '{Name}'.", type: LogType.Error);
            return null;
        }


        public static Resource CreateFromDirectory(string directoryPath)
        {
            try
            {
                string directoryName = Path.GetFileName(directoryPath);
                List<Resource> subResources = new List<Resource>();

                List<string> files = new List<string>();

                // Add files as resources in the current directory
                foreach (var file in Directory.GetFiles(directoryPath))
                {
                    files.Add(file);
                }

                // Recursively add subdirectories as resources
                foreach (var subDirectory in Directory.GetDirectories(directoryPath))
                {
                    subResources.Add(CreateFromDirectory(subDirectory));
                }

                return new Resource(directoryName, subResources.ToArray(), directoryPath, files.ToArray());
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to create resource from directory path '{directoryPath}': {ex.Message}", type: LogType.Error);
                return null;
            }
        }


        public override string ToString()
        {
            return ToString(0);
        }

        private string ToString(int indentationLevel)
        {
            StringBuilder sb = new StringBuilder();
            string indentation = new string(' ', indentationLevel * 2);

            sb.AppendLine($"{indentation}Name: {Name}");
            sb.AppendLine($"{indentation}PathInMemory: {PathInMemory}");

            if (Resources != null && Resources.Count > 0)
            {
                sb.AppendLine($"{indentation}Resources:");
                foreach (var resource in Resources)
                {
                    sb.AppendLine($"{indentation}  - {resource}");
                }
            }

            if (SubDirectories != null && SubDirectories.Count > 0)
            {
                sb.AppendLine($"{indentation}SubDirectories:");
                foreach (var subDirectory in SubDirectories)
                {
                    sb.AppendLine(subDirectory.ToString(indentationLevel + 1));
                }
            }

            return sb.ToString();
        }


    }

    public static class ResourcesManager
    {
        private static Dictionary<string, Resource> resources = new Dictionary<string, Resource>();
        private static Dictionary<Type, IResourceLoader> resourceLoaders = new Dictionary<Type, IResourceLoader>();

        public static void Init()
        {
            RegisterResourceLoader(new ModelLoader());
        }

        public static ResourceLoader<T> HasLoader<T>()
        {
            if (resourceLoaders.ContainsKey(typeof(T)))
            {
                return (ResourceLoader<T>)resourceLoaders[typeof(T)];
            }
            return null;
        }

        public static void RegisterResourceLoader<T>(ResourceLoader<T> loader)
        {
            resourceLoaders.Add(typeof(T), loader);
        }

        public static void DumpResource(string name)
        {
            if (resources.ContainsKey(name))
            {
                Console.WriteLine(resources[name].ToString());
            }
            else
            {
                Logger.Log("Couldnt find the resource with name:" + name, type: LogType.Error);
            }
        }

        public static bool RegisterResource(string name, Resource resource)
        {
            try
            {
                if (!resources.ContainsKey(name))
                {
                    resources[name] = resource;
                    Logger.Log($"Resource '{name}' registered successfully.");
                    return true;
                }
                else
                {
                    Logger.Log($"Resource with the name '{name}' already exists. Registration failed.", type: LogType.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to register resource '{name}': {ex.Message}", type: LogType.Error);
                return false;
            }
        }

        public static bool RegisterResourceFromPath(string name, string path)
        {
            try
            {
                Resource resource = Resource.CreateFromDirectory(path);
                if (resource != null)
                {
                    return RegisterResource(name, resource);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to register resource '{name}' from path '{path}': {ex.Message}", type: LogType.Error);
                return false;
            }
        }

        public static Resource GetResource(string name)
        {
            if (resources.ContainsKey(name))
            {
                return resources[name];
            }
            else
            {
                Logger.Log($"Resource '{name}' not found.", type: LogType.Error);
                return null;
            }
        }
    }
}
