using System.IO;
using System.Windows.Input;
using IoEditor.Desktop.Models;
using IoEditor.Desktop.Utils;

namespace IoEditor.Desktop.ViewModels.Panels;

internal sealed class RecentProjectItemViewModel
{
    public RecentProjectItemViewModel(
        RecentProjectEntry entry,
        Action<RecentProjectItemViewModel> open,
        Action<RecentProjectItemViewModel> remove)
    {
        ReferencePath = entry.ReferencePath;
        TargetPath = entry.TargetPath;
        ReferenceExists = File.Exists(ReferencePath);
        TargetExists = File.Exists(TargetPath);
        OpenCommand = new DelegateCommand(_ => open(this));
        RemoveCommand = new DelegateCommand(_ => remove(this));
    }

    public string ReferencePath { get; }

    public string TargetPath { get; }

    public string ReferenceFileName => Path.GetFileName(ReferencePath);

    public string TargetFileName => Path.GetFileName(TargetPath);

    public string DisplayName => $"{ReferenceFileName} -> {TargetFileName}";

    public bool ReferenceExists { get; }

    public bool ReferenceMissing => !ReferenceExists;

    public bool TargetExists { get; }

    public bool TargetMissing => !TargetExists;

    public ICommand OpenCommand { get; }

    public ICommand RemoveCommand { get; }
}
