using Avalonia.Controls;
using Avalonia.Platform.Storage;
using IoEditor.Models.Configuration;
using System.Runtime.InteropServices;

namespace IoEditor.Desktop.Services;

internal sealed class AvaloniaFilePickerService : IFilePickerService
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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var exeType = new FilePickerFileType("Studio Executable") { Patterns = new[] { "studio.exe" } };
            var files = await top.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Select studio.exe (in your LEGO Studio folder)",
                AllowMultiple = false,
                FileTypeFilter = new[] { exeType }
            });

            if (files.Count == 0)
            {
                return null;
            }

            var path = files[0].TryGetLocalPath();
            return string.IsNullOrEmpty(path) ? null : Path.GetDirectoryName(path);
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
