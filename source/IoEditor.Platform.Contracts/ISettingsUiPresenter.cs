using Avalonia.Controls;

namespace IoEditor.Platform;

public interface ISettingsUiPresenter
{
    /// <summary>Shows settings. Returns true if the user saved (config was written).</summary>
    Task<bool> ShowAsync(Window? owner);
}
