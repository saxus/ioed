using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using IoEditor.Desktop.Hosting;
using IoEditor.Desktop.Services;
using IoEditor.Desktop.ViewModels;
using IoEditor.Desktop.Views;
using IoEditor.Platform;
using IoEditor.Models.Configuration;
using IoEditor.Models.ImageCache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

        var (host, _) = DesktopHostFactory.CreateHost();
        _host = host;
        var services = host.Services;

        WireAboutNativeMenu(services.GetRequiredService<IDialogService>());

        var bg = services.GetRequiredService<BackgroundPartImageLoader>();
        _ = Task.Run(() => bg.StartAsync(CancellationToken.None));

        var options = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<StudioOptions>>().Value;
        services.GetRequiredService<IThemeService>().Apply(options.ThemeMode);
        if (!ConfigurationValidator.Validate(options))
        {
            var saved = services.GetRequiredService<ISettingsUiPresenter>().ShowAsync(null).GetAwaiter().GetResult();
            if (!saved)
            {
                desktop.Shutdown();
                base.OnFrameworkInitializationCompleted();
                return;
            }
        }

        var mainVm = services.GetRequiredService<MainViewModel>();
        var main = new MainWindow(services.GetRequiredService<IMainWindowMenuIntegration>())
        {
            DataContext = mainVm
        };

        desktop.MainWindow = main;
        WireSettingsNativeMenu(mainVm, main);

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
                    mainVm.OpenProjectPanel(args[1], args[2]);
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

    private void WireSettingsNativeMenu(MainViewModel mainVm, Window ownerWindow)
    {
        if (NativeMenu.GetMenu(this) is not NativeMenu appMenu)
        {
            return;
        }

        foreach (var o in appMenu.Items)
        {
            if (o is NativeMenuItem item && string.Equals(item.Header?.ToString(), "Settings…", StringComparison.Ordinal))
            {
                item.Click += (_, _) => mainVm.OpenOrFocusSettingsPanel();
                break;
            }
        }
    }

    private void WireAboutNativeMenu(IDialogService dialogs)
    {
        if (NativeMenu.GetMenu(this) is not NativeMenu appMenu)
        {
            return;
        }

        foreach (var o in appMenu.Items)
        {
            if (o is NativeMenuItem about && string.Equals(about.Header?.ToString(), "About IoEditor", StringComparison.Ordinal))
            {
                about.Click += (_, _) => _ = dialogs.ShowInfoAsync("IO instruction merge editor.", "About IoEditor");
                break;
            }
        }
    }
}
