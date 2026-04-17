using Microsoft.Extensions.DependencyInjection;

namespace IoEditor.Platform;

public static class PlatformUiRegistration
{
    public static void AddPlatformUi(IServiceCollection services)
    {
        services.AddSingleton<IDialogService, WindowsDialogService>();
        services.AddSingleton<IFilePickerService, WindowsFilePickerService>();
        services.AddSingleton<IMainWindowMenuIntegration>(_ => NullMainWindowMenuIntegration.Instance);
    }
}
