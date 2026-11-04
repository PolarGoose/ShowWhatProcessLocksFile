using System.IO;
using Microsoft.Win32.SafeHandles;
using ShowWhatProcessLocksFile.LockFinding.Interop;
using ShowWhatProcessLocksFile.LockFinding.Utils;

namespace ShowWhatProcessLocksFile.LockFinding;

public record struct ProcessInfo(
    int Pid,
    string? ProcessName,
    string? ProcessExecutableFullName,
    string? DomainAndUserName,
    byte[]? Icon,
    List<string> LockedFileFullNames);

internal static class LockFinder
{
    public static IEnumerable<ProcessInfo> FindWhatProcessesLockPath(CanonicalPath path)
    {
        var devicePathToDrivePathConverter = new DevicePathToDrivePathConverter();
        var currentProcess = WinApi.GetCurrentProcess();
        var result = new List<ProcessInfo>();

        var processes = NtDll.QuerySystemHandleInformation().GroupBy(h => h.UniqueProcessId).Select(processAndHandles => (processAndHandles.Key, processAndHandles.ToArray())).ToArray();
        var currentProcessIndex = 0;
        var currentHandleIndex = 0;
        SafeProcessHandle? currentOpenedProcess = null;
        var currentLockedFiles = new List<string>();
        SafeFileHandle? currentDupHandle = null;
        ushort? fileHandleObjectTypeIndex = null;

        while (currentProcessIndex < processes.Length)
        {
            WorkerThreadWithDeadLockDetection.Run(TimeSpan.FromMilliseconds(50), watchdog =>
            {
                while (currentProcessIndex < processes.Length)
                {
                    var (pid, handles) = processes[currentProcessIndex];

                    if (currentOpenedProcess is null)
                    {
                        currentOpenedProcess = ProcessUtils.OpenProcessToDuplicateHandle(pid);
                        if (currentOpenedProcess is null)
                        {
                            currentProcessIndex++;
                            continue;
                        }

                        currentLockedFiles = new List<string>();
                        currentHandleIndex = 0;
                    }

                    while (currentHandleIndex < handles.Length)
                    {
                        currentDupHandle?.Dispose();
                        var h = handles[currentHandleIndex];
                        currentHandleIndex++;

                        if (fileHandleObjectTypeIndex is not null && h.ObjectTypeIndex != fileHandleObjectTypeIndex)
                        {
                            continue;
                        }

                        currentDupHandle = WinApi.DuplicateHandle(currentProcess, currentOpenedProcess, h);
                        if (currentDupHandle.IsInvalid)
                        {
                            continue;
                        }

                        watchdog.Arm();
                        string? lockedFileName = null;
                        try
                        {
                            if(NtDll.GetHandleType(currentDupHandle) != "File")
                            {
                                continue;
                            }
                            fileHandleObjectTypeIndex = h.ObjectTypeIndex;
                            lockedFileName = NtDll.GetHandleName(currentDupHandle);
                        }
                        catch (Exception)
                        {
                        }
                        finally
                        {
                            watchdog.Disarm();
                        }

                        if (lockedFileName == null)
                        {
                            continue;
                        }

                        lockedFileName = ToWindowsPath(devicePathToDrivePathConverter, lockedFileName);
                        if (lockedFileName is null)
                        {
                            continue;
                        }

                        lockedFileName = PathUtils.AddTrailingSeparatorIfItIsAFolder(lockedFileName);
                        if (path.IsDirectory && lockedFileName.StartsWith(path.Path, StringComparison.InvariantCultureIgnoreCase)
                            || string.Equals(lockedFileName, path.Path, StringComparison.InvariantCultureIgnoreCase))
                        {
                            currentLockedFiles.Add(lockedFileName);
                        }
                    }

                    var moduleNames = ProcessUtils.GetProcessModules(currentOpenedProcess)
                                                  .Select(name => ToWindowsPath(devicePathToDrivePathConverter, name) ?? name)
                                                  .Where(name => path.IsDirectory && name.StartsWith(path.Path, StringComparison.InvariantCultureIgnoreCase)
                                                                 || string.Equals(name, path.Path, StringComparison.InvariantCultureIgnoreCase)).ToList();

                    if (currentLockedFiles.Any() || moduleNames.Any())
                    {
                        var processInfo = new ProcessInfo
                        {
                            Pid = (int)pid.ToUInt64(),
                            LockedFileFullNames = currentLockedFiles.Concat(moduleNames).Distinct().OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList(),
                            DomainAndUserName = ProcessUtils.GetOwnerDomainAndUserName(currentOpenedProcess),
                            ProcessExecutableFullName = ProcessUtils.GetProcessExeFullName(currentOpenedProcess),
                        };

                        if (processInfo.ProcessExecutableFullName != null)
                        {
                            processInfo.ProcessName = Path.GetFileName(processInfo.ProcessExecutableFullName);
                            processInfo.Icon = ProcessUtils.GetIcon(processInfo.ProcessExecutableFullName);
                        }

                        result.Add(processInfo);
                    }

                    currentDupHandle?.Dispose();
                    currentOpenedProcess.Dispose();
                    currentOpenedProcess = null;
                    currentProcessIndex++;
                }
            });
        }

        return result;
    }

    private static string? ToWindowsPath(DevicePathToDrivePathConverter devicePathToDrivePathConverter, string devicePath)
    {
        // Network file or folder like "\Device\Mup\wsl.localhost\Ubuntu-24.04\home\user"
        // Convert it to UNC path like "\\wsl.localhost\Ubuntu-24.04\home\user\"
        if (devicePath.StartsWith("\\Device\\Mup\\"))
        {
            return PathUtils.AddTrailingSeparatorIfItIsAFolder("\\" + devicePath.Substring(11));
        }

        if(devicePath.StartsWith("\\Device\\") && devicePathToDrivePathConverter.GetDriveLetterBasedFullName(devicePath) is { } path)
        {
            return PathUtils.AddTrailingSeparatorIfItIsAFolder(path);
        }

        return null;
    }
}
