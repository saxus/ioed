using IoEditor.Models.Configuration;

namespace IoEditor.Platform;

internal sealed class MacosStudioDefaultPathProvider : IStudioDefaultPathProvider
{
    private const string DefaultStudioRoot = "/Applications/Studio 2.0";

    public string? GetDefaultStudioRoot() =>
        Directory.Exists(DefaultStudioRoot) ? DefaultStudioRoot : null;
}
