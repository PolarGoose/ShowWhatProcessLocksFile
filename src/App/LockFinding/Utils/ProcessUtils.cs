using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32.SafeHandles;
using ShowWhatProcessLocksFile.LockFinding.Interop;

namespace ShowWhatProcessLocksFile.LockFinding.Utils;

public static class ProcessUtils
{
    public static string? GetOwnerDomainAndUserName(SafeProcessHandle openedProcess)
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

    public static string? GetProcessExeFullName(SafeProcessHandle processHandle)
    {
        for (var capacity = 1024; ; capacity *= 2)
        {
            var buffer = new StringBuilder(capacity);
            int size = capacity;

            if (!WinApi.QueryFullProcessImageName(processHandle, 0, buffer, ref size))
            {
                int error = Marshal.GetLastWin32Error();
                if (error != WinApi.ERROR_INSUFFICIENT_BUFFER)
                {
                    return null;
                }
                continue;
            }

            return buffer.ToString(0, size);
        }
    }

    public static SafeProcessHandle? OpenProcessToDuplicateHandle(UIntPtr pid)
    {
        var p = WinApi.OpenProcess(
            WinApi.ProcessAccessRights.PROCESS_DUP_HANDLE |
            WinApi.ProcessAccessRights.PROCESS_QUERY_INFORMATION |
            WinApi.ProcessAccessRights.PROCESS_VM_READ,
            false, pid);
        return p.IsInvalid ? null : p;
    }

    public static IEnumerable<string> GetProcessModules(SafeProcessHandle openedProc)
    {
        for (var arraySize = 1024; ; arraySize *= 2)
        {
            var moduleHandles = new IntPtr[arraySize];
            if (!WinApi.EnumProcessModules(openedProc, moduleHandles, IntPtr.Size * moduleHandles.Length, out var bytesNeeded))
            {
                return Array.Empty<string>();
            }

            var numberOfModules = bytesNeeded / IntPtr.Size;

            if (arraySize < numberOfModules)
            {
                continue;
            }

            return GetModuleNames(openedProc, moduleHandles.Take(numberOfModules));
        }
    }

    private static IEnumerable<string> GetModuleNames(SafeProcessHandle openedProc, IEnumerable<IntPtr> moduleHandles)
    {
        foreach (var moduleHandle in moduleHandles)
        {
            var name = GetModuleName(openedProc, moduleHandle);
            if (name != null)
            {
                yield return name;
            }
        }
    }

    private static string? GetModuleName(SafeProcessHandle openedProc, IntPtr moduleHandle)
    {
        for (var moduleNameLength = 1024; ; moduleNameLength *= 2)
        {
            var sb = new StringBuilder(moduleNameLength);
            var resultLen = WinApi.GetModuleFileNameEx(openedProc, moduleHandle, sb, sb.Capacity);

            if (resultLen == 0)
            {
                return null;
            }

            if (resultLen == sb.Capacity)
            {
                continue;
            }

            return sb.ToString();
        }
    }

    public static ImageSource? GetIcon(string executableFullName)
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
