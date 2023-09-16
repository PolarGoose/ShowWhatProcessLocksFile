using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ShowWhatProcessLocksFile.Gui.Controls;
using ShowWhatProcessLocksFile.Gui.Utils;
using ShowWhatProcessLocksFile.LockFinding;
using ShowWhatProcessLocksFile.Utils;

namespace ShowWhatProcessLocksFile.Gui;

internal class MainWindowViewModel : ViewModelBase
{
    public string Title => $"""{AssemblyInfo.ProgramName} {AssemblyInfo.InformationalVersion}{(Elevation.IsUserAnAdmin() ? " (Admin)" : "")}""";

    public RelayCommand RefreshCommand { get; }

    public RelayCommand RestartAsAdministratorCommand { get; }

    public string FilePath { get; }

    private ViewModelBase mainControl;

    public ViewModelBase MainControl
    {
        get => mainControl;
        private set
        {
            mainControl = value;
            OnPropertyChanged();
            RelayCommand.Refresh();
        }
    }

    public MainWindowViewModel(string filePath)
    {
        FilePath = filePath;
        RefreshCommand = new RelayCommand(GetLockingInformation, () => mainControl is not ProgressBarWithTextViewModel);
        RestartAsAdministratorCommand = new RelayCommand(RestartAsAdministrator, () => !Elevation.IsUserAnAdmin());

        GetLockingInformation();
    }

    private void RestartAsAdministrator()
    {
        Elevation.RestartAsAdmin(FilePath);
    }

    private async void GetLockingInformation()
    {
        MainControl = new ProgressBarWithTextViewModel("Getting locking information");

        try
        {
            var res = await Task.Run(() => LockFinder.FindWhatProcessesLockPath(FilePath).ToList());
            MainControl = res.Any()
                ? new ProcessInfoListViewModel(res, OnProcessesKillRequested)
                : ResultTextViewModel.Info("Nothing locks this file");
        }
        catch (Exception ex)
        {
            MainControl = ResultTextViewModel.Error($"Failed to get locking information. Exception:\n{ex}");
        }
    }

    private async void OnProcessesKillRequested(IEnumerable<ProcessInfo> processesToKill)
    {
        MainControl = new ProgressBarWithTextViewModel("Killing processes");

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
