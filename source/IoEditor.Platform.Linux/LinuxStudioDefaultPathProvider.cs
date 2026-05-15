using IoEditor.Models.Configuration;

namespace IoEditor.Platform;

internal sealed class LinuxStudioDefaultPathProvider : IStudioDefaultPathProvider
{
    public string? GetDefaultStudioRoot()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var path = Path.Combine(home, ".wine", "drive_c", "Program Files", "Studio 2.0");
        return Directory.Exists(path) ? path : null;
    }
}
