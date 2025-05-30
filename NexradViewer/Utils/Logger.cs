using System;
using System.IO;

namespace NexradViewer.Utils
{
    /// <summary>
    /// Simple logging utility for the application
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logDirectory = "Logs";
        
        /// <summary>
        /// Logs a message to the console and file
        /// </summary>
        public static void Log(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logMessage = $"[{timestamp}] {message}";
                
                Console.WriteLine(logMessage);
                
                // Also write to log file
                WriteToLogFile(logMessage);
            }
            catch
            {
                // Fail silently if logging fails
            }
        }
        
        /// <summary>
        /// Logs an exception with context
        /// </summary>
        public static void LogException(Exception ex, string context = "")
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var contextInfo = string.IsNullOrEmpty(context) ? "" : $" [{context}]";
                var logMessage = $"[{timestamp}] ERROR{contextInfo}: {ex.Message}\nStack Trace: {ex.StackTrace}";
                
                Console.WriteLine(logMessage);
                
                // Also write to log file
                WriteToLogFile(logMessage);
            }
            catch
            {
                // Fail silently if logging fails
            }
        }
        
        /// <summary>
        /// Logs an error message
        /// </summary>
        public static void LogError(string message)
        {
            Log($"ERROR: {message}");
        }
        
        /// <summary>
        /// Logs a warning message
        /// </summary>
        public static void LogWarning(string message)
        {
            Log($"WARNING: {message}");
        }
        
        /// <summary>
        /// Logs an informational message
        /// </summary>
        public static void LogInfo(string message)
        {
            Log($"INFO: {message}");
        }
        
        /// <summary>
        /// Writes a message to the log file
        /// </summary>
        private static void WriteToLogFile(string message)
        {
            try
            {
                lock (_lock)
                {
                    // Ensure log directory exists
                    if (!Directory.Exists(_logDirectory))
                    {
                        Directory.CreateDirectory(_logDirectory);
                    }
                    
                    // Create log file name based on current date
                    var logFileName = $"app_{DateTime.Now:yyyyMMdd}.log";
                    var logFilePath = Path.Combine(_logDirectory, logFileName);
                    
                    // Append to log file
                    File.AppendAllText(logFilePath, message + Environment.NewLine);
                }
            }
            catch
            {
                // Fail silently if file writing fails
            }
        }
        
        /// <summary>
        /// Sets the log directory
        /// </summary>
        public static void SetLogDirectory(string directory)
        {
            _logDirectory = directory ?? "Logs";
        }
        
        /// <summary>
        /// Initializes the logger
        /// </summary>
        public static void Initialize()
        {
            // Ensure log directory exists
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
            
            Log("Logger initialized");
        }
        
        /// <summary>
        /// Gets the current log file path
        /// </summary>
        public static string GetLogFilePath()
        {
            try
            {
                var logFileName = $"app_{DateTime.Now:yyyyMMdd}.log";
                return Path.Combine(_logDirectory, logFileName);
            }
            catch
            {
                return Path.Combine("Logs", "app.log");
            }
        }
    }
}
