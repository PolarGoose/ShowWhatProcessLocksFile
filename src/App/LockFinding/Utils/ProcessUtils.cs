using System.Diagnostics;
using System.Drawing;
using System.Security.Principal;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32.SafeHandles;
using ShowWhatProcessLocksFile.LockFinding.Interop;

namespace ShowWhatProcessLocksFile.LockFinding.Utils;

public static class ProcessUtils
{
    public static string GetOwnerDomainAndUserNames(SafeProcessHandle openedProcess)
    {
        if (!WinApi.OpenProcessToken(openedProcess, WinApi.TokenAccessRights.TOKEN_QUERY, out var tokenHandle))
        {
            return null;
        }

        using (tokenHandle)
        {
            using var wi = new WindowsIdentity(tokenHandle.DangerousGetHandle());
            return wi.Name;
        }
    }

    public static Process GetProcess(int pid)
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

    public static string GetExecutablePath(Process process)
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

    public static ImageSource GetIcon(string executableFullName)
    {
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
