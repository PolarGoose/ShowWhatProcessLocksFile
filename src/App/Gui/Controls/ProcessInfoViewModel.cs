using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ShowWhatProcessLocksFile.LockFinding;

namespace ShowWhatProcessLocksFile.Gui.Controls;

internal sealed class ProcessInfoViewModel : ObservableObject, IDisposable
{
    public ProcessInfo Process { get; }
    public Bitmap? Icon { get; }

    public ProcessInfoViewModel(ProcessInfo process)
    {
        Process = process;
        if (process.Icon != null)
        {
            using var stream = new MemoryStream(process.Icon);
            Icon = new Bitmap(stream);
        }
    }

    private bool isExpanded;

    public void Dispose() => Icon?.Dispose();

    public bool IsExpanded
    {
        get => isExpanded;
        set => SetProperty(ref isExpanded, value);
    }
}
