using Avalonia.Controls;

namespace IoEditor.Platform;

/// <summary>Default no-op for platforms that keep the in-window <see cref="Menu"/>.</summary>
public sealed class NullMainWindowMenuIntegration : IMainWindowMenuIntegration
{
    public static NullMainWindowMenuIntegration Instance { get; } = new();

    public void AttachMainMenu(Window mainWindow, Menu mainMenu)
    {
    }
}
