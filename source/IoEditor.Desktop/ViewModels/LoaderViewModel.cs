using Avalonia.Controls;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using IoEditor.Platform;
using IoEditor.Desktop.Utils;

namespace IoEditor.Desktop.ViewModels;

internal sealed class LoaderViewModel : INotifyPropertyChanged
{
    private readonly IFilePickerService _files;
    private readonly IDialogService _dialogs;
    private Window? _owner;

    private string? _referenceFile;
    public string? ReferenceFile
    {
        get => _referenceFile;
        set
        {
            if (_referenceFile != value)
            {
                _referenceFile = value;
                RaisePropertyChanged(nameof(ReferenceFile));
            }
        }
    }

    private string? _targetFile;
    public string? TargetFile
    {
        get => _targetFile;
        set
        {
            if (_targetFile != value)
            {
                _targetFile = value;
                RaisePropertyChanged(nameof(TargetFile));
            }
        }
    }

    public bool WasConfirmed { get; private set; }

    public event Action? RequestClose;

    public ICommand OpenCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand BrowseReferenceFileCommand { get; }
    public ICommand BrowseTargetFileCommand { get; }

    public LoaderViewModel(IFilePickerService files, IDialogService dialogs)
    {
        _files = files;
        _dialogs = dialogs;
        OpenCommand = new DelegateCommand(Open);
        CancelCommand = new DelegateCommand(Cancel);
        BrowseReferenceFileCommand = new DelegateCommand(BrowseReferenceFile);
        BrowseTargetFileCommand = new DelegateCommand(BrowseTargetFile);
    }

    public void SetOwner(Window owner) => _owner = owner;

    private async void Open(object? _)
    {
        if (!File.Exists(ReferenceFile))
        {
            await _dialogs.ShowErrorAsync($"Reference file does not exist: {ReferenceFile}");
            return;
        }

        if (!File.Exists(TargetFile))
        {
            await _dialogs.ShowErrorAsync($"Target file does not exist: {TargetFile}");
            return;
        }

        if (string.Equals(ReferenceFile, TargetFile, StringComparison.OrdinalIgnoreCase))
        {
            await _dialogs.ShowErrorAsync("Reference file and Target file cannot be the same.");
            return;
        }

        WasConfirmed = true;
        RequestClose?.Invoke();
    }

    private void Cancel(object? _)
    {
        WasConfirmed = false;
        RequestClose?.Invoke();
    }

    private async void BrowseReferenceFile(object? _)
    {
        if (_owner is null)
        {
            return;
        }

        var path = await _files.PickOpenIoFileAsync(_owner);
        if (!string.IsNullOrEmpty(path))
        {
            ReferenceFile = path;
        }
    }

    private async void BrowseTargetFile(object? _)
    {
        if (_owner is null)
        {
            return;
        }

        var path = await _files.PickOpenIoFileAsync(_owner);
        if (!string.IsNullOrEmpty(path))
        {
            TargetFile = path;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
