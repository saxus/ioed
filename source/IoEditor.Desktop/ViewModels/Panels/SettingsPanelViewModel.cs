using System.IO;
using System.Text.Json;
using System.Windows.Input;
using Avalonia.Controls;
using IoEditor.Desktop.Hosting;
using IoEditor.Desktop.Utils;
using IoEditor.Models.Configuration;
using IoEditor.Models.Studio;
using IoEditor.Platform;
using Microsoft.Extensions.Options;

namespace IoEditor.Desktop.ViewModels.Panels;

/// <summary>Permanent settings panel; always available via the nav rail, never closed.</summary>
internal sealed class SettingsPanelViewModel : EditorPanelViewModelBase
{
    private readonly StudioOptions _options;
    private readonly string _configFilePath;
    private readonly IFilePickerService _files;
    private readonly IDialogService _dialogs;
    private readonly Func<Window?> _getMainWindow;

    public override string Title => "Settings";

    /// <summary>Settings is a permanent panel that cannot be closed.</summary>
    public override bool IsClosable => false;

    private bool _showXmlDebugTabs;
    public bool ShowXmlDebugTabs
    {
        get => _showXmlDebugTabs;
        set
        {
            if (_showXmlDebugTabs == value)
            {
                return;
            }

            _showXmlDebugTabs = value;
            RaisePropertyChanged(nameof(ShowXmlDebugTabs));
        }
    }

    private string _studioFolderPath;
    public string StudioFolderPath
    {
        get => _studioFolderPath;
        set
        {
            if (_studioFolderPath == value)
            {
                return;
            }

            _studioFolderPath = value;
            RaisePropertyChanged(nameof(StudioFolderPath));
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand BrowseStudioFolderCommand { get; }

    /// <summary>Raised after the settings have been successfully written to disk.</summary>
    public event Action<SettingsPanelViewModel>? Saved;

    public SettingsPanelViewModel(
        IOptions<StudioOptions> options,
        string configFilePath,
        IFilePickerService files,
        IDialogService dialogs,
        Func<Window?> getMainWindow)
    {
        _options = options.Value;
        _configFilePath = configFilePath;
        _files = files;
        _dialogs = dialogs;
        _getMainWindow = getMainWindow;
        _studioFolderPath = _options.StudioFolder ?? string.Empty;
        _showXmlDebugTabs = _options.ShowXmlDebugTabs;

        SaveCommand = new DelegateCommand(Save);
        BrowseStudioFolderCommand = new DelegateCommand(BrowseStudioFolder);
    }

    private void Save(object? _)
    {
        _options.StudioFolder = StudioFolderPath;
        _options.ShowXmlDebugTabs = ShowXmlDebugTabs;
        var config = new { StudioOptions = _options };
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configFilePath, json);
        Saved?.Invoke(this);
    }

    private async void BrowseStudioFolder(object? _)
    {
        var owner = _getMainWindow();
        if (owner is null)
        {
            return;
        }

        var selectedPath = await _files.PickStudioInstallRootAsync(owner);
        if (string.IsNullOrEmpty(selectedPath))
        {
            return;
        }

        if (StudioInstallationProbe.IsValidStudioRoot(selectedPath, out var error))
        {
            StudioFolderPath = selectedPath;
        }
        else
        {
            await _dialogs.ShowErrorAsync(error ?? "Invalid LEGO Studio installation folder.");
        }
    }
}
