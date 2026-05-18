namespace IoEditor.Models.ImageCache
{
    internal interface IPartImageSource
    {
        string Name { get; }
        Task InitializeAsync();
        Task<byte[]?> FetchImageAsync(string partId, int colorId);
        Task<long> GetCacheSizeBytesAsync();
        Task ClearCacheAsync();
    }
}
