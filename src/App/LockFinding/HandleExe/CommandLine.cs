using CliWrap;
using CliWrap.Buffered;
using ShowWhatProcessLocksFile.Utils;
using System;

namespace ShowWhatProcessLocksFile.LockFinding.HandleExe
{
    public static class CommandLine
    {
        public static string Execute(string fileFullName, string arguments)
        {
            Log.Info($"Execute process '{fileFullName} {arguments}'");

            var res = Cli.Wrap(fileFullName).WithValidation(CommandResultValidation.None).WithArguments(arguments).ExecuteBufferedAsync().GetAwaiter().GetResult();

            if (res.ExitCode != 0)
            {
                throw new ApplicationException($"Command '{fileFullName} {arguments}' failed with exitCode={res.ExitCode}. Error output:\n{res.StandardError}\nOutput:\n{res.StandardOutput}");
            }

            Log.Info($"Execution succeeded, output:\n{res.StandardOutput}");
            return res.StandardOutput;
        }
    }
}
