using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShowWhatProcessLocksFile.Gui.Controls;
using ShowWhatProcessLocksFile.LockFinding;
using ShowWhatProcessLocksFile.LockFinding.Utils;
using ShowWhatProcessLocksFile.Utils;

namespace ShowWhatProcessLocksFile.Gui;

internal sealed partial class MainWindowViewModel : ObservableObject
{
    public bool IsAdministrator { get; } = Elevation.IsUserAnAdmin();
    public string Title => $"{AssemblyInfo.ProgramName} {AssemblyInfo.InformationalVersion}{(IsAdministrator ? " (Admin)" : "")}";
    public CanonicalPath? FilePath { get; }
    private ObservableObject? mainControl;

    public ObservableObject? MainControl
    {
        get => mainControl;
        private set
        {
            (mainControl as IDisposable)?.Dispose();
            mainControl = value;
            OnPropertyChanged();
            RefreshCommand.NotifyCanExecuteChanged();
        }
    }

    public MainWindowViewModel(CanonicalPath filePath)
    {
        FilePath = filePath;
        _ = RefreshCommand.ExecuteAsync(null);
    }

    private bool CanRestartAsAdministrator() => !IsAdministrator;

    [RelayCommand(CanExecute = nameof(CanRestartAsAdministrator))]
    private void RestartAsAdministrator()
    {
        try
        {
            Elevation.RestartAsAdmin(FilePath!);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            // Cancelling the UAC prompt leaves the current window open.
        }
        catch (Exception ex)
        {
            MainControl = ResultTextViewModel.Error($"Failed to restart as administrator.\n{ex.Message}");
        }
    }

    private bool CanRefresh() => MainControl is not ProgressBarWithTextViewModel;

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        MainControl = new ProgressBarWithTextViewModel("Getting locking information");

        try
        {
            var result = await Task.Run(() => LockFinder.FindWhatProcessesLockPath(FilePath!).ToList());
            MainControl = result.Count > 0
                ? new ProcessInfoListViewModel(result, OnProcessesKillRequested)
                : ResultTextViewModel.Info("Nothing locks this file or folder");
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
            return;
        }

        await RefreshAsync();
    }
}
