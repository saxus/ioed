using System.Collections.ObjectModel;
using System.Windows.Input;
using IoEditor.Desktop.Models;
using IoEditor.Desktop.Utils;

namespace IoEditor.Desktop.ViewModels.Panels;

/// <summary>Panel shown on startup; provides the entry point to open a project.</summary>
internal sealed class StartPanelViewModel : EditorPanelViewModelBase
{
    public override string Title => "Home";

    /// <summary>The home panel is always present and cannot be closed by the user.</summary>
    public override bool IsClosable => false;

    public ObservableCollection<RecentProjectItemViewModel> RecentProjects { get; } = new();

    public bool HasRecentProjects => RecentProjects.Count > 0;

    public ICommand OpenCommand { get; }

    /// <summary>Raised when the user triggers Open; the shell handles the dialog and project loading.</summary>
    public event Action? OpenRequested;

    public event Action<string, string>? RecentOpenRequested;

    public event Action<string, string>? RecentRemoveRequested;

    public StartPanelViewModel()
    {
        OpenCommand = new DelegateCommand(_ => OpenRequested?.Invoke());
    }

    public void SetRecentProjects(IEnumerable<RecentProjectEntry> entries)
    {
        RecentProjects.Clear();
        foreach (var entry in entries)
        {
            RecentProjects.Add(new RecentProjectItemViewModel(entry, OpenRecentProject, RemoveRecentProject));
        }

        RaisePropertyChanged(nameof(HasRecentProjects));
    }

    private void OpenRecentProject(RecentProjectItemViewModel item)
        => RecentOpenRequested?.Invoke(item.ReferencePath, item.TargetPath);

    private void RemoveRecentProject(RecentProjectItemViewModel item)
        => RecentRemoveRequested?.Invoke(item.ReferencePath, item.TargetPath);
}
