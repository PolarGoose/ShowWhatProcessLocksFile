namespace ShowWhatProcessLocksFile.Gui.Controls
{
    internal class ResultTextViewModel : ViewModelBase
    {
        public string Text { get; }
        public bool Success { get; }

        private ResultTextViewModel(string text, bool success)
        {
            Text = text;
            Success = success;
        }

        public static ResultTextViewModel Error(string text)
        {
            return new ResultTextViewModel(text, success: false);
        }

        public static ResultTextViewModel Info(string text)
        {
            return new ResultTextViewModel(text, success: true);
        }
    }
}
