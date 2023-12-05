using Luminosity3D.EntityComponentSystem;
using Luminosity3D.PKGLoader;
using Luminosity3D.Utils;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace JupesMod
{
    public class Program
    {
        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr h, string m, string c, int type);

        public static void Main(string[] args)
        {
            // Attach the exception handler
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);
            AppDomain.CurrentDomain.FirstChanceException += new EventHandler<FirstChanceExceptionEventArgs>(FirstExceptionHandler);
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(ApplicationCloseHandler);

            Temp.ClearTemp();
            Resources.LoadBuiltinResourceTypes();
            Resources.CreateResourcesFolder();
            //Net.StartServer("192.168.2.126", 42069);
            LD.StartEngine();
            
        }

        private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            
            Exception ex = (Exception)e.ExceptionObject;
            Logger.Log("Unhandled Exception:", true, LogType.Error);
            Logger.Log(ex.ToString(), true, LogType.Error);
            MessageBox((IntPtr)0,$"Oops!\nJupesMod has crashed!\n\nDont worry though, your progress has been saved and logs were generated.\n\nIn File:{ex.Source}\n\nIn Method:{ex.TargetSite.Name}\n\nException:\n{ex.Message}", "JupesMod Crash Handler", 0);
            
            
        }

        private static void FirstExceptionHandler(object sender, FirstChanceExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            string errorMessage = $"A first chance, unhandled exception occurred:\n\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}";
            Logger.Log(errorMessage, true, LogType.Error);
        }

        private static void ApplicationCloseHandler(object sender, EventArgs e)
        {
            Net.StopServer();
            PackageLoader.UnloadPaks();
            //Logger.ClearLogFile();
            Temp.ClearTemp();
            LD.StopEngine();
            Logger.Log("JMod is closing now, logs end after here. If they dont, something is really wrong.", true, LogType.Error);
        }
    }
}