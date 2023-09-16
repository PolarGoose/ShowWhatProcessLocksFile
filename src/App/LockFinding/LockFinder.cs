using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Linq;
using ShowWhatProcessLocksFile.LockFinding.Utils;

namespace ShowWhatProcessLocksFile.LockFinding;

public readonly record struct ProcessInfo(
    int Pid,
    string ProcessName,
    string ProcessExecutableFullName,
    string UserNameWithDomain,
    ImageSource Icon,
    IEnumerable<string> LockedPath);

public static class LockFinder
{
    public static IEnumerable<ProcessInfo> FindWhatProcessesLockPath(string path)
    {
        var output = HandleExe.Execute(path);

        var fileHandles = output.Where(el => el.HandleType == "File");

        // File path we supply to Handle.exe is treated as "Show locks for all entities whose names start with this path".
        // It is not what we want, because if we ask to show what locks "C:\Program Files" folder,
        // it will also show processes which lock "C:\Program Files (x86)" folder.
        var handlesFromSpecifiedPath = fileHandles
            .Where(p =>
                string.Equals(p.LockedPath, path, StringComparison.OrdinalIgnoreCase) || PathUtils.IsInsideFolder(p.LockedPath, path));

        // There can be several records corresponding to the same process.
        var handlesBelongingToTheSameProcess = handlesFromSpecifiedPath.GroupBy(el => el.Pid);

        // There can be several records corresponding to the same file locked by the same process but with a different HandleAddress.
        var handlesBelongingToTheSameProcessWithoutDuplicates = handlesBelongingToTheSameProcess
            .Select(el => el.GroupBy(e => e.LockedPath));

        var processInfo = handlesBelongingToTheSameProcessWithoutDuplicates
            .Select(ToProcessInfo);

        var orderedProcessInfo = processInfo
            .Where(el => el != null)
            .Select(el => el.Value)
            .OrderBy(el => el.ProcessName);

        return orderedProcessInfo;
    }

    private static ProcessInfo? ToProcessInfo(IEnumerable<IGrouping<string, HandleInfo>> el)
    {
        var (processName, pid, _, userNameWithDomain, _, _) = el.First().First();

        var process = TryGetProcess(pid);
        if (process == null)
        {
            return null;
        }

        var processFullName = TryGetProcessFullName(process);
        var icon = TryGetIcon(processFullName);
        var lockedPath = el
            .Select(el => el.Key)
            .OrderBy(el => el);

        return new ProcessInfo(
            Pid: pid,
            ProcessName: processName,
            ProcessExecutableFullName: processFullName,
            UserNameWithDomain: userNameWithDomain,
            Icon: icon,
            LockedPath: lockedPath);
    }

    private static Process TryGetProcess(int pid)
    {
        try
        {
            return Process.GetProcessById(pid);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string TryGetProcessFullName(Process process)
    {
        try
        {
            return process.MainModule.FileName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static ImageSource TryGetIcon(string executableFullName)
    {
        if (executableFullName == null)
        {
            return null;
        }

        try
        {
            using var ico = Icon.ExtractAssociatedIcon(executableFullName);
            var image = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            // We need to freeze the image, otherwise the GUI thread will not be able to use it if this function was called from another process
            image.Freeze();
            return image;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
