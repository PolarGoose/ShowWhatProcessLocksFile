# ShowWhatProcessLocksFile Core Principles and System Calls

## Summary

This tool does not try opening the target path itself. Instead it enumerates every handle in the system, duplicates the handle into the current process, resolves the handle to a file path, and then compares that path against the requested file or folder.

The main flow lives in `src/App/LockFinding/LockFinder.cs`:

1. Call `NtQuerySystemInformation(SystemExtendedHandleInformation)` to retrieve the system-wide handle table.
2. Group the handles by PID.
3. For each process call `OpenProcess(PROCESS_DUP_HANDLE | PROCESS_QUERY_INFORMATION | PROCESS_VM_READ)`.
4. For every handle call `DuplicateHandle` so the current process owns a copy.
5. Pass the duplicated handle to `GetFinalPathNameByHandleW` to get the actual filesystem path.
6. Compare paths for exact match when the target is a file, or prefix match when the target is a directory.
7. Optionally scan process modules via `EnumProcessModules + GetModuleFileNameEx` to catch DLL/EXE mappings.

## Key syscalls / Win32 APIs

### 1) Handle enumeration (critical)

- `NtQuerySystemInformation` (`ntdll.dll`)
  - Location: `src/App/LockFinding/Interop/NtDll.cs`
  - Class used: `SystemExtendedHandleInformation (64)`
  - Purpose: captures every handle in the system, including owning PID, handle value, and granted access.

This is the only way the tool learns which process—if any—owns which handle.

### 2) Cross-process handle inspection

- `OpenProcess` (`kernel32.dll`)
  - Location: `src/App/LockFinding/Interop/WinApi.cs`
  - Purpose: open target process with enough rights to duplicate handles.

- `DuplicateHandle` (`kernel32.dll`)
  - Location: `src/App/LockFinding/Interop/WinApi.cs`
  - Purpose: copy a foreign handle into the current process so it can be interrogated safely.

- `GetFinalPathNameByHandleW` (`kernel32.dll`)
  - Location: `src/App/LockFinding/Interop/WinApi.cs`
  - Purpose: resolve the handle back into a filesystem path (normalized by stripping `\\?\`).

### 3) Process metadata and module probing

- `QueryFullProcessImageName` (`kernel32.dll`)
- `OpenProcessToken` (`advapi32.dll`)
- `EnumProcessModules` / `GetModuleFileNameEx` (`psapi.dll`)

These calls collect the executable path, owner principal, and loaded modules for each process. Module probing supplements the handle scan by catching mapped DLLs and EXEs that may lock the target path.

## Stability safeguards in code

When `GetFinalPathNameByHandleW` runs, the enumeration uses `WorkerThreadWithDeadLockDetection + Watchdog` (50ms timeout) so a slow or misbehaving handle cannot freeze the scan:

- `src/App/LockFinding/Utils/WorkerThreadWithDeadLockDetection.cs`
- `src/App/LockFinding/Utils/Watchdog.cs`

This is not part of the locking logic itself but makes the tool viable in real scenarios.

## Role of `NtQueryObject`

`NtQueryObject` is also declared (`src/App/LockFinding/Interop/NtDll.cs`) and wrapped by:

- `GetHandleName`
- `GetHandleType`

The current `LockFinder.FindWhatProcessesLockPath` implementation never invokes these helpers. The effective pipeline is `NtQuerySystemInformation + DuplicateHandle + GetFinalPathNameByHandleW`, not `NtQueryObject`.
