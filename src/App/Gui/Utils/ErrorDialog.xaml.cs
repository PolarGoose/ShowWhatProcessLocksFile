using ShowWhatProcessLocksFile.Gui.Theme;
using System.Media;
using System.Windows;

namespace ShowWhatProcessLocksFile.Gui.Utils;

public partial class ErrorDialog : Window
{
    public static void Show(string text)
    {
        var dialog = new ErrorDialog { Message = { Text = text } };

        // A Window can only own another Window once it has been shown; startup errors happen before that.
        if (Application.Current.MainWindow is { IsVisible: true } owner)
        {
            dialog.Owner = owner;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        SystemSounds.Hand.Play();
        _ = dialog.ShowDialog();
    }

    private ErrorDialog()
    {
        InitializeComponent();
        ThemeManager.Attach(this);
    }
}
