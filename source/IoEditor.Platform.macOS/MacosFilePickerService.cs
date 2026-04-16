using Avalonia.Controls;
using Avalonia.Threading;

namespace IoEditor.Platform;

public sealed class MacosFilePickerService : IFilePickerService
{
    public async Task<string?> PickOpenIoFileAsync(Window owner, string title = "Open stud.io file")
        => await Dispatcher.UIThread.InvokeAsync(() => AppKitInterop.RunOpenIoPanel(title));

    public async Task<string?> PickSaveIoFileAsync(Window owner, string title = "Save As")
        => await Dispatcher.UIThread.InvokeAsync(() => AppKitInterop.RunSaveIoPanel(title));

    public async Task<string?> PickStudioInstallRootAsync(Window owner)
        => await Dispatcher.UIThread.InvokeAsync(() =>
            AppKitInterop.RunOpenFolderPanel("Select LEGO Studio installation folder"));
}
