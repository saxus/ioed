using Avalonia.Controls;
using IoEditor.Desktop.Hosting;
using IoEditor.Desktop.ViewModels;
using IoEditor.Desktop.Views;
using IoEditor.Models.Configuration;
using IoEditor.Platform;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace IoEditor.Desktop.Services;

internal sealed class SettingsUiPresenter : ISettingsUiPresenter
{
    private readonly IHost _host;
    private readonly IOptions<StudioOptions> _options;
    private readonly IFilePickerService _files;
    private readonly IDialogService _dialogs;
    private readonly IThemeService _themeService;

    public SettingsUiPresenter(
        IHost host,
        IOptions<StudioOptions> options,
        IFilePickerService files,
        IDialogService dialogs,
        IThemeService themeService)
    {
        _host = host;
        _options = options;
        _files = files;
        _dialogs = dialogs;
        _themeService = themeService;
    }

    public async Task<bool> ShowAsync(Window? owner)
    {
        var configPath = ApplicationPaths.GetConfigFilePath();
        var vm = new SettingsViewModel(_options, configPath, _files, _dialogs, _themeService);
        var win = new SettingsWindow { DataContext = vm };
        vm.SetOwner(win);
        vm.RequestClose += () => win.Close();

        if (owner is not null)
        {
            await win.ShowDialog(owner);
        }
        else
        {
            win.Show();
            var closed = new TaskCompletionSource();
            win.Closed += (_, _) => closed.TrySetResult();
            await closed.Task;
        }

        if (vm.WasSaved)
        {
            DesktopHostFactory.ReloadConfiguration(_host);
        }

        return vm.WasSaved;
    }
}
