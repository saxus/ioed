using System.Windows.Input;

namespace IoEditor.Desktop.Utils;

internal sealed class AsyncDelegateCommand : ICommand
{
    private readonly Func<object?, Task> _execute;
    private bool _isExecuting;

    public AsyncDelegateCommand(Func<object?, Task> execute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public bool CanExecute(object? parameter) => !_isExecuting;

    public async void Execute(object? parameter)
    {
        if (_isExecuting) return;
        _isExecuting = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try
        {
            await _execute(parameter);
        }
        finally
        {
            _isExecuting = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler? CanExecuteChanged;
}
