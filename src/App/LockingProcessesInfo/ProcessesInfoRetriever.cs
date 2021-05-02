using MoreLinq;
using ShowWhatProcessLocksFile.LockingProcessesInfo.HandleExe;
using ShowWhatProcessLocksFile.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShowWhatProcessLocksFile.LockingProcessesInfo
{
    public static class ProcessesInfoRetriever
    {
        public static IEnumerable<ProcessInfo> GetProcessesInfo(string fileFullName)
        {
            var output = Handle.Execute(fileFullName);
            var parsedHandleExeOutput = HandleOutputParser.Parse(output);

            // Handle.exe produces non grouped output. We need to group all locked handles per process.
            var groupedByPid = parsedHandleExeOutput.GroupBy(el => el.Pid);

            // There can be several records corresponding to the same file locked by the same process but with a different 'HandleCode'.
            // We don't care about the 'HandleCode' and therefore want to leave only one of these records to avoid showing the user that the same file is locked twice.
            var groupedByPidWithoutDiplicateFileNames = groupedByPid.Select(el => el.DistinctBy(e => e.FileFullName));

            return groupedByPidWithoutDiplicateFileNames.Select(el => CreateProcessInfo(el)).Where(el => el != null);
        }

        private static ProcessInfo CreateProcessInfo(IEnumerable<HandleParsedLine> handleExeParsedLinesForTheSameProcess)
        {
            try
            {
                var handle = handleExeParsedLinesForTheSameProcess.First();
                var pid = handle.Pid;
                var processName = handle.ProcessName;
                var process = Process.GetProcessById(pid);
                var processFullName = process.MainModule.FileName;
                var processIcon = GetProcessIcon(processFullName);

                return new ProcessInfo(
                    pid: pid,
                    processName: processName,
                    executableFullName: processFullName,
                    process: process,
                    icon: processIcon,
                    userName: handle.UserName,
                    lockedFiles: handleExeParsedLinesForTheSameProcess.Select(l => l.FileFullName).OrderBy(x => x));
            }
            catch (Exception ex)
            {
                Log.Warn($"Failed to create a process info from: '{handleExeParsedLinesForTheSameProcess.FirstOrDefault()}'. Exception:\n{ex}");
                return null;
            }
        }

        private static ImageSource GetProcessIcon(string executableFullName)
        {
            using (var ico = Icon.ExtractAssociatedIcon(executableFullName))
            {
                return Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }
    }
}
