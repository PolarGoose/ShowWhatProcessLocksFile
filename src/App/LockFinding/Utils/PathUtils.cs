using System;
using System.IO;

namespace ShowWhatProcessLocksFile.LockFinding.Utils;

public static class PathUtils
{
    public static string RemoveTrailingPathSeparator(string path)
    {
        path = Path.GetFullPath(path);
        return path.TrimEnd('\\');
    }

    public static bool IsInsideFolder(string path, string folder)
    {
        return path.StartsWith($@"{RemoveTrailingPathSeparator(folder)}\", StringComparison.InvariantCultureIgnoreCase);
    }
}
