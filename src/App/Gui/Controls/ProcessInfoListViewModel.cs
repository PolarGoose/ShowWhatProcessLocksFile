using ShowWhatProcessLocksFile.Gui.Utils;
using ShowWhatProcessLocksFile.LockFinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShowWhatProcessLocksFile.Gui.Controls
{
    internal class ProcessInfoListViewModel : ViewModelBase
    {
        public IEnumerable<ProcessInfoViewModel> ProcessInfoViewModels { get; }
        public RelayCommand ExpandAllCommand { get; }
        public RelayCommand CollapseAllCommand { get; }
        public RelayCommand KillAll { get; }
        public RelayCommand<System.Collections.IEnumerable> KillSelected { get; }

        public ProcessInfoListViewModel(IEnumerable<ProcessInfo> processesInfoViewModels, Action<IEnumerable<ProcessInfo>> killProcessesRequested)
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

            KillAll = new RelayCommand(() =>
            {
                killProcessesRequested(ProcessInfoViewModels.Select(p => p.Process));
            });

            KillSelected = new RelayCommand<System.Collections.IEnumerable>(processes =>
            {
                killProcessesRequested(processes.OfType<ProcessInfoViewModel>().Select(p => p.Process));
            });
        }
    }
}
