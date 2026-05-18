using IoEditor.Models.Configuration;

using Microsoft.Extensions.Options;

namespace IoEditor.Models.ImageCache
{
    internal sealed class PartImageSourceSelector : IPartImageSourceSelector
    {
        private IPartImageSource _activeSource;

        public IReadOnlyList<IPartImageSource> AvailableSources { get; }

        /// <summary>Raised when the active source changes. Subscribers (e.g. PartImageCache) can use this to invalidate caches.</summary>
        public event Action? ActiveSourceChanged;

        public IPartImageSource ActiveSource
        {
            get => _activeSource;
            set
            {
                if (_activeSource == value)
                    return;

                _activeSource = value;
                ActiveSourceChanged?.Invoke();
            }
        }

        public PartImageSourceSelector(
            IEnumerable<IPartImageSource> sources,
            IOptions<StudioOptions> options)
        {
            AvailableSources = sources.ToList().AsReadOnly();

            if (AvailableSources.Count == 0)
                throw new InvalidOperationException("No IPartImageSource implementations are registered.");

            var savedName = options.Value.ImageSourceName;
            _activeSource = AvailableSources.FirstOrDefault(s => s.Name == savedName)
                            ?? AvailableSources[0];
        }
    }
}
