namespace IoEditor.Desktop.Services;

/// <summary>Reloads the application configuration after a settings save.</summary>
internal interface IConfigurationReloader
{
    void Reload();
}
