using System.Runtime.InteropServices;

namespace ShowWhatProcessLocksFile.Gui.Theme;

/// <summary>
/// WPF answers WM_ERASEBKGND without painting and presents its first frame asynchronously, so DWM would display
/// the window's untouched white surface from the moment it becomes visible until that frame arrives. ShowWindow
/// makes the window visible before running activation and only then erases, so the erase alone is tens of
/// milliseconds late. This hook paints the theme background with GDI on the first message that arrives while the
/// window is visible, and again on every erase. Public HwndSource hooks run before WPF's own handler.
/// </summary>
internal sealed class WindowBackgroundPainter
{
    private const int WM_ERASEBKGND = 0x0014;

    private bool paintedOnFirstShow;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    public IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (!paintedOnFirstShow && IsWindowVisible(hwnd))
        {
            paintedOnFirstShow = true;
            GdiApi.FillClientArea(hwnd, ThemeManager.WindowBackgroundColor);
        }

        if (msg == WM_ERASEBKGND)
        {
            GdiApi.FillClientArea(hwnd, hdc: wParam, ThemeManager.WindowBackgroundColor);
            handled = true;
            return new IntPtr(1);
        }

        return IntPtr.Zero;
    }
}
