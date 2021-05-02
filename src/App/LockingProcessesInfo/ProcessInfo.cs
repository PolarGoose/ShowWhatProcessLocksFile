
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;

namespace ShowWhatProcessLocksFile.LockingProcessesInfo
{
    public class ProcessInfo
    {
        public int Pid { get; }
        public string Name { get; }
        public string ExecutableFullName { get; }
        public string UserName { get; }
        public Process Process { get; }
        public ImageSource Icon { get; }
        public IEnumerable<string> LockedFiles { get; }

        public ProcessInfo(int pid, string processName, string executableFullName, Process process, ImageSource icon, string userName, IEnumerable<string> lockedFiles)
        {
            Pid = pid;
            Name = processName;
            ExecutableFullName = executableFullName;
            Process = process;
            Icon = icon;
            UserName = userName;
            LockedFiles = lockedFiles;
        }
    }
}
