using ShowWhatProcessLocksFile.LockFinding.Utils;

namespace ShowWhatProcessLocksFile.Utils;

internal static class CommandLineParser
{
    public static CanonicalPath GetFileFullName(string[] args)
    {
        if (args.Length == 0)
        {
            ErrorDialog.Show("Please provide full path to file or folder as a command line argument");
        }

        if (args.Length > 1)
        {
            ErrorDialog.Show("Please provide exactly one file or folder path as a command line argument");
        }

        if (!Exists(args[0]))
        {
            ErrorDialog.Show($"'{args[0]}' doesn't exist");
        }

        return new CanonicalPath(args[0]);
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
