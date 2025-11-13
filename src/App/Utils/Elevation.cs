using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace ShowWhatProcessLocksFile.Utils;

internal static class Elevation
{
    public static void RestartAsAdmin(string path)
    {
        // some of `\` signs can be interpreted as escape sequences.
        path = path.Replace("\\", "/");

        new Process
        {
            StartInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location, $"\"{path}\"")
            {
                UseShellExecute = true,
                Verb = "runas"
            }
        }.Start();
        Application.Current.Shutdown();
    }

    [DllImport("shell32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsUserAnAdmin();
}
