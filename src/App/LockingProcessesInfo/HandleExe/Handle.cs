using System;
using System.IO;

namespace ShowWhatProcessLocksFile.LockingProcessesInfo.HandleExe
{
    public static class Handle
    {
        private static readonly string FullName = Path.Combine(AppContext.BaseDirectory, "handle.exe");

        public static string Execute(string fileFullName)
        {
            // Handle.exe doesn't work if a path contains "\" character at the end
            if (fileFullName.EndsWith(@"\"))
            {
                fileFullName = fileFullName.Remove(fileFullName.Length - 1);
            }

            return CommandLine.Execute(FullName, $"-u -nobanner -accepteula \"{fileFullName}\"");
        }
    }
}
