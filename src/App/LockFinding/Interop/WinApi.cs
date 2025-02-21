using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Text;

namespace ShowWhatProcessLocksFile.LockFinding.Interop;

public class WinApi
{
    [Flags]
    private enum StandardAccessRights : long
    {
        DELETE = 0x00010000L,
        READ_CONTROL = 0x00020000L,
        WRITE_DAC = 0x00040000L,
        WRITE_OWNER = 0x00080000L,
        SYNCHRONIZE = 0x00100000L,
        STANDARD_RIGHTS_REQUIRED = 0x000F0000L
    }

    [Flags]
    public enum ProcessAccessRights : ulong
    {
        PROCESS_ALL_ACCESS = StandardAccessRights.STANDARD_RIGHTS_REQUIRED | StandardAccessRights.SYNCHRONIZE | 0xFFFF,
        PROCESS_TERMINATE = 0x0001,
        PROCESS_CREATE_THREAD = 0x0002,
        PROCESS_SET_SESSIONID = 0x0004,
        PROCESS_VM_OPERATION = 0x0008,
        PROCESS_VM_READ = 0x0010,
        PROCESS_VM_WRITE = 0x0020,
        PROCESS_DUP_HANDLE = 0x0040,
        PROCESS_CREATE_PROCESS = 0x0080,
        PROCESS_SET_QUOTA = 0x0100,
        PROCESS_SET_INFORMATION = 0x0200,
        PROCESS_QUERY_INFORMATION = 0x0400,
        PROCESS_SUSPEND_RESUME = 0x0800,
        PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
        PROCESS_SET_LIMITED_INFORMATION = 0x2000
    }

    [Flags]
    private enum DuplicateObjectOptions : uint
    {
        DUPLICATE_CLOSE_SOURCE = 0x00000001,
        DUPLICATE_SAME_ACCESS = 0x00000002
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern SafeProcessHandle OpenProcess(
        ProcessAccessRights dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        UIntPtr dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DuplicateHandle(
        SafeProcessHandle hSourceProcessHandle,
        IntPtr hSourceHandle,
        SafeProcessHandle hTargetProcessHandle,
        out SafeFileHandle lpTargetHandle,
        uint dwDesiredAccess,
        [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
        uint dwOptions);

    public static SafeFileHandle DuplicateHandle(
        SafeProcessHandle currentProcess,
        SafeProcessHandle handleOwnerProcess,
        NtDll.SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX handle)
    {
        var res = DuplicateHandle(
            handleOwnerProcess,
            handle.HandleValue,
            currentProcess,
            out var dupHandle,
            0,
            false,
            0);
        return res ? dupHandle : new SafeFileHandle(IntPtr.Zero, true);
    }

    [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "GetCurrentProcess")]
    private static extern IntPtr GetCurrentProcessPrivate();

    public static SafeProcessHandle GetCurrentProcess()
    {
        var handle = GetCurrentProcessPrivate();
        return new SafeProcessHandle(handle, false);
    }

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    private static extern int GetFinalPathNameByHandleW(SafeFileHandle hFile, [Out] StringBuilder filePathBuffer, int filePathBufferSize, int flags);

    public static string? GetFinalPathNameByHandle(SafeFileHandle hFile)
    {
        var buf = new StringBuilder();
        var result = GetFinalPathNameByHandleW(hFile, buf, buf.Capacity, 0);
        if (result == 0)
        {
            return null;
        }

        buf.EnsureCapacity(result);
        result = GetFinalPathNameByHandleW(hFile, buf, buf.Capacity, 0);
        if (result == 0)
        {
            return null;
        }

        var str = buf.ToString();
        return str.StartsWith(@"\\?\") ? str[4..] : str;
    }

    [Flags]
    public enum TokenAccessRights : ulong
    {
        TOKEN_QUERY = 0x0008
    }

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool OpenProcessToken(SafeProcessHandle processHandle, TokenAccessRights desiredAccess, out SafeFileHandle tokenHandle);

    public enum FileType : uint
    {
        FILE_TYPE_UNKNOWN = 0x0000,
        FILE_TYPE_DISK = 0x0001,
        FILE_TYPE_CHAR = 0x0002,
        FILE_TYPE_PIPE = 0x0003,
        FILE_TYPE_REMOTE = 0x8000
    }

    [DllImport("Kernel32.dll", SetLastError = true)]
    public static extern FileType GetFileType(SafeFileHandle hFile);

    [Flags]
    public enum FileDesiredAccess : uint
    {
        None = 0,
        Synchronize = 0x00100000,
        Delete = 0x00010000,
        GenericRead = 0x80000000,
        GenericWrite = 0x40000000,
        FileReadAttributes = 0x0080,
        FileWriteAttributes = 0x00100
    }

    [Flags]
    public enum FileFlagsAndAttributes : uint
    {
        None = 0,
        FileAttributeArchive = 0x20,
        FileAttributeEncrypted = 0x4000,
        FileAttributeHidden = 0x2,
        FileAttributeNormal = 0x80,
        FileAttributeOffline = 0x1000,
        FileAttributeReadOnly = 0x1,
        FileAttributeSystem = 0x4,
        FileAttributeTemporary = 0x100,
        FileFlagBackupSemantics = 0x02000000,
        FileFlagDeleteOnClose = 0x04000000,
        FileFlagNoBuffering = 0x20000000,
        FileFlagOpenNoRecall = 0x00100000,
        FileFlagOpenReparsePoint = 0x00200000,
        FileFlagOverlapped = 0x40000000,
        FileFlagPosixSemantics = 0x01000000,
        FileFlagRandomAccess = 0x10000000,
        FileFlagSessionAware = 0x00800000,
        FileFlagSequentialScan = 0x08000000,
        FileFlagWriteThrough = 0x80000000,
        SecurityAnonymous = 0x00100000
    }

    public const int ERROR_INSUFFICIENT_BUFFER = 122;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern bool QueryFullProcessImageName(
        SafeProcessHandle hProcess,
        int dwFlags,
        StringBuilder? lpExeName,
        ref int lpdwSize);

    [DllImport("psapi.dll", SetLastError = true)]
    public static extern bool EnumProcessModules(SafeProcessHandle hProcess, IntPtr[] lphModule, int cb, out int lpcbNeeded);

    [DllImport("psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern uint GetModuleFileNameEx(SafeProcessHandle hProcess, IntPtr hModule, [Out] StringBuilder lpFilename, int nSize);
}
