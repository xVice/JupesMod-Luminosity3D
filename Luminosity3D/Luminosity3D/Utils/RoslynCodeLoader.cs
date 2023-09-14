using Luminosity3DPAK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.Loader;
using Luminosity3D.LuminosityPackageLoader;

namespace Luminosity3D.Utils
{
    public class RoslynCodeLoader
    {

        public RoslynCodeLoader()
        {
        }

        //use config from individual mod later.
        public Assembly LoadAndCompileDlls(string mainAssemblyName,string[] dllPaths)
        {

            try
            {
                
                Assembly targetAssembly = null;
                var assemblies = AssemblyLoadContext.Default.Assemblies
                    .Where(a => !a.IsDynamic)
                    .Select(a => a.Location);

                Logger.Log($"Found {assemblies.Count()} loaded assemblies.");

                foreach (var dllPath in dllPaths)
                {
                    var dll = Path.GetFullPath(dllPath);
                    try
                    {
                        Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                        if (Path.GetFileNameWithoutExtension(dll) == mainAssemblyName || Path.GetFileName(dll) == mainAssemblyName)
                        {
                            targetAssembly = assembly;
                            Logger.Log($"Found host assembly: {dll}..");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error loading {dll}: {ex.Message}");
                        // Handle the error, e.g., decide whether to continue or abort loading.
                    }
                }
                return targetAssembly;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error while loading assemblies: {ex.Message}");
                // Handle the error at a higher level, e.g., application-level exception handling.
            }

            return null;
        }
    }
}
