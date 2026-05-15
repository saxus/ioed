using IoEditor.Models.Configuration;

namespace IoEditor.Platform;

internal sealed class WindowsStudioDefaultPathProvider : IStudioDefaultPathProvider
{
    private static readonly string[] Candidates =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "LEGO Company", "BrickLink Studio 2.0"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "LEGO Company", "BrickLink Studio 2.0"),
    ];

    public string? GetDefaultStudioRoot() =>
        Candidates.FirstOrDefault(Directory.Exists);
}
