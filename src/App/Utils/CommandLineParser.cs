using System;
using System.IO;

namespace ShowWhatProcessLocksFile.Utils;

internal static class CommandLineParser
{
    public static string GetFileFullName()
    {
        var args = Environment.GetCommandLineArgs();

        if (args.Length == 1)
        {
            throw new Exception("Please provide full path to file or folder as a command line argument");
        }

        if (args.Length > 2)
        {
            throw new Exception($"Wrong number of commandline arguments. {args}");
        }

        if (IsUncPath(args[1]))
        {
            throw new Exception($"UNC paths are not supported:\n{args[1]}");
        }

        if (!Exists(args[1]))
        {
            throw new Exception($"'{args[1]}' doesn't exist");
        }

        Log.Info($"File path is provided: {args[1]}");
        return args[1];
    }

    private static bool Exists(string path)
    {
        try
        {
            _ = File.GetAttributes(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsUncPath(this string path)
    {
        return path.StartsWith(@"\\");
    }
}
