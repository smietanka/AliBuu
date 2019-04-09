using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliBuu.Logger
{
    enum LogTypes
    {
        INFO,
        ERROR,
        WARNING
    }
    public class Log
    {
        public static void Info(string path, string[] tags, Dictionary<string, object> dict, string message, params string[] keys)
        {
            Console.WriteLine(ComposeLog(LogTypes.INFO, path, tags, dict, message, keys));
            Trace.WriteLine(ComposeLog(LogTypes.INFO, path, tags, dict, message, keys));
        }

        public static void Exception(string path, string[] tags, Dictionary<string, object> dict, string message, Exception e, params string[] keys)
        {
            Console.WriteLine(ComposeLog(LogTypes.ERROR, path, tags, dict, $"{message}. {e.Message}", keys));
            Trace.WriteLine(ComposeLog(LogTypes.ERROR, path, tags, dict, $"{message}. {e.Message}", keys));
        }


        private static string ComposeLog(LogTypes log, string path, string[] tags, Dictionary<string, object> dict, string message, params string[] keys)
        {
            var result = new StringBuilder();

            result.Append($"[{DateTime.Now.ToShortTimeString()} - {log.ToString()}] [{path}; {string.Join(", ", tags)}]\n");
            if(dict != null && dict.Any())
            {
                foreach(var value in dict)
                {
                    result.AppendLine($"{value.Key} - {value.Value}");
                }
            }
            result.AppendLine($"{string.Format(message, keys)}");
            return result.ToString();
        }
        
    }
}
