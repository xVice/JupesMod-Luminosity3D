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


            Temp.ClearTemp();
            Resources.LoadBuiltinResourceTypes();
            Resources.CreateResourcesFolder();

            LD.StartEngine();
            
        }

        private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            
            Exception ex = (Exception)e.ExceptionObject;
            Logger.LogToFile("Unhandled Exception:");
            Logger.LogToFile(ex.ToString());
            MessageBox((IntPtr)0,$"Oops!\n JupesMod has crashed!\n\nDont worry though, your progress has been saved and logs were generated.\n\nException:\n{ex.Message}", "JupesMod Crash Handler", 0);
            LD.StopEngine();
        }

        private static void FirstExceptionHandler(object sender, FirstChanceExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            string errorMessage = $"A first chance, unhandled exception occurred:\n\n{ex.Message}\n\nStackTrace:\n{ex.StackTrace}";
            Logger.LogToFile(errorMessage);
        }
    }
}