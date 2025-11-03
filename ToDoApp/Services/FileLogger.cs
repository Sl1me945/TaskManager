using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToDoApp.Interfaces;

namespace ToDoApp.Services
{
    public class FileLogger : ILogger
    {
        private readonly string _logPath;

        public FileLogger(string logPath = "data/logs/app.log")
        {
            _logPath = logPath;
            Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);
        }

        public void Info(string message) => Write("INFO", message);
        public void Warn(string message) => Write("WARN", message);
        public void Error(string message, Exception? ex = null) =>
            Write("ERROR", $"{message} {ex?.Message}");

        private void Write(string level, string message) 
        {
            string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            Console.WriteLine(logLine);
            File.AppendAllText(_logPath, logLine + Environment.NewLine);
        }
    }
}
