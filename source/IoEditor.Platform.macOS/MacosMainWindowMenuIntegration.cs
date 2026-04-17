using System;
using Avalonia.Controls;

namespace IoEditor.Platform;

/// <summary>Maps the XAML <see cref="Menu"/> to the macOS system menu bar and hides the in-window menu.</summary>
public sealed class MacosMainWindowMenuIntegration : IMainWindowMenuIntegration
{
    private bool _hooked;

    public void AttachMainMenu(Window mainWindow, Menu mainMenu)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        if (_hooked)
        {
            return;
        }

        _hooked = true;

        var nativeBar = new NativeMenu();
        var nativeMenuInstalled = false;

        void Refresh(object? _, EventArgs __)
        {
            mainMenu.IsVisible = false;
            MacosMenuToNativeMenuBar.RebuildBar(nativeBar, mainMenu);
            if (!nativeMenuInstalled)
            {
                NativeMenu.SetMenu(mainWindow, nativeBar);
                nativeMenuInstalled = true;
            }
        }

        mainWindow.Opened += Refresh;
        mainWindow.DataContextChanged += Refresh;
        Refresh(null, EventArgs.Empty);
    }
}
