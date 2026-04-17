using Avalonia.Controls;

namespace IoEditor.Platform;

/// <summary>Platform-specific wiring for the main window menu (e.g. macOS system menu bar).</summary>
public interface IMainWindowMenuIntegration
{
    /// <summary>Attach platform behavior to <paramref name="mainMenu"/>; safe to call once per window.</summary>
    void AttachMainMenu(Window mainWindow, Menu mainMenu);
}
