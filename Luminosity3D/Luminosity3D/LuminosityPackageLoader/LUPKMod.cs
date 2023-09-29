using System;
using System.Reflection;
using System.Runtime.Loader;
using Luminosity3D.Utils;
using Luminosity3DPAK;

namespace Luminosity3D.LuminosityPackageLoader
{
    public class LUPKMod
    {
        public Assembly Assembly { get; private set; }
        public PAK PAK { get; private set; }

        public LUPKMod(Assembly assembly, PAK pak)
        {
            Assembly = assembly;
            PAK = pak;
        }

        public void InvokeOnLoadMethod()
        {
            try
            {
                // Find and invoke the "OnLoad" method
                var type = Assembly.GetType($"{PAK.metadata.Namespace}.{PAK.metadata.Class}");
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
                        catch (TargetInvocationException ex)
                        {
                            // Handle exceptions thrown by the invoked method
                            Exception innerEx = ex.InnerException;
                            Logger.Log($"{PAK.metadata.Name} START ERROR LOG (During Method Invocation)");
                            Logger.Log($"An error occurred while invoking 'OnLoad' method for {PAK.metadata.Name}:");
                            Logger.Log(innerEx.ToString());
                            Logger.Log($"{PAK.metadata.Name} END ERROR LOG (During Method Invocation)");
                        }
                        catch (Exception ex)
                        {
                            // Handle other exceptions during method invocation
                            Logger.Log($"{PAK.metadata.Name} START ERROR LOG (During Method Invocation)");
                            Logger.Log($"An error occurred while invoking 'OnLoad' method for {PAK.metadata.Name}:");
                            Logger.Log(ex.ToString());
                            Logger.Log($"{PAK.metadata.Name} END ERROR LOG (During Method Invocation)");
                        }
                    }
                    else
                    {
                        Logger.Log($"Method 'OnLoad' not found in {PAK.metadata.Name}.PAK");
                    }

                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur outside of the method invocation
                Logger.Log($"{PAK.metadata.Name} START ERROR LOG (Outside of Method Invocation)");
                Logger.Log($"A fatal error occurred while starting: {PAK.metadata.Name}, exception below.");
                Logger.Log(ex.ToString());
                Logger.Log($"{PAK.metadata.Name} END ERROR LOG (Outside of Method Invocation)");
            }
        }
    }
}
