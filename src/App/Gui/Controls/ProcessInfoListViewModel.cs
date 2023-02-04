using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ShowWhatProcessLocksFile.Gui.Utils;
using ShowWhatProcessLocksFile.LockFinding;

namespace ShowWhatProcessLocksFile.Gui.Controls;

internal class ProcessInfoListViewModel : ViewModelBase
{
    public IEnumerable<ProcessInfoViewModel> ProcessInfoViewModels { get; }
    public RelayCommand ExpandAllCommand { get; }
    public RelayCommand CollapseAllCommand { get; }
    public RelayCommand<IEnumerable> KillSelected { get; }

    public ProcessInfoListViewModel(IEnumerable<ProcessInfo> processesInfoViewModels,
        Action<IEnumerable<ProcessInfo>> killProcessesRequested)
    {
        ProcessInfoViewModels = processesInfoViewModels.Select(p => new ProcessInfoViewModel(p)).ToList();

        ExpandAllCommand = new RelayCommand(() =>
        {
            foreach (var p in ProcessInfoViewModels)
            {
                p.IsExpanded = true;
            }
        });

        CollapseAllCommand = new RelayCommand(() =>
        {
            foreach (var p in ProcessInfoViewModels)
            {
                p.IsExpanded = false;
            }
        });

        KillSelected = new RelayCommand<IEnumerable>(processes =>
        {
            killProcessesRequested(processes.OfType<ProcessInfoViewModel>().Select(p => p.Process));
        });
    }
}
