using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using IoEditor.Desktop.Hosting;
using IoEditor.Desktop.Services;
using IoEditor.Desktop.ViewModels;
using IoEditor.Desktop.Views;
using IoEditor.Models.Configuration;
using IoEditor.Models.ImageCache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace IoEditor.Desktop;

public partial class App : Application
{
    private IHost? _host;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        var (host, configPath) = DesktopHostFactory.CreateHost();
        _host = host;
        var services = host.Services;

        var bg = services.GetRequiredService<BackgroundPartImageLoader>();
        _ = Task.Run(() => bg.StartAsync(CancellationToken.None));

        var options = services.GetRequiredService<IOptions<StudioOptions>>().Value;
        if (!ConfigurationValidator.Validate(options))
        {
            var settingsVm = new SettingsViewModel(
                services.GetRequiredService<IOptions<StudioOptions>>(),
                configPath,
                services.GetRequiredService<IFilePickerService>(),
                services.GetRequiredService<IDialogService>());
            var settingsWin = new SettingsWindow { DataContext = settingsVm };
            settingsVm.SetOwner(settingsWin);
            var settingsClosed = new TaskCompletionSource();
            settingsWin.Closed += (_, _) => settingsClosed.TrySetResult();
            settingsVm.RequestClose += () => settingsWin.Close();
            settingsWin.Show();
            settingsClosed.Task.GetAwaiter().GetResult();
            if (!settingsVm.WasSaved)
            {
                desktop.Shutdown();
                base.OnFrameworkInitializationCompleted();
                return;
            }

            DesktopHostFactory.ReloadConfiguration(host);
        }

        var main = new MainWindow
        {
            DataContext = services.GetRequiredService<MainViewModel>()
        };
        desktop.MainWindow = main;

        desktop.ShutdownRequested += async (_, _) =>
        {
            try
            {
                if (_host is not null)
                {
                    var loader = _host.Services.GetService<BackgroundPartImageLoader>();
                    if (loader != null)
                    {
                        await loader.StopAsync(CancellationToken.None);
                    }
                }
            }
            catch
            {
                // ignore
            }

            _host?.Dispose();
            _host = null;
        };

        main.Opened += (_, __) =>
        {
            var args = desktop.Args ?? Array.Empty<string>();
            if (args.Length >= 3 && args[0].Equals("/openfiles", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    services.GetRequiredService<MainViewModel>().OpenFiles(args[1], args[2]);
                }
                catch (Exception ex)
                {
                    _ = services.GetRequiredService<IDialogService>().ShowErrorAsync($"Error opening files: {ex.Message}");
                }
            }
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                Console.WriteLine($"Unhandled: {ex}");
            }
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            Console.WriteLine($"Unobserved task: {e.Exception}");
            e.SetObserved();
        };

        base.OnFrameworkInitializationCompleted();
    }
}
