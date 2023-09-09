using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Utils
{
    public class Logger
    {
        public static void ClearLogFile()
        {
            File.Create("./log.txt");
        }

        public static void LogToFile(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Use reflection to get the calling class name
            StackTrace stackTrace = new StackTrace();
            StackFrame frame = stackTrace.GetFrame(1); // Get the caller's frame
            MethodBase method = frame.GetMethod();
            Type callingClass = method.DeclaringType;
            string callingClassName = callingClass != null ? callingClass.Name : "Unknown";

            string logMessage = $"[{timestamp}] [{callingClassName}] : {message}";

            File.WriteAllText("./log.txt", logMessage);
        }

        public static void Log(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Use reflection to get the calling class name
            StackTrace stackTrace = new StackTrace();
            StackFrame frame = stackTrace.GetFrame(1); // Get the caller's frame
            MethodBase method = frame.GetMethod();
            Type callingClass = method.DeclaringType;
            string callingClassName = callingClass != null ? callingClass.Name : "Unknown";

            string logMessage = $"[{timestamp}] [{callingClassName}] : {message}";

            if(Engine.Instance.Console != null)
            {
                Engine.Instance.Console.Log(logMessage);

            }

            Console.WriteLine(logMessage); // You can replace Console.WriteLine with your preferred logging mechanism.
        }
    }
}
