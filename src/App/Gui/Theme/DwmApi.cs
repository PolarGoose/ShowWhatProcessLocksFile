using System.Runtime.InteropServices;

namespace ShowWhatProcessLocksFile.Gui.Theme;

internal static class DwmApi
{
    // DWMWA_USE_IMMERSIVE_DARK_MODE is 20 since Windows 10 20H1 (build 18985).
    // Windows 10 1809 - 1909 expose the same attribute under 19.
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

    public static void SetDarkTitleBar(IntPtr hwnd, bool enabled)
    {
        var value = enabled ? 1 : 0;
        if (DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int)) != 0)
        {
            _ = DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref value, sizeof(int));
        }
    }
}
