using System;

namespace ShowWhatProcessLocksFile.Utils
{
    public static class PathUtils
    {
        public static string RemoveTrailingPathSeparator(string path)
        {
            return path.TrimEnd(new char[] { '\\' });
        }

        public static bool IsInsideFolder(string path, string folder)
        {
            return path.StartsWith($@"{RemoveTrailingPathSeparator(folder)}\", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
