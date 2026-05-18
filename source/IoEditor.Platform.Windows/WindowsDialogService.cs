using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace IoEditor.Platform;

public sealed class WindowsDialogService : IDialogService
{
    private const uint MbOk = 0;
    private const uint MbIconerror = 0x00000010;
    private const uint MbIconinformation = 0x00000040;
    private const uint MbYesno       = 0x00000004;
    private const uint MbYesnocancel = 0x00000003;
    private const uint MbIconquestion = 0x00000020;
    private const int Idyes = 6;
    private const int Idno  = 7;

    public Task ShowErrorAsync(string message, string title = "Error")
    {
        MessageBoxW(GetOwnerHwnd(), message, title, MbOk | MbIconerror);
        return Task.CompletedTask;
    }

    public Task ShowInfoAsync(string message, string title = "")
    {
        var caption = string.IsNullOrEmpty(title) ? "IoEditor" : title;
        MessageBoxW(GetOwnerHwnd(), message, caption, MbOk | MbIconinformation);
        return Task.CompletedTask;
    }

    public Task<bool> ConfirmAsync(string message, string title = "Confirm")
    {
        var r = MessageBoxW(GetOwnerHwnd(), message, title, MbYesno | MbIconquestion);
        return Task.FromResult(r == Idyes);
    }

    public Task<SaveConfirmResult> ShowSaveConfirmAsync(string message, string title = "Unsaved changes")
    {
        var r = MessageBoxW(GetOwnerHwnd(), message, title, MbYesnocancel | MbIconquestion);
        return Task.FromResult(r switch
        {
            Idyes => SaveConfirmResult.Save,
            Idno  => SaveConfirmResult.Discard,
            _     => SaveConfirmResult.Cancel
        });
    }

    private static IntPtr GetOwnerHwnd()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return IntPtr.Zero;
        }

        if (desktop.MainWindow is not Window w)
        {
            return IntPtr.Zero;
        }

        return w.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int MessageBoxW(IntPtr hWnd, string? lpText, string? lpCaption, uint uType);
}
