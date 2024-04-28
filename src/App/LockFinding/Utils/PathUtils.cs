using System.IO;

namespace ShowWhatProcessLocksFile.LockFinding.Utils;

public static class PathUtils
{
    public static string ToCanonicalPath(string fileOrFolderPath)
    {
        var p = Path.GetFullPath(fileOrFolderPath);
        if (!p.EndsWith('\\') && Directory.Exists(p))
        {
            p += '\\';
        }

        return p;
    }
}
