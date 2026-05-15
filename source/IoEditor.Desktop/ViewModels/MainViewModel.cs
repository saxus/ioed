using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using IoEditor.Desktop.Hosting;
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
    private readonly ISettingsUiPresenter _settingsUi;
    private readonly IOptionsMonitor<StudioOptions> _studioOptions;
    private readonly IOptions<StudioOptions> _optionsSnapshot;
    private readonly IConfigurationReloader _configReloader;
    private readonly SettingsPanelViewModel _settingsPanel;

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
        ISettingsUiPresenter settingsUi,
        IOptionsMonitor<StudioOptions> studioOptions,
        IOptions<StudioOptions> optionsSnapshot,
        IConfigurationReloader configReloader)
    {
        _partLibrary = partLibrary;
        _colorLibrary = colorLibrary;
        _imageProxyFactory = imageProxyFactory;
        _loaderDialog = loaderDialog;
        _filePicker = filePicker;
        _dialogs = dialogs;
        _appLifetime = appLifetime;
        _settingsUi = settingsUi;
        _studioOptions = studioOptions;
        _optionsSnapshot = optionsSnapshot;
        _configReloader = configReloader;

        ExitCommand = new DelegateCommand(_ => _appLifetime.Shutdown());
        OpenSettingsPanelCommand = new DelegateCommand(_ => OpenOrFocusSettingsPanel());

        _settingsPanel = new SettingsPanelViewModel(
            _optionsSnapshot,
            ApplicationPaths.GetConfigFilePath(),
            _filePicker,
            _dialogs,
            GetMainWindow);
        _settingsPanel.Saved += _ => _configReloader.Reload();

        var startPanel = new StartPanelViewModel();
        startPanel.OpenRequested += OpenFilesAsync;
        RegisterPanel(startPanel);
        SelectedPanel = startPanel;
    }

    // -----------------------------------------------------------------------
    // Public panel operations (called by App and command-line handler)
    // -----------------------------------------------------------------------

    /// <summary>Creates a new project panel, loads the project, and selects the panel.</summary>
    public void OpenProjectPanel(string reference, string target)
    {
        var panel = new ProjectPanelViewModel(
            _partLibrary, _colorLibrary, _imageProxyFactory,
            _filePicker, _dialogs, _studioOptions, GetMainWindow);

        panel.CloseRequested += ClosePanel;

        RegisterPanel(panel);
        SelectedPanel = panel;

        try
        {
            panel.LoadProject(reference, target);
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

    private async void OpenFilesAsync()
    {
        var owner = GetMainWindow();
        if (owner is null)
        {
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

    public event PropertyChangedEventHandler? PropertyChanged;

    private void RaisePropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
