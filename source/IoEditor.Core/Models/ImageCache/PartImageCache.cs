using System.Collections.Concurrent;

namespace IoEditor.Models.ImageCache
{
    internal class PartImageCache
    {
        private readonly IPartImageSourceSelector _selector;
        private readonly ConcurrentDictionary<string, byte[]?> _cache = new();

        public PartImageCache(IPartImageSourceSelector selector)
        {
            _selector = selector;
            _selector.ActiveSourceChanged += InvalidateMemoryCache;
        }

        public async Task<byte[]?> LoadImageAsync(string partName, int blColorId)
        {
            if (partName.EndsWith(".dat", StringComparison.OrdinalIgnoreCase))
                partName = partName[..^4];

            if (blColorId <= 0)
                return null;

            var key = $"{blColorId}::{partName}";

            if (!_cache.TryGetValue(key, out var cached))
            {
                cached = await _selector.ActiveSource.FetchImageAsync(partName, blColorId);
                _cache.TryAdd(key, cached);
            }

            return cached;
        }

        internal void InvalidateMemoryCache() => _cache.Clear();
    }
}
