using System.Text.Json;
using IoEditor.Models.Configuration;
using IoEditor.Platform;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IoEditor.Desktop.Hosting;

internal static class DesktopHostFactory
{
    public static (IHost Host, string ConfigFilePath) CreateHost()
    {
        var configFolder = ApplicationPaths.GetConfigFolderPath();
        var configFile = ApplicationPaths.GetConfigFilePath();
        Directory.CreateDirectory(configFolder);

        if (!File.Exists(configFile))
        {
            var studioOptions = new StudioOptions { StudioFolder = string.Empty };
            StudioInstallationProbe.TryApplySuggestedRootIfEmpty(studioOptions, PlatformUiRegistration.CreateDefaultPathProvider());
            var defaultConfig = new { StudioOptions = studioOptions };
            File.WriteAllText(configFile, JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true }));
        }

        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((_, config) =>
            {
                config.AddJsonFile(configFile, optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddIoEditorServices(context.Configuration);
                services.AddDesktopUiServices();
            })
            .Build();

        return (host, configFile);
    }

    public static void ReloadConfiguration(IHost host)
    {
        if (host.Services.GetService<IConfiguration>() is IConfigurationRoot root)
        {
            root.Reload();
        }
    }
}
