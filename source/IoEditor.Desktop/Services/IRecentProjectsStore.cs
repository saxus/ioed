using IoEditor.Desktop.Models;

namespace IoEditor.Desktop.Services;

internal interface IRecentProjectsStore
{
    int MaxEntries { get; }

    IReadOnlyList<RecentProjectEntry> Load();

    void Save(IEnumerable<RecentProjectEntry> entries);
}
