using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace ShowWhatProcessLocksFile.LockFinding.Interop;

public static class NtDll
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX
    {
        // Pointer to the handle in the kernel virtual address space.
        public IntPtr Object;

        // PID that owns the handle
        public UIntPtr UniqueProcessId;

        // Handle value in the process that owns the handle.
        public IntPtr HandleValue;

        // Access rights associated with the handle.
        // Bit mask consisting of the fields defined in the winnt.h
        // For example: READ_CONTROL|DELETE|SYNCHRONIZE|WRITE_DAC|WRITE_OWNER|EVENT_ALL_ACCESS
        // The exact information that this field contain depends on the type of the handle.
        public uint GrantedAccess;

        // This filed is reserved for debugging purposes
        // For instance, it can store an index to a stack trace that was captured when the handle was created.
        public ushort CreatorBackTraceIndex;

        // Type of object a handle refers to.
        // For instance: file, thread, or process
        public ushort ObjectTypeIndex;

        // Bit mask that provides additional information about the handle.
        // For example: OBJ_INHERIT, OBJ_EXCLUSIVE
        // The attributes are defined in the winternl.h
        public uint HandleAttributes;

        public uint Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SYSTEM_HANDLE_INFORMATION_EX
    {
        public IntPtr NumberOfHandles;
        public IntPtr Reserved;
        public SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX Handles; // Single element
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct UNICODE_STRING
    {
        public ushort Length;
        public ushort MaximumLength;
        public IntPtr Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GENERIC_MAPPING
    {
        public int GenericRead;
        public int GenericWrite;
        public int GenericExecute;
        public int GenericAll;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct OBJECT_TYPE_INFORMATION
    {
        public UNICODE_STRING Name;
        public uint TotalNumberOfObjects;
        public uint TotalNumberOfHandles;
        public uint TotalPagedPoolUsage;
        public uint TotalNonPagedPoolUsage;
        public uint TotalNamePoolUsage;
        public uint TotalHandleTableUsage;
        public uint HighWaterNumberOfObjects;
        public uint HighWaterNumberOfHandles;
        public uint HighWaterPagedPoolUsage;
        public uint HighWaterNonPagedPoolUsage;
        public uint HighWaterNamePoolUsage;
        public uint HighWaterHandleTableUsage;
        public uint InvalidAttributes;
        public GENERIC_MAPPING GenericMapping;
        public uint ValidAccess;
        public byte SecurityRequired;
        public byte MaintainHandleCount;
        public ushort MaintainTypeList;
        public int PoolType;
        public int PagedPoolUsage;
        public int NonPagedPoolUsage;
    }

    private enum OBJECT_INFORMATION_CLASS
    {
        ObjectBasicInformation = 0,
        ObjectNameInformation = 1,
        ObjectTypeInformation = 2,
        ObjectAllTypesInformation = 3,
        ObjectHandleInformation = 4
    }

    private enum SYSTEM_INFORMATION_CLASS
    {
        SystemExtendedHandleInformation = 64
    }

    [Flags]
    public enum DuplicateObjectOptions
    {
        None = 0,
        CloseSource = 1,
        SameAccess = 2,
        SameAttributes = 4,
        NoRightsUpgrade = 8
    }

    private const uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;
    private const int NT_SUCCESS = 0;

    [DllImport("ntdll.dll")]
    private static extern uint NtQuerySystemInformation(
        SYSTEM_INFORMATION_CLASS systemInformationClass,
        IntPtr systemInformation,
        int systemInformationLength,
        out int returnLength);

    [DllImport("ntdll.dll", EntryPoint = "NtQueryObject")]
    private static extern int NtQueryObject(
        IntPtr handle,
        OBJECT_INFORMATION_CLASS objectInformationClass,
        IntPtr buf,
        int bufLength,
        ref int returnLength);

    public static SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[] QuerySystemHandleInformation()
    {
        for (var bufSize = 32 * 1024 * 1024 /* 32Mb */; bufSize <= 1024 * 1024 * 1024 /* 1Gb */; bufSize *= 2)
        {
            var buffer = Marshal.AllocHGlobal(bufSize);
            try
            {
                var status = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation, buffer, bufSize, out _);

                if (status == NT_SUCCESS)
                {
                    var handleInfo = Marshal.PtrToStructure<SYSTEM_HANDLE_INFORMATION_EX>(buffer);
                    return GetHandleEntries(handleInfo, buffer);
                }

                if (status != STATUS_INFO_LENGTH_MISMATCH)
                {
                    throw new InvalidOperationException("NtQuerySystemInformation failed with status " + status);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        throw new InvalidOperationException("NtQuerySystemInformation buffer size is not enough");
    }

    public static string? GetHandleName(SafeFileHandle handle)
    {
        var buffer = QueryObjectInformation(handle, OBJECT_INFORMATION_CLASS.ObjectNameInformation, out _);
        if (buffer == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            var name = (UNICODE_STRING)Marshal.PtrToStructure(buffer, typeof(UNICODE_STRING))!;
            return name.Length > 0 ? Marshal.PtrToStringUni(name.Buffer, name.Length / 2) : null;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    // Handle types:
    // * ALPC Port
    // * Composition
    // * CoreMessaging
    // * DebugObject
    // * Desktop
    // * Directory
    // * DxgkCompositionObject
    // * DxgkDisplayManagerObject
    // * DxgkSharedResource
    // * DxgkSharedSyncObject
    // * EnergyTracker
    // * EtwConsumer
    // * EtwRegistration
    // * Event
    // * File
    // * FilterCommunicationPort
    // * FilterConnectionPort
    // * IRTimer
    // * IoCompletion
    // * IoCompletionReserve
    // * Job
    // * Key
    // * Mutant
    // * Partition
    // * PcwObject
    // * PowerRequest
    // * Process
    // * RawInputManager
    // * Section
    // * Semaphore
    // * Session
    // * SymbolicLink
    // * Thread
    // * Timer
    // * TmEn
    // * TmRm
    // * TmTm
    // * Token
    // * TpWorkerFactory
    // * UserApcReserve
    // * WaitCompletionPacket
    // * WindowStation
    // * WmiGuid
    public static string? GetHandleType(SafeFileHandle handle)
    {
        var buffer = QueryObjectInformation(handle, OBJECT_INFORMATION_CLASS.ObjectTypeInformation, out _);
        if (buffer == IntPtr.Zero)
        {
            return null;
        }

        try
        {
            var info = (OBJECT_TYPE_INFORMATION)Marshal.PtrToStructure(buffer, typeof(OBJECT_TYPE_INFORMATION))!;
            return info.Name.Length > 0 ? Marshal.PtrToStringUni(info.Name.Buffer, info.Name.Length / 2) : string.Empty;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static IntPtr QueryObjectInformation(SafeFileHandle handle, OBJECT_INFORMATION_CLASS informationClass, out int length)
    {
        length = 0;
        NtQueryObject(handle.DangerousGetHandle(), informationClass, IntPtr.Zero, 0, ref length);

        if (length == 0)
        {
            return IntPtr.Zero;
        }

        var buffer = Marshal.AllocHGlobal(length);
        if (NtQueryObject(handle.DangerousGetHandle(), informationClass, buffer, length, ref length) != 0)
        {
            Marshal.FreeHGlobal(buffer);
            return IntPtr.Zero;
        }

        return buffer;
    }

    private static SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[] GetHandleEntries(SYSTEM_HANDLE_INFORMATION_EX handleInfo, IntPtr buffer)
    {
        var numberOfHandles = handleInfo.NumberOfHandles.ToInt64();
        var handleEntries = new SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[numberOfHandles];

        long handleSize = Marshal.SizeOf(typeof(SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX));
        var current = new IntPtr(buffer.ToInt64() + Marshal.SizeOf(typeof(SYSTEM_HANDLE_INFORMATION_EX)) - handleSize);

        for (long i = 0; i < numberOfHandles; i++)
        {
            var handleEntry = Marshal.PtrToStructure<SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX>(current);
            handleEntries[i] = handleEntry;
            current = new IntPtr(current.ToInt64() + handleSize);
        }

        return handleEntries;
    }
}
