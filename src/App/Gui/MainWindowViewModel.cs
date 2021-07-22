using ShowWhatProcessLocksFile.Gui.Controls;
using ShowWhatProcessLocksFile.Gui.Utils;
using ShowWhatProcessLocksFile.LockingProcessesInfo;
using ShowWhatProcessLocksFile.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShowWhatProcessLocksFile.Gui
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public string Title => $"{AssemblyInfo.ProgramName} {AssemblyInfo.InformationalVersion}";

        public RelayCommand RefreshCommand { get; }

        public string FilePath { get; }

        private ViewModelBase mainControl;

        public ViewModelBase MainControl
        {
            get => mainControl;
            set
            {
                mainControl = value;
                OnPropertyChanged();
                RelayCommand.Refresh();
            }
        }

        public MainWindowViewModel(string filePath)
        {
            FilePath = filePath;
            RefreshCommand = new RelayCommand(GetLockingInformation, () => !(mainControl is ProgressBarWithTextViewModel));
            GetLockingInformation();
        }

        public async void GetLockingInformation()
        {
            MainControl = new ProgressBarWithTextViewModel($"Getting locking information");

            try
            {
                var res = await Task.Run(() => ProcessesInfoRetriever.GetProcessesInfo(FilePath));
                if (res.Any())
                {
                    MainControl = new ProcessInfoListViewModel(res, OnProcessesKillRequested);
                }
                else
                {
                    MainControl = ResultTextViewModel.Info("Nothing locks this file");
                }
            }
            catch (Exception ex)
            {
                MainControl = ResultTextViewModel.Error($"Failed to get locking information. Exception:\n{ex}");
            }
        }

        public async void OnProcessesKillRequested(IEnumerable<ProcessInfo> processesToKill)
        {
            MainControl = new ProgressBarWithTextViewModel($"Killing processes");

            try
            {
                await Task.Run(() => ProcessKiller.Kill(processesToKill));
            }
            catch (Exception ex)
            {
                MainControl = ResultTextViewModel.Error($"Failed to kill processes. Exception:\n{ex}");
            }

            GetLockingInformation();
        }
    }
}
