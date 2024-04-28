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

    private enum SYSTEM_INFORMATION_CLASS
    {
        SystemExtendedHandleInformation = 64
    }

    private const uint STATUS_INFO_LENGTH_MISMATCH = 0xC0000004;
    private const int NT_SUCCESS = 0;

    [DllImport("ntdll.dll")]
    private static extern uint NtQuerySystemInformation(
        SYSTEM_INFORMATION_CLASS systemInformationClass,
        IntPtr systemInformation,
        int systemInformationLength,
        out int returnLength);

    public static SYSTEM_HANDLE_TABLE_ENTRY_INFO_EX[] QuerySystemHandleInformation()
    {
        for (var buffSize = 32 * 1024 * 1024 /* 32Mb */; buffSize <= 1024 * 1024 * 1024 /* 1Gb */; buffSize *= 2)
        {
            var buffer = Marshal.AllocHGlobal(buffSize);
            try
            {
                var status = NtQuerySystemInformation(SYSTEM_INFORMATION_CLASS.SystemExtendedHandleInformation, buffer, buffSize, out _);

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
