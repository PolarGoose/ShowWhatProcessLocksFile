using ShowWhatProcessLocksFile.Gui.Theme;
using System.Windows;

namespace ShowWhatProcessLocksFile;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ThemeManager.Initialize();
    }
}
