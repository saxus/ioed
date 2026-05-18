using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using IoEditor.Desktop.Hosting;
using IoEditor.Desktop.Models;
using IoEditor.Desktop.Services;
using IoEditor.Desktop.Utils;
using IoEditor.Desktop.ViewModels.Panels;
using IoEditor.Models.Configuration;
using IoEditor.Models.ImageCache;
using IoEditor.Models.Studio;
using IoEditor.Platform;
using Microsoft.Extensions.Options;

namespace IoEditor.Desktop.ViewModels;

/// <summary>Shell view-model: manages the open panel list and delegates per-panel work to panel view-models.</summary>
internal sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly PartLibrary _partLibrary;
    private readonly ColorLibrary _colorLibrary;
    private readonly IPartImageProxyFactory _imageProxyFactory;
    private readonly ILoaderDialogPresenter _loaderDialog;
    private readonly IFilePickerService _filePicker;
    private readonly IDialogService _dialogs;
    private readonly IAppLifetime _appLifetime;
    private readonly IOptionsMonitor<StudioOptions> _studioOptions;
    private readonly IOptions<StudioOptions> _optionsSnapshot;
    private readonly IConfigurationReloader _configReloader;
    private readonly IThemeService _themeService;
    private readonly SettingsPanelViewModel _settingsPanel;
    private readonly IRecentProjectsStore _recentProjectsStore;
    private readonly List<RecentProjectEntry> _recentProjects;
    private readonly StartPanelViewModel _startPanel;

    public ObservableCollection<EditorPanelViewModelBase> OpenPanels { get; } = new();

    private EditorPanelViewModelBase? _selectedPanel;
    public EditorPanelViewModelBase? SelectedPanel
    {
        get => _selectedPanel;
        set
        {
            if (_selectedPanel == value)
            {
                return;
            }

            if (_selectedPanel is not null)
            {
                _selectedPanel.IsActive = false;
            }

            _selectedPanel = value;

            if (_selectedPanel is not null)
            {
                _selectedPanel.IsActive = true;
            }

            RaisePropertyChanged(nameof(SelectedPanel));
            RaisePropertyChanged(nameof(WindowTitle));
            RaisePropertyChanged(nameof(IsSettingsPanelActive));
        }
    }

    public ICommand ExitCommand { get; }
    public ICommand OpenSettingsPanelCommand { get; }

    public bool IsSettingsPanelActive => _selectedPanel is SettingsPanelViewModel;

    public string WindowTitle
        => _selectedPanel?.Title is { Length: > 0 } t ? $"IoEditor – {t}" : "IoEditor";

    public MainViewModel(
        PartLibrary partLibrary,
        ColorLibrary colorLibrary,
        IPartImageProxyFactory imageProxyFactory,
        ILoaderDialogPresenter loaderDialog,
        IFilePickerService filePicker,
        IDialogService dialogs,
        IAppLifetime appLifetime,
        IOptionsMonitor<StudioOptions> studioOptions,
        IOptions<StudioOptions> optionsSnapshot,
        IConfigurationReloader configReloader,
        IThemeService themeService,
        IRecentProjectsStore recentProjectsStore,
        IPartImageSourceSelector imageSourceSelector)
    {
        _partLibrary = partLibrary;
        _colorLibrary = colorLibrary;
        _imageProxyFactory = imageProxyFactory;
        _loaderDialog = loaderDialog;
        _filePicker = filePicker;
        _dialogs = dialogs;
        _appLifetime = appLifetime;
        _studioOptions = studioOptions;
        _optionsSnapshot = optionsSnapshot;
        _configReloader = configReloader;
        _themeService = themeService;
        _recentProjectsStore = recentProjectsStore;
        _recentProjects = _recentProjectsStore.Load().ToList();

        ExitCommand = new DelegateCommand(_ => _appLifetime.Shutdown());
        OpenSettingsPanelCommand = new DelegateCommand(_ => OpenOrFocusSettingsPanel());

        _settingsPanel = new SettingsPanelViewModel(
            _optionsSnapshot,
            ApplicationPaths.GetConfigFilePath(),
            _filePicker,
            _dialogs,
            _themeService,
            GetMainWindow,
            imageSourceSelector);
        _settingsPanel.Saved += _ => _configReloader.Reload();

        _startPanel = new StartPanelViewModel();
        _startPanel.OpenRequested += OpenFilesAsync;
        _startPanel.RecentOpenRequested += OpenRecentProjectAsync;
        _startPanel.RecentRemoveRequested += RemoveRecentProject;
        _startPanel.SetRecentProjects(_recentProjects);
        RegisterPanel(_startPanel);
        SelectedPanel = _startPanel;

        if (!ConfigurationValidator.Validate(_optionsSnapshot.Value))
        {
            OpenOrFocusSettingsPanel();
        }
    }

    // -----------------------------------------------------------------------
    // Public panel operations (called by App and command-line handler)
    // -----------------------------------------------------------------------

    /// <summary>Creates a new project panel, loads the project, and selects the panel.</summary>
    public void OpenProjectPanel(string reference, string target)
    {
        if (!ConfigurationValidator.Validate(_studioOptions.CurrentValue))
        {
            _ = PromptConfigureStudioIfNeededAsync();
            return;
        }

        var panel = new ProjectPanelViewModel(
            _partLibrary, _colorLibrary, _imageProxyFactory,
            _filePicker, _dialogs, _studioOptions, GetMainWindow);

        panel.CloseRequested += ClosePanel;

        RegisterPanel(panel);
        SelectedPanel = panel;

        try
        {
            panel.LoadProject(reference, target);
            AddRecentProject(reference, target);
        }
        catch
        {
            ClosePanel(panel);
            throw;
        }
    }

    /// <summary>Selects the permanent settings panel.</summary>
    public void OpenOrFocusSettingsPanel()
    {
        SelectedPanel = _settingsPanel;
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    private void RegisterPanel(EditorPanelViewModelBase panel)
    {
        panel.SelectCommand = new DelegateCommand(_ => SelectedPanel = panel);
        OpenPanels.Add(panel);
    }

    private void ClosePanel(EditorPanelViewModelBase panel)
    {
        var idx = OpenPanels.IndexOf(panel);
        OpenPanels.Remove(panel);

        if (SelectedPanel == panel)
        {
            SelectedPanel = idx > 0
                ? OpenPanels[Math.Min(idx, OpenPanels.Count) - 1]
                : OpenPanels.FirstOrDefault();
        }
    }

    private void AddRecentProject(string reference, string target)
    {
        var entry = new RecentProjectEntry
        {
            ReferencePath = NormalizePath(reference),
            TargetPath = NormalizePath(target)
        };

        _recentProjects.RemoveAll(existing => IsSameProject(existing, entry.ReferencePath, entry.TargetPath));
        _recentProjects.Insert(0, entry);

        if (_recentProjects.Count > _recentProjectsStore.MaxEntries)
        {
            _recentProjects.RemoveRange(_recentProjectsStore.MaxEntries, _recentProjects.Count - _recentProjectsStore.MaxEntries);
        }

        SaveAndRefreshRecentProjects();
    }

    private async void OpenRecentProjectAsync(string reference, string target)
    {
        try
        {
            if (!ConfigurationValidator.Validate(_studioOptions.CurrentValue))
            {
                await PromptConfigureStudioIfNeededAsync();
                return;
            }

            OpenProjectPanel(reference, target);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync($"Error opening files: {ex.Message}");
        }
    }

    private void RemoveRecentProject(string reference, string target)
    {
        _recentProjects.RemoveAll(existing => IsSameProject(existing, reference, target));
        SaveAndRefreshRecentProjects();
    }

    private void SaveAndRefreshRecentProjects()
    {
        try
        {
            _recentProjectsStore.Save(_recentProjects);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }

        _startPanel.SetRecentProjects(_recentProjects);
    }

    private static bool IsSameProject(RecentProjectEntry entry, string reference, string target)
        => string.Equals(NormalizePath(entry.ReferencePath), NormalizePath(reference), StringComparison.OrdinalIgnoreCase)
           && string.Equals(NormalizePath(entry.TargetPath), NormalizePath(target), StringComparison.OrdinalIgnoreCase);

    private static string NormalizePath(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch
        {
            return path;
        }
    }

    private async void OpenFilesAsync()
    {
        var owner = GetMainWindow();
        if (owner is null)
        {
            return;
        }

        if (!ConfigurationValidator.Validate(_studioOptions.CurrentValue))
        {
            await PromptConfigureStudioIfNeededAsync();
            return;
        }

        var pick = await _loaderDialog.ShowAsync(owner);
        if (pick is null)
        {
            return;
        }

        try
        {
            OpenProjectPanel(pick.Value.ReferencePath, pick.Value.TargetPath);
        }
        catch (Exception ex)
        {
            await _dialogs.ShowErrorAsync($"Error opening files: {ex.Message}");
        }
    }

    private static Window? GetMainWindow()
        => (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
           ?.MainWindow as Window;

    private async Task PromptConfigureStudioIfNeededAsync()
    {
        await _dialogs.ShowInfoAsync(
            "Please configure the Studio folder in Settings before opening files.",
            "Studio not configured");
        OpenOrFocusSettingsPanel();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

