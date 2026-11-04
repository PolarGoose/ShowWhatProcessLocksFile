using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using ShowWhatProcessLocksFile.LockFinding;

namespace ShowWhatProcessLocksFile.Gui.Controls;

internal sealed class ProcessInfoListViewModel(
    IEnumerable<ProcessInfo> processes, Action<IEnumerable<ProcessInfo>> killProcessesRequested) : ObservableObject, IDisposable
{
    public IReadOnlyList<ProcessInfoViewModel> ProcessInfoViewModels { get; } =
        processes.Select(p => new ProcessInfoViewModel(p)).ToList();

    public void ExpandAll() => SetExpanded(true);

    public void CollapseAll() => SetExpanded(false);

    public void KillSelected(IEnumerable? selection)
    {
        var selected = selection?.OfType<ProcessInfoViewModel>().Select(p => p.Process).ToArray() ?? [];
        if (selected.Length > 0)
        {
            killProcessesRequested(selected);
        }
    }

    private void SetExpanded(bool expanded)
    {
        foreach (var process in ProcessInfoViewModels)
        {
            process.IsExpanded = expanded;
        }
    }

    public void Dispose()
    {
        foreach (var process in ProcessInfoViewModels)
        {
            process.Dispose();
        }
    }
}
