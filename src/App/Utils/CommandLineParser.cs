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

        if (!Exists(args[1]))
        {
            throw new Exception($"'{args[1]}' doesn't exist");
        }

        return args[1];
    }

    private static bool Exists(string path)
    {
        try
        {
            return File.Exists(path) || Directory.Exists(path);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
