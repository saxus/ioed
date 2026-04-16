namespace IoEditor.Desktop.Hosting;

internal static class ApplicationPaths
{
    public static string GetConfigFolderPath()
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IoEditor");

    public static string GetConfigFilePath()
        => Path.Combine(GetConfigFolderPath(), "appsettings.json");
}
