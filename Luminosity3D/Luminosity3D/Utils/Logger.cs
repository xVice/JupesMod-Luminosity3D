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
        public static List<Log> LogList { get; } = new List<Log>();
        public static string LogPath { get; set; } = "./logs";
        private static string LogFile = string.Empty;

        public static void SetupFolder()
        {
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }

            string logFileName = $"log-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.txt";
            LogFile = Path.Combine(LogPath, logFileName);
        }

        public static void Log(string message, bool logToFile = false, LogType type = LogType.Information, [CallerMemberName] string callerMemberName = "",
                                [CallerFilePath] string callerFilePath = "",
                                [CallerLineNumber] int callerLineNumber = 0)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string callingClassName = Path.GetFileNameWithoutExtension(callerFilePath);
            string label = $"[{callingClassName}.{callerMemberName}:{callerLineNumber}]";

            var log = new Log(type, label, timestamp, message);
            LogList.Add(log);

            if (LogFile != string.Empty)
            {
                File.AppendAllText(LogFile, $"{timestamp} | {label} - \n(\n{message}\n);\n\n");
            }

            Console.WriteLine($"{timestamp} | {label} - {message}");
        }
    }

}
