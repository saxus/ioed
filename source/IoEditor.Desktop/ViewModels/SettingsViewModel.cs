using Avalonia.Controls;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using IoEditor.Desktop.Services;
using IoEditor.Platform;
using IoEditor.Desktop.Utils;
using IoEditor.Models.Configuration;
using Microsoft.Extensions.Options;

namespace IoEditor.Desktop.ViewModels;

internal sealed class SettingsViewModel : INotifyPropertyChanged
{
    private readonly StudioOptions _options;
    private readonly string _configFilePath;
    private readonly IFilePickerService _files;
    private readonly IDialogService _dialogs;
    private readonly IThemeService _themeService;
    private Window? _owner;

    public SettingsViewModel(
        IOptions<StudioOptions> options,
        string configFilePath,
        IFilePickerService files,
        IDialogService dialogs,
        IThemeService themeService)
    {
        _options = options.Value;
        _configFilePath = configFilePath;
        _files = files;
        _dialogs = dialogs;
        _themeService = themeService;
        _studioFolderPath = _options.StudioFolder ?? string.Empty;
        _showXmlDebugTabs = _options.ShowXmlDebugTabs;
        _themeMode = _options.ThemeMode;
        BrowseStudioFolderCommand = new DelegateCommand(BrowseStudioFolder);
        SaveCommand = new DelegateCommand(Save);
        CancelCommand = new DelegateCommand(Cancel);
    }

    public void SetOwner(Window owner) => _owner = owner;

    private bool _showXmlDebugTabs;
    public bool ShowXmlDebugTabs
    {
        get => _showXmlDebugTabs;
        set
        {
            if (_showXmlDebugTabs != value)
            {
                _showXmlDebugTabs = value;
                RaisePropertyChanged(nameof(ShowXmlDebugTabs));
            }
        }
    }

    private string _studioFolderPath;
    public string StudioFolderPath
    {
        get => _studioFolderPath;
        set
        {
            if (_studioFolderPath != value)
            {
                _studioFolderPath = value;
                RaisePropertyChanged(nameof(StudioFolderPath));
            }
        }
    }

    private AppThemeMode _themeMode;
    public AppThemeMode ThemeMode
    {
        get => _themeMode;
        set
        {
            if (_themeMode != value)
            {
                _themeMode = value;
                RaisePropertyChanged(nameof(ThemeMode));
            }
        }
    }

    public AppThemeMode[] ThemeModes { get; } = Enum.GetValues<AppThemeMode>();

    public ICommand BrowseStudioFolderCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public bool WasSaved { get; private set; }

    public event Action? RequestClose;

    private async void BrowseStudioFolder(object? _)
    {
        if (_owner is null)
        {
            return;
        }

        var selectedPath = await _files.PickStudioInstallRootAsync(_owner);
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

    private void Save(object? _)
    {
        _options.StudioFolder = StudioFolderPath;
        _options.ShowXmlDebugTabs = ShowXmlDebugTabs;
        _options.ThemeMode = ThemeMode;
        var config = new { StudioOptions = _options };
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configFilePath, json);
        _themeService.Apply(_options.ThemeMode);
        WasSaved = true;
        RequestClose?.Invoke();
    }

    private void Cancel(object? _)
    {
        WasSaved = false;
        RequestClose?.Invoke();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
