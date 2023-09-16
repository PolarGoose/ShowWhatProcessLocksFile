using System;
using System.Collections.Generic;
using System.Diagnostics;
using ShowWhatProcessLocksFile.LockFinding;

namespace ShowWhatProcessLocksFile.Utils;

internal static class ProcessKiller
{
    public static void Kill(IEnumerable<ProcessInfo> processes)
    {
        foreach (var p in processes)
        {
            Log.Info($"Request to kill '{p.ProcessName}' Pid:{p.Pid}");
            var process = Process.GetProcessById(p.Pid);
            process.Kill();
        }

        foreach (var p in processes)
        {
            Log.Info($"Waiting for '{p.ProcessName}' Pid:{p.Pid}");
            var process = Process.GetProcessById(p.Pid);
            var res = process.WaitForExit(3000);
            if (!res)
            {
                throw new Exception($"Timeout waiting for a process to exit: Pid={p.Pid} Name='{p.ProcessName}'");
            }
        }
    }
}
