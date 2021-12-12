using ShowWhatProcessLocksFile.Utils;
using System;
using System.IO;

namespace ShowWhatProcessLocksFile.LockFinding.HandleExe
{
    public static class Handle
    {
        private static readonly string HandleExeFullName = Path.Combine(AppContext.BaseDirectory, "handle.exe");

        public static string Execute(string fileFullName)
        {
            // Handle.exe doesn't work if a path contains "\" character at the end
            fileFullName = PathUtils.RemoveTrailingPathSeparator(fileFullName);

            return CommandLine.Execute(HandleExeFullName, $"-u -nobanner -accepteula \"{fileFullName}\"");
        }
    }
}
