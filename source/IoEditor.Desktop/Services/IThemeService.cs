using IoEditor.Models.Configuration;

namespace IoEditor.Desktop.Services;

internal interface IThemeService
{
    void Apply(AppThemeMode themeMode);
}
