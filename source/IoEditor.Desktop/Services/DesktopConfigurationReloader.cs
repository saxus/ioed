using IoEditor.Desktop.Hosting;
using Microsoft.Extensions.Hosting;

namespace IoEditor.Desktop.Services;

internal sealed class DesktopConfigurationReloader : IConfigurationReloader
{
    private readonly IHost _host;

    public DesktopConfigurationReloader(IHost host)
    {
        _host = host;
    }

    public void Reload() => DesktopHostFactory.ReloadConfiguration(_host);
}
