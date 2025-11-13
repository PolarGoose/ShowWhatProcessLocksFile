using System.IO;
using System.Windows.Media;
using Microsoft.Win32.SafeHandles;
using ShowWhatProcessLocksFile.LockFinding.Interop;
using ShowWhatProcessLocksFile.LockFinding.Utils;

namespace ShowWhatProcessLocksFile.LockFinding;

public record struct ProcessInfo(
    int Pid,
    string? ProcessName,
    string? ProcessExecutableFullName,
    string? DomainAndUserName,
    ImageSource? Icon,
    List<string> LockedFileFullNames);

public static class LockFinder
{
    public static IEnumerable<ProcessInfo> FindWhatProcessesLockPath(string path)
    {
        path = PathUtils.ToCanonicalPath(path);
        var currentProcess = WinApi.GetCurrentProcess();
        var result = new List<ProcessInfo>();

        var processes = NtDll.QuerySystemHandleInformation().GroupBy(h => h.UniqueProcessId).Select(processAndHandles => (processAndHandles.Key, processAndHandles.ToArray())).ToArray();
        var currentProcessIndex = 0;
        var currentHandleIndex = 0;
        SafeProcessHandle? currentOpenedProcess = null;
        var currentLockedFiles = new List<string>();
        SafeFileHandle? currentDupHandle = null;

        while (currentProcessIndex < processes.Length)
        {
            new WorkerThreadWithDeadLockDetection(TimeSpan.FromMilliseconds(50), watchdog =>
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

                        currentDupHandle = WinApi.DuplicateHandle(currentProcess, currentOpenedProcess, h);
                        if (currentDupHandle.IsInvalid)
                        {
                            continue;
                        }

                        watchdog.Arm();
                        var lockedFileName = WinApi.GetFinalPathNameByHandle(currentDupHandle);
                        watchdog.Disarm();
                        if (lockedFileName is null)
                        {
                            continue;
                        }

                        lockedFileName = PathUtils.AddTrailingSeparatorIfItIsAFolder(lockedFileName);
                        if (path.EndsWith('\\') && lockedFileName.StartsWith(path, StringComparison.InvariantCultureIgnoreCase)
                            || string.Equals(lockedFileName, path, StringComparison.InvariantCultureIgnoreCase))
                        {
                            currentLockedFiles.Add(lockedFileName);
                        }
                    }

                    var moduleNames = ProcessUtils.GetProcessModules(currentOpenedProcess)
                                                  .Where(name => path.EndsWith('\\') && name.StartsWith(path, StringComparison.InvariantCultureIgnoreCase)
                                                                 || string.Equals(name, path, StringComparison.InvariantCultureIgnoreCase)).ToList();

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
            }).Run();
        }

        return result;
    }
}
