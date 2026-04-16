using Avalonia.Controls;
using IoEditor.Desktop.ViewModels;
using IoEditor.Desktop.Views;
using IoEditor.Platform;

namespace IoEditor.Desktop.Services;

internal sealed class LoaderDialogPresenter : ILoaderDialogPresenter
{
    private readonly IFilePickerService _files;
    private readonly IDialogService _dialogs;

    public LoaderDialogPresenter(IFilePickerService files, IDialogService dialogs)
    {
        _files = files;
        _dialogs = dialogs;
    }

    public async Task<LoaderPickResult?> ShowAsync(Window owner)
    {
        var vm = new LoaderViewModel(_files, _dialogs);
        var win = new LoaderWindow { DataContext = vm };
        vm.SetOwner(win);
        vm.RequestClose += win.Close;
        await win.ShowDialog(owner);
        if (vm.WasConfirmed && !string.IsNullOrEmpty(vm.ReferenceFile) && !string.IsNullOrEmpty(vm.TargetFile))
        {
            return new LoaderPickResult(vm.ReferenceFile!, vm.TargetFile!);
        }

        return null;
    }
}
