using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace ShowWhatProcessLocksFile.Utils;

internal static class ErrorDialog
{
    private const uint MB_OK = 0x00000000;
    private const uint MB_ICONERROR = 0x00000010;
    private const uint MB_TASKMODAL = 0x00002000;

    [DoesNotReturn]
    public static void Show(string text)
    {
        MessageBoxW(IntPtr.Zero, text, "Error", MB_OK | MB_ICONERROR | MB_TASKMODAL);
        Environment.Exit(1);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
