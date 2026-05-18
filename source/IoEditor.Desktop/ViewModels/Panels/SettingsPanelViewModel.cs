using System.IO;
using System.Text.Json;
using System.Windows.Input;
using Avalonia.Controls;
using IoEditor.Desktop.Hosting;
using IoEditor.Desktop.Services;
using IoEditor.Desktop.Utils;
using IoEditor.Models.Configuration;
using IoEditor.Models.ImageCache;
using IoEditor.Models.Studio;
using IoEditor.Platform;
using Microsoft.Extensions.Options;

namespace IoEditor.Desktop.ViewModels.Panels;

internal enum SettingsSection { General, Studio, Images }

/// <summary>Permanent settings panel; always available via the nav rail, never closed.</summary>
internal sealed class SettingsPanelViewModel : EditorPanelViewModelBase
{
    private readonly StudioOptions _options;
    private readonly string _configFilePath;
    private readonly IFilePickerService _files;
    private readonly IDialogService _dialogs;
    private readonly Func<Window?> _getMainWindow;
    private readonly IThemeService _themeService;
    private readonly IPartImageSourceSelector _imageSourceSelector;

    public override string Title => "Settings";
    public override bool IsClosable => false;

    // ── Section navigation ───────────────────────────────────────────────────

    public SettingsSection[] Sections { get; } = Enum.GetValues<SettingsSection>();

    private SettingsSection _selectedSection = SettingsSection.General;
    public SettingsSection SelectedSection
    {
        get => _selectedSection;
        set
        {
            if (_selectedSection == value) return;
            _selectedSection = value;
            RaisePropertyChanged(nameof(SelectedSection));
            RaisePropertyChanged(nameof(IsGeneralSelected));
            RaisePropertyChanged(nameof(IsStudioSelected));
            RaisePropertyChanged(nameof(IsImagesSelected));
            RaisePropertyChanged(nameof(ShowSaveButton));
            if (value == SettingsSection.Images)
                _ = RefreshCacheSizeAsync();
        }
    }

    public bool IsGeneralSelected => _selectedSection == SettingsSection.General;
    public bool IsStudioSelected  => _selectedSection == SettingsSection.Studio;
    public bool IsImagesSelected  => _selectedSection == SettingsSection.Images;
    public bool ShowSaveButton    => true;

    // ── General ──────────────────────────────────────────────────────────────

    private bool _showXmlDebugTabs;
    public bool ShowXmlDebugTabs
    {
        get => _showXmlDebugTabs;
        set
        {
            if (_showXmlDebugTabs == value) return;
            _showXmlDebugTabs = value;
            RaisePropertyChanged(nameof(ShowXmlDebugTabs));
        }
    }

    private AppThemeMode _themeMode;
    public AppThemeMode ThemeMode
    {
        get => _themeMode;
        set
        {
            if (_themeMode == value) return;
            _themeMode = value;
            RaisePropertyChanged(nameof(ThemeMode));
        }
    }

    public AppThemeMode[] ThemeModes { get; } = Enum.GetValues<AppThemeMode>();

    // ── Studio ───────────────────────────────────────────────────────────────

    private string _studioFolderPath;
    public string StudioFolderPath
    {
        get => _studioFolderPath;
        set
        {
            if (_studioFolderPath == value) return;
            _studioFolderPath = value;
            RaisePropertyChanged(nameof(StudioFolderPath));
        }
    }

    // ── Images ───────────────────────────────────────────────────────────────

    public IReadOnlyList<IPartImageSource> AvailableSources => _imageSourceSelector.AvailableSources;

    public IPartImageSource ActiveSource
    {
        get => _imageSourceSelector.ActiveSource;
        set
        {
            if (_imageSourceSelector.ActiveSource == value) return;
            _imageSourceSelector.ActiveSource = value;
            RaisePropertyChanged(nameof(ActiveSource));
            _ = RefreshCacheSizeAsync();
        }
    }

    private double _cacheSizeMb;
    public double CacheSizeMb
    {
        get => _cacheSizeMb;
        private set
        {
            if (_cacheSizeMb == value) return;
            _cacheSizeMb = value;
            RaisePropertyChanged(nameof(CacheSizeMb));
        }
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    public ICommand SaveCommand { get; }
    public ICommand BrowseStudioFolderCommand { get; }
    public ICommand ClearCacheCommand { get; }

    /// <summary>Raised after settings have been successfully written to disk.</summary>
    public event Action<SettingsPanelViewModel>? Saved;

    // ── Constructor ──────────────────────────────────────────────────────────

    public SettingsPanelViewModel(
        IOptions<StudioOptions> options,
        string configFilePath,
        IFilePickerService files,
        IDialogService dialogs,
        IThemeService themeService,
        Func<Window?> getMainWindow,
        IPartImageSourceSelector imageSourceSelector)
    {
        _options = options.Value;
        _configFilePath = configFilePath;
        _files = files;
        _dialogs = dialogs;
        _themeService = themeService;
        _getMainWindow = getMainWindow;
        _imageSourceSelector = imageSourceSelector;

        _studioFolderPath = _options.StudioFolder ?? string.Empty;
        _showXmlDebugTabs = _options.ShowXmlDebugTabs;
        _themeMode = _options.ThemeMode;

        SaveCommand = new DelegateCommand(Save);
        BrowseStudioFolderCommand = new DelegateCommand(BrowseStudioFolder);
        ClearCacheCommand = new DelegateCommand(async _ => await ClearCacheAsync());
    }

    // ── Save ─────────────────────────────────────────────────────────────────

    private void Save(object? _)
    {
        _options.StudioFolder = StudioFolderPath;
        _options.ShowXmlDebugTabs = ShowXmlDebugTabs;
        _options.ThemeMode = ThemeMode;
        _options.ImageSourceName = ActiveSource.Name;
        var config = new { StudioOptions = _options };
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_configFilePath, json);
        _themeService.Apply(_options.ThemeMode);
        Saved?.Invoke(this);
    }

    // ── Browse ───────────────────────────────────────────────────────────────

    private async void BrowseStudioFolder(object? _)
    {
        var owner = _getMainWindow();
        if (owner is null) return;

        var selectedPath = await _files.PickStudioInstallRootAsync(owner);
        if (string.IsNullOrEmpty(selectedPath)) return;

        if (StudioInstallationProbe.IsValidStudioRoot(selectedPath, out var error))
            StudioFolderPath = selectedPath;
        else
            await _dialogs.ShowErrorAsync(error ?? "Invalid LEGO Studio installation folder.");
    }

    // ── Cache helpers ────────────────────────────────────────────────────────

    private async Task RefreshCacheSizeAsync()
    {
        var bytes = await _imageSourceSelector.ActiveSource.GetCacheSizeBytesAsync();
        CacheSizeMb = Math.Round(bytes / (1024.0 * 1024.0), 1);
    }

    private async Task ClearCacheAsync()
    {
        await _imageSourceSelector.ActiveSource.ClearCacheAsync();
        await RefreshCacheSizeAsync();
    }
}
