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
        private Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();

        public RoslynCodeLoader()
        {
        }

        public Assembly LoadAndCompileCSharpFiles(string assemblyName, string[] filePaths)
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .Select(a => a.Location);

                Logger.Log($"Referenced {assemblies.Count()} dlls for {assemblyName}");

                var compilation = CSharpCompilation.Create(assemblyName)
                    .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                    .AddReferences(AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => !a.IsDynamic)
                        .Select(a =>
                        {
                            if (!string.IsNullOrEmpty(a.Location))
                            {
                                return MetadataReference.CreateFromFile(a.Location);
                            }
                            else
                            {
                                Logger.Log($"Skipping empty location for assembly: {a.FullName}");
                                return null;
                            }
                        })
                        .Where(reference => reference != null))

                    .AddSyntaxTrees(filePaths.Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path))));

                using (var ms = new MemoryStream())
                {
                    var result = compilation.Emit(ms);
                    if (!result.Success)
                    {
                        foreach (var diagnostic in result.Diagnostics)
                        {
                            Logger.Log(diagnostic.ToString());
                        }
                        return null;
                    }

                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());

                    // Store the loaded assembly in the dictionary for later access
                    loadedAssemblies[assemblyName] = assembly;

                    return assembly;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"An exception occurred: {ex}");
                return null;
            }
        }

        public Assembly GetLoadedAssembly(string assemblyName)
        {
            if (loadedAssemblies.ContainsKey(assemblyName))
            {
                return loadedAssemblies[assemblyName];
            }
            return null;
        }
    }
}
