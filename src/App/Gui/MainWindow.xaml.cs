using System.Windows;
using ShowWhatProcessLocksFile.Gui.Theme;
using ShowWhatProcessLocksFile.Gui.Utils;
using ShowWhatProcessLocksFile.Utils;

namespace ShowWhatProcessLocksFile.Gui;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ThemeManager.Attach(this);

        try
        {
            DataContext = new MainWindowViewModel(CommandLineParser.GetFileFullName());
        }
        catch (Exception ex)
        {
            ErrorDialog.Show($"{ex.Message}");
            Application.Current.Shutdown();
        }
    }
}
