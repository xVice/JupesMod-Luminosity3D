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
    public enum LogType
    {
        Information,
        Debug,
        Warning,
        Error
    }

    public struct Log
    {
        public LogType Type = LogType.Information;
        public string Lable = string.Empty;
        public string TimeStamp = string.Empty;
        public string Content = string.Empty;

        public Log(LogType type, string lable, string timeStamp, string content)
        {
            Type = type;
            Lable = lable;
            TimeStamp = timeStamp;
            Content = content;
        }
    }

    public class Logger
    {
        public static List<Log> logList = new List<Log>();

        public static void ClearLogFile()
        {
            File.WriteAllText("./log.txt", string.Empty);
        }




        public static void Log(string message,LogType type = LogType.Information ,[CallerMemberName] string callerMemberName = "",
                            [CallerFilePath] string callerFilePath = "",
                            [CallerLineNumber] int callerLineNumber = 0)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string callingClassName = Path.GetFileNameWithoutExtension(callerFilePath);
            string lable = $"[{callingClassName}.{callerMemberName}:{callerLineNumber}]";

            if (Engine.Console != null)
            {
                var log = new Log(type, lable, timestamp, message);
                logList.Add(log);

            }

            Console.WriteLine(timestamp + " " + lable + ":" + message);
        }

        public static void LogToFile(string message, bool fileExclusive = true, LogType type = LogType.Information,[CallerMemberName] string callerMemberName = "",
                    [CallerFilePath] string callerFilePath = "",
                    [CallerLineNumber] int callerLineNumber = 0)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string callingClassName = Path.GetFileNameWithoutExtension(callerFilePath);
            string lable = $"[{callingClassName}.{callerMemberName}:{callerLineNumber}]";
            var msg = timestamp + " " + lable + ":" + message;
            using (StreamWriter sw = File.AppendText("./log.txt"))
            {
                sw.WriteLine(msg);

            }

            if (fileExclusive == false)
            {
                var log = new Log(type, lable, timestamp, message);
                logList.Add(log);
     

            }
            Console.WriteLine(msg);

        }

    }
}
