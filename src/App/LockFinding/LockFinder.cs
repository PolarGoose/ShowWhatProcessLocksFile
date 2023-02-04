using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ShowWhatProcessLocksFile.Utils;

namespace ShowWhatProcessLocksFile.LockFinding;

public class ProcessInfo
{
    public int Pid { get; }
    public string Name { get; }
    public string ExecutableFullName { get; }
    public string UserName { get; }
    public ImageSource Icon { get; }
    public IEnumerable<string> LockedFiles { get; }

    public ProcessInfo(int pid, string processName, string executableFullName, ImageSource icon, string userName,
        IEnumerable<string> lockedFiles)
    {
        Pid = pid;
        Name = processName;
        ExecutableFullName = executableFullName;
        Icon = icon;
        UserName = userName;
        LockedFiles = lockedFiles;
    }
}

public static class LockFinder
{
    public static async Task<IEnumerable<ProcessInfo>> FindWhatProcessesLockPath(string path)
    {
        var res = await HandleExe.GetProcessesLockingPath(path);
        return res.Select(ToProcessInfo).ToList();
    }

    private static ProcessInfo ToProcessInfo(LockingProcess p)
    {
        try
        {
            return new ProcessInfo(
                pid: p.pid,
                processName: Path.GetFileName(p.process_full_name),
                executableFullName: p.process_full_name,
                icon: TryGetIcon(p.process_full_name),
                userName: @$"{p.domain}\{p.user}",
                lockedFiles: p.locked_paths.OrderBy(x => x));
        }
        catch (Exception ex)
        {
            Log.Warn($"Failed to create a process info from: '{p.pid}' '{p.process_full_name}'. Exception:\n{ex}");
            return null;
        }
    }

    private static ImageSource TryGetIcon(string processExecutable)
    {
        // There are processes for which it is not possible to get an icon.
        // One of such processes is "System"

        try
        {
            using var ico = Icon.ExtractAssociatedIcon(processExecutable);
            var image = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            // We need to freeze the image, otherwise the GUI thread will not be able to use it if this function was called from another process
            image.Freeze();
            return image;
        }
        catch (Exception ex)
        {
            Log.Warn($"Failed to get an icon from executable '{processExecutable}'. Exception:\n{ex}");
            return null;
        }
    }
}
