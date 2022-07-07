using System.Windows.Input;

namespace SampleBlog.Web.Client.Core;

public sealed class DelegateCommand : ICommand
{
    private readonly Action callback;
    private readonly Func<bool> canExecute;
    private bool? canRun;

    public event EventHandler? CanExecuteChanged;

    public DelegateCommand(Action callback, Func<bool>? canExecute = null)
    {
        this.callback = callback;
        this.canExecute = canExecute ?? Always;
    }

    public bool CanExecute(object? parameter) => canExecute.Invoke();

    public void Execute(object? parameter)
    {
        var result = CanExecute(parameter);

        if (false == canRun.HasValue || canRun != result)
        {
            canRun = result;
            RaiseCanExecuteChanged(EventArgs.Empty);
        }

        if (canRun.Value)
        {
            callback.Invoke();
        }
    }

    private void RaiseCanExecuteChanged(EventArgs args)
    {
        CanExecuteChanged?.Invoke(this, args);
    }

    private static bool Always() => true;
}

public sealed class DelegateCommand<T> : ICommand
{
    private readonly Action<T> callback;
    private readonly Predicate<T> canExecute;
    private bool? canRun;

    public event EventHandler? CanExecuteChanged;

    public DelegateCommand(Action<T> callback, Predicate<T>? canExecute = null)
    {
        this.callback = callback;
        this.canExecute = canExecute ?? Always;
    }

    public bool CanExecute(object? parameter) => canExecute.Invoke((T?)parameter!);

    public void Execute(object? parameter)
    {
        if (parameter is T arg)
        {
            var result = CanExecute(parameter);

            if (false == canRun.HasValue || canRun != result)
            {
                canRun = result;
                RaiseCanExecuteChanged(EventArgs.Empty);
            }

            if (canRun.Value)
            {
                callback.Invoke(arg);
            }
        }
        else
        {
            throw new ArgumentException("Wrong type", nameof(parameter));
        }
    }

    private void RaiseCanExecuteChanged(EventArgs args)
    {
        CanExecuteChanged?.Invoke(this, args);
    }

    private static bool Always(T _) => true;
}