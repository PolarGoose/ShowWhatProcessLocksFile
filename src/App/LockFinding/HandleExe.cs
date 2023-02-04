using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CliWrap;
using CliWrap.Buffered;

namespace ShowWhatProcessLocksFile.LockFinding;

public class LockingProcess
{
    public int pid;
    public string process_full_name;
    public string user;
    public string domain;
    public List<string> locked_paths;
}

public static class HandleExe
{
    private static readonly string HandleExeFullName = Path.Combine(AppContext.BaseDirectory, "Handle2.exe");

    public static async Task<IEnumerable<LockingProcess>> GetProcessesLockingPath(string path)
    {
        var res = await Cli
            .Wrap(HandleExeFullName)
            .WithValidation(CommandResultValidation.None)
            .WithArguments(new[] { "--json", path })
            .ExecuteBufferedAsync(Encoding.UTF8);
        if (res.ExitCode != 0)
        {
            throw new ApplicationException(
                $"{HandleExeFullName} failed for '{path}'. ExitCode={res.ExitCode}. Error message:\n{res.StandardError}");
        }

        return new JavaScriptSerializer().Deserialize<List<LockingProcess>>(res.StandardOutput);
    }
}
