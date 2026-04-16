using Avalonia.Controls;

namespace IoEditor.Desktop.Services;

internal readonly record struct LoaderPickResult(string ReferencePath, string TargetPath);

internal interface ILoaderDialogPresenter
{
    Task<LoaderPickResult?> ShowAsync(Window owner);
}
