using System.IO;
using System.Windows.Media;
using ShowWhatProcessLocksFile.LockFinding.Interop;
using ShowWhatProcessLocksFile.LockFinding.Utils;

namespace ShowWhatProcessLocksFile.LockFinding;

public record struct ProcessInfo(
    int Pid,
    string ProcessName,
    string ProcessExecutableFullName,
    string DomainAndUserName,
    ImageSource Icon,
    List<string> LockedFileFullNames);

public static class LockFinder
{
    public static IEnumerable<ProcessInfo> FindWhatProcessesLockPath(string path)
    {
        path = PathUtils.ToCanonicalPath(path);
        var currentProcess = WinApi.GetCurrentProcess();
        var result = new List<ProcessInfo>();

        foreach (var pidAndHandles in NtDll.QuerySystemHandleInformation().GroupBy(h => h.UniqueProcessId))
        {
            var (pid, handles) = (pidAndHandles.Key, pidAndHandles);

            using var openedProcess = WinApi.OpenProcess(WinApi.ProcessAccessRights.PROCESS_DUP_HANDLE | WinApi.ProcessAccessRights.PROCESS_QUERY_INFORMATION, false, pid);
            if (openedProcess.IsInvalid)
            {
                continue;
            }

            var processInfo = new ProcessInfo
            {
                Pid = (int)pid.ToUInt64(),
                LockedFileFullNames = []
            };

            foreach (var h in handles)
            {
                using var dupHandle = WinApi.DuplicateHandle(currentProcess, openedProcess, h);
                if (dupHandle.IsInvalid)
                {
                    continue;
                }

                using var reopenedHandle = WinApi.ReOpenFile(dupHandle, WinApi.FileDesiredAccess.None, FileShare.None, WinApi.FileFlagsAndAttributes.None);
                if (reopenedHandle.IsInvalid)
                {
                    continue;
                }

                var fileOrFolderFullName = WinApi.GetFinalPathNameByHandle(reopenedHandle);
                if (fileOrFolderFullName?.StartsWith(path, StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    processInfo.LockedFileFullNames.Add(fileOrFolderFullName);
                }
            }

            if (processInfo.LockedFileFullNames.Any())
            {
                processInfo.DomainAndUserName = ProcessUtils.GetOwnerDomainAndUserNames(openedProcess);
                var process = ProcessUtils.GetProcess((int)pid.ToUInt64());
                if (process != null)
                {
                    processInfo.ProcessExecutableFullName = ProcessUtils.GetExecutablePath(process);
                    processInfo.ProcessName = Path.GetFileName(processInfo.ProcessExecutableFullName);
                    if (processInfo.ProcessExecutableFullName != null)
                    {
                        processInfo.Icon = ProcessUtils.GetIcon(processInfo.ProcessExecutableFullName);
                    }
                }

                result.Add(processInfo);
            }
        }

        return result;
    }
}
