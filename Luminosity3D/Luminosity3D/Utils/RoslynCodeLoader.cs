using System;
using System.IO;
using System.Reflection;
using Luminosity3DPAK;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.Loader;
using Luminosity3D.LuminosityPackageLoader;
using System.Reflection.Emit;

namespace Luminosity3D.Utils
{
    public class RoslynCodeLoader
    {


        public Assembly LoadAndCompileDlls(string mainAssemblyName, string[] dllPaths)
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
                        Assembly assembly = Assembly.LoadFile(dll);
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

