using Luminosity3D.LuminosityPackageLoader;
using Luminosity3D.Utils;
using Luminosity3DPAK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace Luminosity3D.PKGLoader
{
    public class PackageLoader
    {
        private static Engine Engine { get => Engine.Instance; }
        public static PackageLoader Instance { get => Engine.PackageLoader; }

        private List<LUPKMod> LoadedMods = new List<LUPKMod>();

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
            return LoadedMods.Where(x => x.PAK.metadata.Name == name).Any();
        }

        public void LoadPaks()
        {
            if (Directory.Exists(LUPKDir))
            {
                var paks = Directory.GetFiles(LUPKDir + LUPKAutoLoadDir, "*.lupk");
                if (paks.Length != 0)
                {
                    foreach (var pakdir in paks)
                    {

                        string pakpath = Path.GetFileNameWithoutExtension(pakdir);

                        LoadPackageFromAutoLoad(pakpath); // Pass the filename to LoadPackage

                    }
                }
            }
        }
    
        public void LoadPaks(string[] pakNames)
        {
            foreach(var pakname in pakNames)
            {
                if (PakExist(pakname))
                {
                    if (!IsModLoaded(pakname))
                    {
                        LoadPackage(pakname);
                    }
                    else
                    {
                        Logger.Log($"{pakname} is allready loaded..");
                    }
                }
                else
                {
                    Logger.Log($"{pakname} is was not found it the lupk folder..");
                }
            }
        }

        public bool PakExist(string name)
        {
            string path = LUPKDir + $"/{name}.lupk";
            Logger.Log(path);
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
            Logger.Log($"Loading: {name}..");
            //LoadDependencies(name);
            var lupkg = UnpackPKG(name);

            var csFiles = Directory.GetFiles(lupkg.UnpackedPath, "*.dll", SearchOption.AllDirectories);
            Logger.Log($"Found {csFiles.Count()} dll files in {name}, loading assemblys..");


            // Use the RoslynCodeLoader to load and compile C# files
            var codeLoader = new RoslynCodeLoader();
            var compiledAssembly = codeLoader.LoadAndCompileDlls(lupkg.metadata.Namespace, csFiles);

            if (compiledAssembly != null)
            {
                var mod = new LUPKMod(compiledAssembly, lupkg);
                mod.InvokeOnLoadMethod();
                LoadedMods.Add(mod);
                timer.Stop();
                Logger.Log($"Successfully loaded {name} in {timer.ElapsedMilliseconds}ms!");
                return mod;
            }

            timer.Stop();
            Logger.Log($"Successfully loaded {name} in {timer.ElapsedMilliseconds}ms!");

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
            Logger.Log($"Loading: {name}..");

            //LoadDependencies(name);

            var lupkg = UnpackPKGFromAutoLoad(name);

            var csFiles = Directory.GetFiles(lupkg.UnpackedPath, "*.dll", SearchOption.AllDirectories);
            Logger.Log($"Found {csFiles.Count()} dll files in {name}, loading assemblys..");

            // Use the RoslynCodeLoader to load and compile C# files
            var codeLoader = new RoslynCodeLoader();
            var compiledAssembly = codeLoader.LoadAndCompileDlls(lupkg.metadata.Namespace, csFiles);

            if (compiledAssembly != null)
            {
                var mod = new LUPKMod(compiledAssembly, lupkg);
                mod.InvokeOnLoadMethod();
                LoadedMods.Add(mod);
                Logger.Log($"OnLoad called for {name}!");
                return mod;
            }

            timer.Stop();
            Logger.Log($"Successfully loaded {name} in {timer.ElapsedMilliseconds}ms!");
            return null;
        }

        public void UnloadPaks()
        {
            Logger.Log("Unloading loaded lupk mods.");
            if (Directory.Exists(LUPKLoadedDir))
            {
                Directory.Delete(LUPKLoadedDir, true);
                
                Directory.CreateDirectory(LUPKLoadedDir);
            }
        }

        private PAK UnpackPKGFromAutoLoad(string name)
        {
            Logger.Log($"Unpacking {name}..");
            var lupkg = UnpackPKGFromPath(LUPKDir + LUPKAutoLoadDir + $"/{name}.lupk");
            return lupkg;
        }

        private PAK UnpackPKG(string name)
        {
            Logger.Log($"Unpacking {name}..");
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
