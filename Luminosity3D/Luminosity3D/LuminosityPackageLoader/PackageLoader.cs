using Luminosity3D.LuminosityPackageLoader;
using Luminosity3D.Utils;
using Luminosity3DPAK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;

namespace Luminosity3D.PKGLoader
{
    public class PackageLoader
    {
        private List<LUPKMod> LoadedMods = new List<LUPKMod>();
        private readonly object lockObject = new object();

        public const string LUPKDir = "./lupk";
        public const string LUPKAutoLoadDir = "/autoload";
        public const string LoadedFolder = "/loaded";
        public const string LUPKLoadedDir = LUPKDir + LoadedFolder;

        public PackageLoader()
        {
            if (!Directory.Exists(LUPKDir))
            {
                Directory.CreateDirectory(LUPKDir);
                Directory.CreateDirectory(LUPKLoadedDir);
            }
            else if (!Directory.Exists(LUPKLoadedDir))
            {
                Directory.CreateDirectory(LUPKLoadedDir);
            }
        }

        public bool IsModLoaded(string name)
        {
            lock (lockObject)
            {
                return LoadedMods.Any(x => x.PAK.metadata.Name == name);
            }
        }

        public void LoadPaks()
        {
            if (Directory.Exists(LUPKDir))
            {
                var paks = Directory.GetFiles(LUPKDir + LUPKAutoLoadDir, "*.lupk");
                if (paks.Length != 0)
                {
                    Parallel.ForEach(paks, pakdir =>
                    {
                        string pakpath = Path.GetFileNameWithoutExtension(pakdir);
                        LoadPackageFromAutoLoad(pakpath); // Pass the filename to LoadPackage
                    });
                }
            }
        }

        public void LoadPaks(string[] pakNames)
        {
            Parallel.ForEach(pakNames, pakname =>
            {
                if (PakExist(pakname))
                {
                    if (!IsModLoaded(pakname))
                    {
                        LoadPackage(pakname);
                    }
                    else
                    {
                        Logger.LogToFile($"{pakname} is already loaded..");
                    }
                }
                else
                {
                    Logger.LogToFile($"{pakname} was not found in the lupk folder..");
                }
            });
        }

        public bool PakExist(string name)
        {
            string path = LUPKDir + $"/{name}.lupk";
            Logger.LogToFile(path);
            return File.Exists(path);
        }

        public void PakFolder(string path, string dest)
        {
            PAK pak = new PAK(path);
            pak.PackageToPAK(dest);
        }

        public LUPKMod LoadPackage(string name)
        {
            var timer = new Stopwatch();
            timer.Start();
            Logger.LogToFile($"Loading: {name}..");

            try
            {
                var lupkg = UnpackPKG(name);

                var csFiles = Directory.GetFiles(lupkg.UnpackedPath, "*.dll", SearchOption.AllDirectories);
                Logger.LogToFile($"Found {csFiles.Count()} dll files in {name}, loading assemblies..");

                // Use the RoslynCodeLoader to load and compile C# files
                var codeLoader = new RoslynCodeLoader();
                var compiledAssembly = codeLoader.LoadAndCompileDlls(lupkg.metadata.Namespace, csFiles);

                if (compiledAssembly != null)
                {
                    var mod = new LUPKMod(compiledAssembly, lupkg);
                    mod.InvokeOnLoadMethod();
                    lock (lockObject)
                    {
                        LoadedMods.Add(mod);
                    }
                    timer.Stop();
                    Logger.LogToFile($"Successfully loaded {name} in {timer.ElapsedMilliseconds}ms!");
                    return mod;
                }

                timer.Stop();
                Logger.LogToFile($"Failed to load {name}.");
            }
            catch (Exception ex)
            {
                Logger.LogToFile($"Error loading {name}: {ex.Message}");
            }

            return null;
        }

        private void LoadDependencies(string name)
        {
            var lupk = new PAK($"{LUPKDir}/{name}.lupk");
            var meta = lupk.ExtractMetadata();

            LoadPaks(meta.Dependencies);
        }

        public LUPKMod LoadPackageFromAutoLoad(string name)
        {
            var timer = new Stopwatch();
            timer.Start();
            Logger.LogToFile($"Loading: {name}..");

            try
            {
                var lupkg = UnpackPKGFromAutoLoad(name);

                var csFiles = Directory.GetFiles(lupkg.UnpackedPath, "*.dll", SearchOption.AllDirectories);
                Logger.LogToFile($"Found {csFiles.Count()} dll files in {name}, loading assemblies..");

                // Use the RoslynCodeLoader to load and compile C# files
                var codeLoader = new RoslynCodeLoader();
                var compiledAssembly = codeLoader.LoadAndCompileDlls(lupkg.metadata.Namespace, csFiles);

                if (compiledAssembly != null)
                {
                    var mod = new LUPKMod(compiledAssembly, lupkg);
                    mod.InvokeOnLoadMethod();
                    lock (lockObject)
                    {
                        LoadedMods.Add(mod);
                    }
                    Logger.LogToFile($"OnLoad called for {name}!");
                    return mod;
                }

                timer.Stop();
                Logger.LogToFile($"Failed to load {name}.");
            }
            catch (Exception ex)
            {
                Logger.LogToFile($"Error loading {name}: {ex.Message}");
            }

            return null;
        }

        public void UnloadPaks()
        {
            Logger.LogToFile("Unloading loaded lupk mods.");
            if (Directory.Exists(LUPKLoadedDir))
            {
                Directory.Delete(LUPKLoadedDir, true);
                Directory.CreateDirectory(LUPKLoadedDir);
            }
        }

        private PAK UnpackPKGFromAutoLoad(string name)
        {
            Logger.LogToFile($"Unpacking {name}..");
            var lupkg = UnpackPKGFromPath(LUPKDir + LUPKAutoLoadDir + $"/{name}.lupk");
            return lupkg;
        }

        private PAK UnpackPKG(string name)
        {
            Logger.LogToFile($"Unpacking {name}..");
            var lupkg = UnpackPKGFromPath(LUPKDir + $"/{name}.lupk");
            return lupkg;
        }

        private PAK UnpackPKGFromPath(string path)
        {
            var lupk = new PAK(path);
            lupk.Unpack(LUPKLoadedDir + $"/{lupk.PakName}");
            return lupk;
        }
    }
}
