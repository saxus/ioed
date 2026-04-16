using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace IoEditor.Platform;

public sealed class LinuxFilePickerService : IFilePickerService
{
    public async Task<string?> PickOpenIoFileAsync(Window owner, string title = "Open stud.io file")
    {
        var top = TopLevel.GetTopLevel(owner);
        if (top?.StorageProvider is null)
        {
            return null;
        }

        var ioType = new FilePickerFileType("stud.io file") { Patterns = new[] { "*.io" } };
        var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = new[] { ioType }
        });

        return files.Count > 0 ? files[0].TryGetLocalPath() : null;
    }

    public async Task<string?> PickSaveIoFileAsync(Window owner, string title = "Save As")
    {
        var top = TopLevel.GetTopLevel(owner);
        if (top?.StorageProvider is null)
        {
            return null;
        }

        var file = await top.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = title,
            DefaultExtension = "io",
            FileTypeChoices = new[] { new FilePickerFileType("Studio file") { Patterns = new[] { "*.io" } } }
        });

        return file?.TryGetLocalPath();
    }

    public async Task<string?> PickStudioInstallRootAsync(Window owner)
    {
        var top = TopLevel.GetTopLevel(owner);
        if (top?.StorageProvider is null)
        {
            return null;
        }

        var folders = await top.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select LEGO Studio installation folder",
            AllowMultiple = false
        });

        if (folders.Count == 0)
        {
            return null;
        }

        return folders[0].TryGetLocalPath();
    }
}
