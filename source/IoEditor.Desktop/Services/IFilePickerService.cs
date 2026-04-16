using Avalonia.Controls;

namespace IoEditor.Desktop.Services;

internal interface IFilePickerService
{
    /// <summary>Pick a .io stud.io file.</summary>
    Task<string?> PickOpenIoFileAsync(Window owner, string title = "Open stud.io file");

    /// <summary>Pick save path for .io file.</summary>
    Task<string?> PickSaveIoFileAsync(Window owner, string title = "Save As");

    /// <summary>LEGO Studio install root: Windows via studio.exe picker; macOS/Linux via folder picker.</summary>
    Task<string?> PickStudioInstallRootAsync(Window owner);
}
