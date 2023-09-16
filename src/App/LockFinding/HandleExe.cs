using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CliWrap;
using CliWrap.Buffered;
using ShowWhatProcessLocksFile.LockFinding.Utils;

namespace ShowWhatProcessLocksFile.LockFinding;

// More information on the meaning of the fields that are printed by Handle.exe:
// https://stackoverflow.com/questions/52701911/output-of-sysinternals-handle-exe
public readonly record struct HandleInfo (
    string ProcessName,
    int Pid,
    string HandleType,
    string UserAndDomainName,
    long HandleAddress,
    string LockedPath);

public static class HandleExe
{
    private static readonly string HandleExeFullName = Path.Combine(AppContext.BaseDirectory, "handle64_v5.0_Unicode.exe");

    public static IEnumerable<HandleInfo> Execute(string path)
    {
        var rawOutput = Launch(path);
        return ParseRawOutput(rawOutput);
    }

    private static string Launch(string path)
    {
        // Handle.exe doesn't work if a path contains "\" character at the end
        path = PathUtils.RemoveTrailingPathSeparator(path);

        var res = Cli
            .Wrap(HandleExeFullName)
            .WithValidation(CommandResultValidation.None)
            .WithArguments(new[] { "-u", "-nobanner", "-accepteula", "-v", path })
            .ExecuteBufferedAsync(Encoding.UTF8)
            .GetAwaiter()
            .GetResult();
        if (res.ExitCode != 0)
        {
            if (res.StandardOutput == "No matching handles found.\r\n")
            {
                return "";
            }
            throw new ApplicationException(
                $"'{HandleExeFullName}' failed for '{path}'.\nExitCode={res.ExitCode}\nStdError:\n{res.StandardError}\nStdOut:\n{res.StandardOutput}");
        }

        return res.StandardOutput;
    }

    // The console output of Handle.exe looks like this:
    //     Process,PID,User,Handle,Type,Share Flags,Name,Access
    //     ipf_helper.exe,3456,File,Domain\UserName,0x00000050,C:\Windows\System32\DriverStore\FileRepository\ipf_cpu.inf_amd64_e6050705c26c770f
    //     sihost.exe,21260,File,Domain\UserName,0x00000044,C:\Windows\System32
    //     sihost.exe,21260,File,Domain\UserName,0x000005E0,C:\Windows\System32\en-US\windows.storage.dll.mui
    // Notes:
    // * The header doesn't match the actual data that is being printed. Therefore we have to ignore.
    // * There is a trailing whitespace at the end of every line.
    private static IEnumerable<HandleInfo> ParseRawOutput(string rawOutput)
    {
        return CsvParser.Parse(rawOutput).Skip(1).Select(line => new HandleInfo
        {
            ProcessName = line[0],
            Pid = int.Parse(line[1]),
            HandleType = line[2],
            UserAndDomainName = line[3],
            HandleAddress = Convert.ToInt64(line[4], 16),
            LockedPath = line[5]
        });
    }
}
