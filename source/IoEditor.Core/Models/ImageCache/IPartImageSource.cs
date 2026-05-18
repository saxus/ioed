namespace IoEditor.Models.ImageCache
{
    public interface IPartImageSource
    {
        string Name { get; }
        Task InitializeAsync();
        Task<byte[]?> FetchImageAsync(string partId, int colorId);
        Task<long> GetCacheSizeBytesAsync();
        Task ClearCacheAsync();
    }
}
