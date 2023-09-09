using System;
using System.Reflection;
using Luminosity3D.Utils;
using Luminosity3DPAK;

namespace Luminosity3D.LuminosityPackageLoader
{
    public class LUPKMod
    {
        public string Name { get; private set; }
        public Assembly Assembly { get; private set; }
        public PAK PAK { get; private set; }

        public LUPKMod(string name, Assembly assembly, PAK pak)
        {
            Name = name;
            Assembly = assembly;
            PAK = pak;
        }

        public void InvokeOnLoadMethod()
        {
            try
            {
                var type = Assembly.GetType($"{Name}.PAK");
                if (type != null)
                {
                    MethodInfo method = type.GetMethod("OnLoad", BindingFlags.Static | BindingFlags.Public);
                    if (method != null)
                    {
                        try
                        {
                            var result = method.Invoke(null, null);
                            // Handle the result if needed
                        }
                        catch (Exception ex)
                        {
                            // Handle any exceptions that occur during the method invocation
                            Logger.LogToFile($"{Name} START ERROR LOG (During Method Invocation)");
                            Logger.LogToFile($"An error occurred while invoking 'OnLoad' method for {Name}:");
                            Logger.LogToFile(ex.ToString());
                            Logger.LogToFile($"{Name} END ERROR LOG (During Method Invocation)");
                        }
                    }
                    else
                    {
                        Logger.LogToFile($"Method 'OnLoad' not found in {Name}.PAK");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur outside of the method invocation
                Logger.LogToFile($"{Name} START ERROR LOG (Outside of Method Invocation)");
                Logger.LogToFile($"A fatal error occurred while starting: {Name}, exception below.");
                Logger.LogToFile(ex.ToString());
                Logger.LogToFile($"{Name} END ERROR LOG (Outside of Method Invocation)");
            }
        }

    }
}
