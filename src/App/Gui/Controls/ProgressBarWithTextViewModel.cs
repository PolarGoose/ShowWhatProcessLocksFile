using CommunityToolkit.Mvvm.ComponentModel;

namespace ShowWhatProcessLocksFile.Gui.Controls;

internal class ProgressBarWithTextViewModel : ObservableObject
{
    public string Text { get; }

    public ProgressBarWithTextViewModel(string text)
    {
        Text = text;
    }
}
