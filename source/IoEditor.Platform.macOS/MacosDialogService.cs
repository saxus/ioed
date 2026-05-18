using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace IoEditor.Platform;

public sealed class MacosDialogService : IDialogService
{
    private static Window? GetOwner()
        => (Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow as Window;

    public Task ShowErrorAsync(string message, string title = "Error")
        => SimpleMessageBox.ShowAsync(GetOwner(), message, title);

    public Task ShowInfoAsync(string message, string title = "")
        => SimpleMessageBox.ShowAsync(GetOwner(), message, string.IsNullOrEmpty(title) ? "IoEditor" : title);

    public Task<bool> ConfirmAsync(string message, string title = "Confirm")
        => SimpleMessageBox.ConfirmAsync(GetOwner(), message, title);

    public Task<SaveConfirmResult> ShowSaveConfirmAsync(string message, string title = "Unsaved changes")
        => SimpleMessageBox.SaveConfirmAsync(GetOwner(), message, title);
}
