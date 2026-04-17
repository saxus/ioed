using Microsoft.Extensions.DependencyInjection;

namespace IoEditor.Platform;

public static class PlatformUiRegistration
{
    public static void AddPlatformUi(IServiceCollection services)
    {
        services.AddSingleton<IDialogService, LinuxDialogService>();
        services.AddSingleton<IFilePickerService, LinuxFilePickerService>();
        services.AddSingleton<IMainWindowMenuIntegration>(_ => NullMainWindowMenuIntegration.Instance);
    }
}
