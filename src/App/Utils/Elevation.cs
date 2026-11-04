using ShowWhatProcessLocksFile.LockFinding.Utils;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace ShowWhatProcessLocksFile.Utils;

internal static class Elevation
{
    public static void RestartAsAdmin(CanonicalPath canonicalPath)
    {
        using var process = Process.Start(new ProcessStartInfo(Environment.ProcessPath!)
        {
            UseShellExecute = true,
            Verb = "runas",
            ArgumentList = { canonicalPath.Path }
        });
        ((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).Shutdown();
    }

    [DllImport("shell32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool IsUserAnAdmin();
}
