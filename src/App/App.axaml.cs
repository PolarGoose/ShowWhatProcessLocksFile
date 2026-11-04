using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ShowWhatProcessLocksFile.Gui;
using ShowWhatProcessLocksFile.Utils;

namespace ShowWhatProcessLocksFile;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var lifetime = (IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!;
        var filePath = CommandLineParser.GetFileFullName(lifetime.Args!);
        lifetime.MainWindow = new MainWindow { DataContext = new MainWindowViewModel(filePath) };
        base.OnFrameworkInitializationCompleted();
    }
}
