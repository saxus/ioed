using System.Text.Json;
using IoEditor.Desktop.Models;

namespace IoEditor.Desktop.Services;

internal sealed class RecentProjectsStore : IRecentProjectsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public int MaxEntries => 10;

    public RecentProjectsStore(string filePath)
    {
        _filePath = filePath;
    }

    public IReadOnlyList<RecentProjectEntry> Load()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<RecentProjectEntry>>(json, JsonOptions)?
                .Where(entry => !string.IsNullOrWhiteSpace(entry.ReferencePath)
                                && !string.IsNullOrWhiteSpace(entry.TargetPath))
                .Take(MaxEntries)
                .ToList()
                ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    public void Save(IEnumerable<RecentProjectEntry> entries)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(entries.Take(MaxEntries).ToList(), JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
