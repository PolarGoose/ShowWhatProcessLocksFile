using MoreLinq;
using ShowWhatProcessLocksFile.LockFinding.HandleExe;
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

namespace ShowWhatProcessLocksFile.LockFinding
{
    public static class LockFinder
    {
        public static IEnumerable<ProcessInfo> FindWhatProcessesLockPath(string path)
        {
            var output = Handle.Execute(path);
            var parsedHandleExeOutput = HandleOutputParser.Parse(output);

            return parsedHandleExeOutput
                // File path which we supply to Handle.exe is treated as "Show locks for all entities whose names start with this path".
                // Which is not what we want, because if we ask to show what locks "C:\Program Files" folder,
                // it will also show processes which lock "C:\Program Files (x86)" folder.
                // Therefore, we need to manually filter out this extra information.
                .Where(p => string.Equals(p.FileFullName, path, StringComparison.OrdinalIgnoreCase)
                            || PathUtils.IsInsideFolder(p.FileFullName, path)
                            // If the path contains cyrillic letters, Handle.exe replaces them with '?'.
                            // For example if something locks "C:\файл.txt" the Handle.exe will print "C:\????.txt" as the name of the file.
                            // However, the first condition "string.Equals(p.FileFullName, ...)" will filter such lines out.
                            // Therefore, regardless if this file is locked or not, our app will show that nothing locks this file.
                            || p.FileFullName.Contains('?'))
                // Handle.exe produces non grouped output. We need to group all locked handles per process.
                .GroupBy(el => el.Pid)
                // There can be several records corresponding to the same file locked by the same process but with a different 'HandleCode'.
                // We don't use 'HandleCode' field and want to leave only one of these records to avoid showing the user that the same file is locked twice.
                .Select(el => el.DistinctBy(e => e.FileFullName))
                .Select(el => CreateProcessInfo(el))
                // Remove all elements for which 'CreateProcessInfo' has failed to extract process information
                .Where(el => el != null)
                .OrderBy(el => el.Name);
        }

        private static ProcessInfo CreateProcessInfo(IEnumerable<HandleParsedLine> handleExeParsedLinesForTheSameProcess)
        {
            try
            {
                var handle = handleExeParsedLinesForTheSameProcess.First();
                var pid = handle.Pid;
                var processName = handle.ProcessName;
                var process = Process.GetProcessById(pid);
                // There are processes for which it is not possible to get full name and icon.
                // One of such processes is "System"
                var processFullName = TryGetProcessFullName(process);
                var processIcon = TryGetIcon(processFullName);

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

        private static string TryGetProcessFullName(Process process)
        {
            try
            {
                return process.MainModule.FileName;
            }
            catch (Exception ex)
            {
                Log.Warn($"Failed to get a full name of a process: name='{process.ProcessName}' pid='{process.Id}'. Exception:\n{ex}");
                return null;
            }
        }

        private static ImageSource TryGetIcon(string executableFullName)
        {
            if (executableFullName == null)
            {
                return null;
            }

            try
            {
                using (var ico = Icon.ExtractAssociatedIcon(executableFullName))
                {
                    var image = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    // We need to freeze the image, otherwise the GUI thread will not be able to use it if this function was called from another process
                    image.Freeze();
                    return image;
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"Failed to get an icon from executable '{executableFullName}'. Exception:\n{ex}");
                return null;
            }
        }
    }
}
