using IoEditor.Models.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IoEditor.Platform;

public static class PlatformUiRegistration
{
    public static void AddPlatformUi(IServiceCollection services)
    {
        services.AddSingleton<IDialogService, MacosDialogService>();
        services.AddSingleton<IFilePickerService, MacosFilePickerService>();
        services.AddSingleton<IMainWindowMenuIntegration, MacosMainWindowMenuIntegration>();
        services.AddSingleton<IStudioDefaultPathProvider, MacosStudioDefaultPathProvider>();
    }

    public static IStudioDefaultPathProvider CreateDefaultPathProvider()
        => new MacosStudioDefaultPathProvider();
}
