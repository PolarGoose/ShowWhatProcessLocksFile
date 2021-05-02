

namespace ShowWhatProcessLocksFile.Gui.Controls
{
    internal class ProgressBarWithTextViewModel : ViewModelBase
    {
        public string Text { get; }

        public ProgressBarWithTextViewModel(string text)
        {
            Text = text;
        }

    }
}
