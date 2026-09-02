using System.Runtime.InteropServices;
using System.Windows.Media;

namespace ShowWhatProcessLocksFile.Gui.Theme;

internal static class GdiApi
{
    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern int FillRect(IntPtr hDC, ref RECT lprc, IntPtr hbr);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateSolidBrush(uint crColor);

    [DllImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject(IntPtr hObject);

    public static void FillClientArea(IntPtr hwnd, Color color)
    {
        var hdc = GetDC(hwnd);
        try
        {
            FillClientArea(hwnd, hdc, color);
        }
        finally
        {
            _ = ReleaseDC(hwnd, hdc);
        }
    }

    public static void FillClientArea(IntPtr hwnd, IntPtr hdc, Color color)
    {
        var colorRef = (uint)(color.R | color.G << 8 | color.B << 16);
        var brush = CreateSolidBrush(colorRef);
        try
        {
            _ = GetClientRect(hwnd, out var rect);
            _ = FillRect(hdc, ref rect, brush);
        }
        finally
        {
            _ = DeleteObject(brush);
        }
    }
}
