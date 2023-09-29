using Luminosity3D.Utils;

namespace JupesMod
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Attach the exception handler
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ExceptionHandler);

            var engine = LD.StartEngine();

            Console.ReadKey();

            Logger.Log("Closing..");

            // Make sure to stop the engine and perform other cleanup if needed.
            engine.StopEngine();
        }

        private static void ExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Logger.Log("Unhandled Exception:");
            Logger.Log(ex.ToString());
            // Optionally, you can log the exception here.
            // You might want to perform some cleanup or logging before exiting the application.
        }
    }
}