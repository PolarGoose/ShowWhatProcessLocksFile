using ShowWhatProcessLocksFile.LockFinding;
using System;
using System.Collections.Generic;

namespace ShowWhatProcessLocksFile.Utils
{
    internal static class ProcessKiller
    {
        public static void Kill(IEnumerable<ProcessInfo> processes)
        {
            foreach (var p in processes)
            {
                Log.Info($"Request to kill '{p.Name}' Pid:{p.Pid}");
                p.Process.Kill();
            }

            foreach (var p in processes)
            {
                Log.Info($"Waiting for '{p.Name}' Pid:{p.Pid}");
                var res = p.Process.WaitForExit(3000);
                if (!res)
                {
                    throw new Exception($"Timeout waiting for a process to exit: Pid={p.Pid} Name='{p.Name}'");
                }
            }
        }
    }
}
