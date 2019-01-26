using System;
using System.IO;

namespace TestTask
{
    public static class Logger
    {
        public static void Log(string message)
        {
            File.AppendAllText($"{AppDomain.CurrentDomain.BaseDirectory}log.txt", message + Environment.NewLine);
        }
    }
}