using ShowWhatProcessLocksFile.Gui.Utils;
using ShowWhatProcessLocksFile.Utils;
using System;
using System.Windows;

namespace ShowWhatProcessLocksFile.Gui
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

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
}
