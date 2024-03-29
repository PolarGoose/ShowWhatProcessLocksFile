using ShowWhatProcessLocksFile.Gui.Utils;
using ShowWhatProcessLocksFile.LockFinding;

namespace ShowWhatProcessLocksFile.Gui.Controls;

internal class ProcessInfoViewModel : ViewModelBase
{
    public ProcessInfo Process { get; }

    private bool isExpanded;

    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            isExpanded = value;
            OnPropertyChanged();
        }
    }

    public ProcessInfoViewModel(ProcessInfo process)
    {
        Process = process;
    }
}
