using System.Diagnostics;

namespace ShowWhatProcessLocksFile.Utils
{
    internal static class Log
    {
        public static void Info(string msg)
        {
            Debug.WriteLine(msg, "Info");
        }

        public static void Warn(string msg)
        {
            Debug.WriteLine(msg, "Warn");
        }

        public static void Error(string msg)
        {
            Debug.WriteLine(msg, "Error");
        }
    }
}
