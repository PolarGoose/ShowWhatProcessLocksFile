using System.Runtime.InteropServices;
using System.Text;

namespace ShowWhatProcessLocksFile.LockFinding.Utils;

internal static class MappedDriveResolver
{
    [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int WNetGetConnection(string lpLocalName, StringBuilder lpRemoteName, ref int lpnLength);

    private const int NO_ERROR = 0;
    private const int ERROR_MORE_DATA = 234;

    // Converts a path like "Z:\folder\file.txt" to its UNC path like "\\server\share\folder\file.txt" if Z: is a mapped network drive.
    public static string ResolveToUncPathIfNetworkDrivePath(string path)
    {
        var root = Path.GetPathRoot(path);
        string drive = root!.Substring(0, 2); // "Z:"

        string? uncRoot = GetUncRootForDrive(drive);
        if (uncRoot is null)
            return path;

        string relativePart = path.Substring(2); // Remove the drive letter and colon

        return uncRoot + relativePart;
    }

    private static string? GetUncRootForDrive(string drive)
    {
        var remoteName = new StringBuilder(1000);
        int capacity = remoteName.Capacity;

        var result = WNetGetConnection(drive, remoteName, ref capacity);
        if (result == NO_ERROR)
            return remoteName.ToString();
        if (result == ERROR_MORE_DATA)
        {
            remoteName = new StringBuilder(capacity);
            result = WNetGetConnection(drive, remoteName, ref capacity);
            if (result == NO_ERROR)
                return remoteName.ToString();
        }

        return null;
    }
}

internal sealed class CanonicalPath
{
    public string Path { get; }
    public bool IsDirectory { get; }

    public CanonicalPath(string path)
    {
        Path = System.IO.Path.GetFullPath(path).Replace('/', '\\');
        if (!Path.StartsWith(@"\\"))
        {
            Path = MappedDriveResolver.ResolveToUncPathIfNetworkDrivePath(Path);
        }

        bool existedAsDir = System.IO.Directory.Exists(Path);

        // A root path might be like "C:\" directly from GetFullPath. 
        if (existedAsDir && !Path.EndsWith("\\"))
        {
            Path += "\\";
        }
        
        IsDirectory = existedAsDir;
    }
}

internal static class PathUtils
{
    public static string AddTrailingSeparatorIfItIsAFolder(string fileOrFolderPath)
    {
        return System.IO.Directory.Exists(fileOrFolderPath) && !fileOrFolderPath.EndsWith('\\') ? fileOrFolderPath + '\\' : fileOrFolderPath;
    }
}
