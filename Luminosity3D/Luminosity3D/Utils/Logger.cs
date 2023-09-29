using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Luminosity3D.Utils
{
    public class Logger
    {
        public static void ClearLogFile()
        {
            File.WriteAllText("./log.txt", string.Empty);
        }


        public static void Log(string message, [CallerMemberName] string callerMemberName = "",
                            [CallerFilePath] string callerFilePath = "",
                            [CallerLineNumber] int callerLineNumber = 0)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string callingClassName = Path.GetFileNameWithoutExtension(callerFilePath);
            string logMessage = $"[{timestamp}] [{callingClassName}.{callerMemberName}:{callerLineNumber}] : {message}";

            using (StreamWriter sw = File.AppendText("./log.txt"))
            {
                sw.WriteLine(logMessage);

            }
            if (Engine.Instance.Console != null)
            {
                Engine.Instance.Console.Log(logMessage);

            }

            Console.WriteLine(logMessage);
        }

    }
}
