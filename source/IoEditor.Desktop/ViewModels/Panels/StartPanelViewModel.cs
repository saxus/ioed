using System.Windows.Input;
using IoEditor.Desktop.Utils;

namespace IoEditor.Desktop.ViewModels.Panels;

/// <summary>Panel shown on startup; provides the entry point to open a project.</summary>
internal sealed class StartPanelViewModel : EditorPanelViewModelBase
{
    public override string Title => "Home";

    /// <summary>The home panel is always present and cannot be closed by the user.</summary>
    public override bool IsClosable => false;

    public ICommand OpenCommand { get; }

    /// <summary>Raised when the user triggers Open; the shell handles the dialog and project loading.</summary>
    public event Action? OpenRequested;

    public StartPanelViewModel()
    {
        OpenCommand = new DelegateCommand(_ => OpenRequested?.Invoke());
    }
}
