using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32.SafeHandles;
using ShowWhatProcessLocksFile.LockFinding.Interop;

namespace ShowWhatProcessLocksFile.LockFinding.Utils;

internal static class ProcessUtils
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
                return [];
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
                // Some module names are prefixed with "\\?\". For example: "\\?\C:\Windows\System32\ntdll.dll".
                if (name.StartsWith(@"\\?"))
                {
                    name = name.Substring(4);
                }
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

    public static byte[]? GetIcon(string executableFullName)
    {
        try
        {
            using var ico = Icon.ExtractAssociatedIcon(executableFullName);
            if (ico == null)
            {
                return null;
            }
            using var bitmap = ico.ToBitmap();
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            return stream.ToArray();
        }
        catch (Exception)
        {
            return null;
        }
    }
}
