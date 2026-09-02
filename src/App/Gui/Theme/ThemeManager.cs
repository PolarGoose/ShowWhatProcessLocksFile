using Microsoft.Win32;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ShowWhatProcessLocksFile.Gui.Theme;

/// <summary>
/// Follows the Windows "Choose your default app mode" setting (light/dark) and reacts to changes at runtime.
/// </summary>
internal static class ThemeManager
{
    private static readonly Uri LightThemeUri = new("/Gui/Theme/Light.xaml", UriKind.Relative);
    private static readonly Uri DarkThemeUri = new("/Gui/Theme/Dark.xaml", UriKind.Relative);

    private static ResourceDictionary? themeBrushes;

    public static bool IsDark { get; private set; }

    public static void Initialize()
    {
        ApplyTheme(IsSystemAppModeDark());
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
    }

    public static void Attach(Window window)
    {
        window.SourceInitialized += (_, _) =>
        {
            var source = (HwndSource)PresentationSource.FromVisual(window);
            source.AddHook(new WindowBackgroundPainter().Hook);
            DwmApi.SetDarkTitleBar(source.Handle, IsDark);
        };
    }

    public static Color WindowBackgroundColor => (Color)Application.Current.Resources["WindowBackgroundColor"];

    private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(() =>
        {
            var dark = IsSystemAppModeDark();
            if (dark == IsDark)
            {
                return;
            }

            ApplyTheme(dark);
            foreach (Window window in Application.Current.Windows)
            {
                var hwnd = new WindowInteropHelper(window).Handle;
                if (hwnd != IntPtr.Zero)
                {
                    DwmApi.SetDarkTitleBar(hwnd, dark);
                }
            }
        });
    }

    private static void ApplyTheme(bool dark)
    {
        IsDark = dark;

        var dictionaries = Application.Current.Resources.MergedDictionaries;
        if (themeBrushes != null)
        {
            _ = dictionaries.Remove(themeBrushes);
        }

        themeBrushes = new ResourceDictionary { Source = dark ? DarkThemeUri : LightThemeUri };
        dictionaries.Add(themeBrushes);
    }

    private static bool IsSystemAppModeDark()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
        return key?.GetValue("AppsUseLightTheme") is 0;
    }
}
