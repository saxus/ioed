using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using IoEditor.Desktop.Models;
using IoEditor.Desktop.Utils;

namespace IoEditor.Desktop.ViewModels.Panels;

internal sealed class RecentProjectItemViewModel : INotifyPropertyChanged
{
    private Bitmap? _thumbnailImage;
    private bool _isThumbnailLoaded;

    public RecentProjectItemViewModel(
        RecentProjectEntry entry,
        Action<RecentProjectItemViewModel> open,
        Action<RecentProjectItemViewModel> remove)
    {
        ReferencePath = entry.ReferencePath;
        TargetPath = entry.TargetPath;
        ReferenceExists = File.Exists(ReferencePath);
        TargetExists = File.Exists(TargetPath);
        OpenCommand = new DelegateCommand(_ => open(this));
        RemoveCommand = new DelegateCommand(_ => remove(this));
        _ = LoadThumbnailAsync();
    }

    public string ReferencePath { get; }

    public string TargetPath { get; }

    public string ReferenceFileName => Path.GetFileName(ReferencePath);

    public string TargetFileName => Path.GetFileName(TargetPath);

    public string DisplayName => $"{ReferenceFileName} -> {TargetFileName}";

    public bool ReferenceExists { get; }

    public bool ReferenceMissing => !ReferenceExists;

    public bool TargetExists { get; }

    public bool TargetMissing => !TargetExists;

    public ICommand OpenCommand { get; }

    public ICommand RemoveCommand { get; }

    public Bitmap? ThumbnailImage
    {
        get => _thumbnailImage;
        private set
        {
            if (ReferenceEquals(_thumbnailImage, value))
            {
                return;
            }

            _thumbnailImage = value;
            RaisePropertyChanged(nameof(ThumbnailImage));
            RaisePropertyChanged(nameof(HasThumbnail));
            RaisePropertyChanged(nameof(ShowThumbnailPlaceholder));
        }
    }

    public bool HasThumbnail => ThumbnailImage is not null;

    public bool IsThumbnailLoaded
    {
        get => _isThumbnailLoaded;
        private set
        {
            if (_isThumbnailLoaded == value)
            {
                return;
            }

            _isThumbnailLoaded = value;
            RaisePropertyChanged(nameof(IsThumbnailLoaded));
            RaisePropertyChanged(nameof(ShowThumbnailPlaceholder));
        }
    }

    /// <summary>Shown only after load finishes when no image was found.</summary>
    public bool ShowThumbnailPlaceholder => IsThumbnailLoaded && !HasThumbnail;

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task LoadThumbnailAsync()
    {
        Bitmap? bitmap = null;
        try
        {
            bitmap = await Task.Run(LoadThumbnailFromDisk).ConfigureAwait(false);
        }
        catch
        {
            // Keep bitmap null; show placeholder.
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ThumbnailImage = bitmap;
            IsThumbnailLoaded = true;
        });
    }

    private Bitmap? LoadThumbnailFromDisk()
    {
        var bytes = TryReadThumbnailBytes(ReferencePath, ReferenceExists)
                    ?? TryReadThumbnailBytes(TargetPath, TargetExists);
        if (bytes is null || bytes.Length == 0)
        {
            return null;
        }

        try
        {
            using var ms = new MemoryStream(bytes);
            return new Bitmap(ms);
        }
        catch
        {
            return null;
        }
    }

    private static byte[]? TryReadThumbnailBytes(string path, bool fileExists)
    {
        if (!fileExists)
        {
            return null;
        }

        try
        {
            using var archive = ZipFile.OpenRead(path);
            var entry = archive.Entries.FirstOrDefault(e =>
                e.FullName.Equals("thumbnail.png", StringComparison.OrdinalIgnoreCase));
            if (entry is null)
            {
                return null;
            }

            using var stream = entry.Open();
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return memory.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private void RaisePropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
