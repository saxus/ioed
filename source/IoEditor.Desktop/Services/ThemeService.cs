using Avalonia;
using Avalonia.Styling;
using IoEditor.Models.Configuration;

namespace IoEditor.Desktop.Services;

internal sealed class ThemeService : IThemeService
{
    public void Apply(AppThemeMode themeMode)
    {
        if (Application.Current is null)
        {
            return;
        }

        Application.Current.RequestedThemeVariant = themeMode switch
        {
            AppThemeMode.Light => ThemeVariant.Light,
            AppThemeMode.Dark => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };
    }
}
