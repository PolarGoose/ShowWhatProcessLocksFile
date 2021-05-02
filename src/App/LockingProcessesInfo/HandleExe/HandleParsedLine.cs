namespace ShowWhatProcessLocksFile.LockingProcessesInfo.HandleExe
{
    public enum HandleType
    {
        Section,
        File
    }

    public sealed class HandleParsedLine
    {
        public string ProcessName { get; }
        public int Pid { get; }
        public HandleType HandleType { get; }
        public string UserName { get; }
        public string HandleCode { get; }
        public string FileFullName { get; }

        public HandleParsedLine(string processName, int pid, HandleType handleType, string userName, string handleCode, string fileFullName)
        {
            ProcessName = processName;
            Pid = pid;
            HandleType = handleType;
            UserName = userName;
            HandleCode = handleCode;
            FileFullName = fileFullName;
        }

        public override string ToString()
        {
            return $"'{ProcessName}' {Pid} {HandleType} '{UserName}' '{FileFullName}'";
        }
    }
}
