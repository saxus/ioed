using IoEditor.Desktop.Services;
using IoEditor.Desktop.ViewModels;
using IoEditor.Platform;

using Microsoft.Extensions.DependencyInjection;

namespace IoEditor.Desktop.Hosting;

internal static class DesktopServiceConfiguration
{
    public static void AddDesktopUiServices(this IServiceCollection services)
    {
        PlatformUiRegistration.AddPlatformUi(services);
        services.AddSingleton<IAppLifetime, AvaloniaAppLifetime>();
        services.AddSingleton<ILoaderDialogPresenter, LoaderDialogPresenter>();
        services.AddSingleton<ISettingsUiPresenter, SettingsUiPresenter>();
        services.AddSingleton<MainViewModel>();
    }
}
