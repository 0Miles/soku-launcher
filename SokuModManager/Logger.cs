using System;
using System.IO;

namespace SokuModManager
{
    public class Logger
    {
        private static readonly string LogFilePath = "log.txt";

        static Logger()
        {
            if (!File.Exists(LogFilePath))
            {
                File.Create(LogFilePath).Close();
            }
        }

        public static void LogInformation(string message)
        {
            LogMessage($"[Information] {DateTime.Now}: {message}");
        }

        public static void LogError(string message, Exception ex)
        {
            LogMessage($"[Error] {DateTime.Now}: {message}\nException: {ex}");
        }

        private static void LogMessage(string logEntry)
        {
            try
            {
                Console.WriteLine(logEntry);
                using (StreamWriter writer = File.AppendText(LogFilePath))
                {

                    writer.WriteLine(logEntry);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to write to log file. Error: {e}");
            }
        }
    }
}
