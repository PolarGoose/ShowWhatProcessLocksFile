using ShowWhatProcessLocksFile.Utils;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ShowWhatProcessLocksFile.LockingProcessesInfo.HandleExe
{
    public class HandleOutputParser
    {
        public static IEnumerable<HandleParsedLine> Parse(string output)
        {
            var lines = output.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length == 1 && lines[0] == "No matching handles found.")
            {
                Log.Info("Nothing blocks this folder");
                return new List<HandleParsedLine>();
            }

            var r = @"^(?<ProcessName>.*(?=\spid:\s))\spid:\s(?<Pid>\d+)\s+type:\s(?<Type>(File)|(Section))\s+(?<UserName>[^:]*)\s+(?<HandleCode>[A-Z0-9]+):\s(?<FileName>.*)$";
            var parsed = new List<HandleParsedLine>();
            foreach (var line in lines)
            {
                try
                {
                    var match = Regex.Match(line, r);
                    parsed.Add(new HandleParsedLine(
                        processName: match.Groups["ProcessName"].Value.Trim(),
                        pid: int.Parse(match.Groups["Pid"].Value),
                        handleType: match.Groups["Type"].Value.Trim() == "File" ? HandleType.File : HandleType.Section,
                        userName: match.Groups["UserName"].Value.Trim(),
                        handleCode: match.Groups["HandleCode"].Value.Trim(),
                        fileFullName: match.Groups["FileName"].Value.Trim()));
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Filed to parse line: '{line}'", ex);
                }
            }

            return parsed;
        }
    }
}
