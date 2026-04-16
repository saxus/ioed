using Avalonia.Threading;

namespace IoEditor.Platform;

public sealed class MacosDialogService : IDialogService
{
    public async Task ShowErrorAsync(string message, string title = "Error")
        => await Dispatcher.UIThread.InvokeAsync(() => AppKitInterop.RunError(message, title));

    public async Task ShowInfoAsync(string message, string title = "")
        => await Dispatcher.UIThread.InvokeAsync(() => AppKitInterop.RunInformational(message, title));

    public async Task<bool> ConfirmAsync(string message, string title = "Confirm")
        => await Dispatcher.UIThread.InvokeAsync(() => AppKitInterop.RunConfirm(message, title));
}
