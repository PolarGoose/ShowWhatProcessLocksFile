using System.Windows.Input;

namespace ShowWhatProcessLocksFile.Gui.Utils;

public class RelayCommand : ICommand
{
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    private readonly Action methodToExecute;
    private readonly Func<bool>? canExecuteEvaluator;

    public RelayCommand(Action methodToExecute, Func<bool>? canExecuteEvaluator = null)
    {
        this.methodToExecute = methodToExecute;
        this.canExecuteEvaluator = canExecuteEvaluator;
    }

    public bool CanExecute(object parameter)
    {
        return canExecuteEvaluator == null || canExecuteEvaluator();
    }

    public void Execute(object parameter)
    {
        methodToExecute();
    }

    public static void Refresh()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}

public class RelayCommand<T> : ICommand
{
    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    private readonly Action<T> methodToExecute;
    private readonly Predicate<T>? canExecuteEvaluator;

    public RelayCommand(Action<T> methodToExecute, Predicate<T>? canExecuteEvaluator = null)
    {
        this.methodToExecute = methodToExecute;
        this.canExecuteEvaluator = canExecuteEvaluator;
    }

    public bool CanExecute(object parameter)
    {
        return canExecuteEvaluator == null || canExecuteEvaluator((T)parameter);
    }

    public void Execute(object parameter)
    {
        methodToExecute((T)parameter);
    }

    public static void Refresh()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}
