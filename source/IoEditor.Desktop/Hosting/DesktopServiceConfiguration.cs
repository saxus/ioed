using IoEditor.Desktop.Services;
using IoEditor.Desktop.ViewModels;

using Microsoft.Extensions.DependencyInjection;

namespace IoEditor.Desktop.Hosting;

internal static class DesktopServiceConfiguration
{
    public static void AddDesktopUiServices(this IServiceCollection services)
    {
        services.AddSingleton<IDialogService, AvaloniaDialogService>();
        services.AddSingleton<IFilePickerService, AvaloniaFilePickerService>();
        services.AddSingleton<IAppLifetime, AvaloniaAppLifetime>();
        services.AddSingleton<ILoaderDialogPresenter, LoaderDialogPresenter>();
        services.AddSingleton<MainViewModel>();
    }
}
