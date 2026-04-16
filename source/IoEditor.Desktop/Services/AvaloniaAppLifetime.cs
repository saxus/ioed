using Avalonia.Controls.ApplicationLifetimes;

namespace IoEditor.Desktop.Services;

internal sealed class AvaloniaAppLifetime : IAppLifetime
{
    public void Shutdown()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }
}
