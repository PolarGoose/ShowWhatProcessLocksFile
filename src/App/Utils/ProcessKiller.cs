using System.Diagnostics;
using ShowWhatProcessLocksFile.LockFinding;

namespace ShowWhatProcessLocksFile.Utils;

internal static class ProcessKiller
{
    public static void Kill(IEnumerable<ProcessInfo> processes)
    {
        var killedProcesses = new List<(Process Process, ProcessInfo Info)>();
        try
        {
            foreach (var info in processes)
            {
                var process = Process.GetProcessById(info.Pid);
                killedProcesses.Add((process, info));
                process.Kill();
            }

            foreach (var (process, info) in killedProcesses)
            {
                if (!process.WaitForExit(3000))
                {
                    throw new Exception($"Timeout waiting for a process to exit: Pid={info.Pid} Name='{info.ProcessName}'");
                }
            }
        }
        finally
        {
            foreach (var (process, _) in killedProcesses)
            {
                process.Dispose();
            }
        }
    }
}
