using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace IoEditor.Models.ImageCache
{
    internal sealed class BrickLinkImageSource : IPartImageSource
    {
        private static readonly string[] s_extensions = { "jpg", "JPG", "GIF" };
        private static readonly HttpClient s_httpClient = new HttpClient();
        private static readonly SemaphoreSlim s_semaphore = new SemaphoreSlim(4);

        private readonly string _cacheDir;
        private readonly NotFoundCache _notFoundCache;
        private readonly ConcurrentDictionary<string, bool> _failedParts = new();

        public string Name => "BrickLink";

        public BrickLinkImageSource(string basePath)
        {
            _cacheDir = Path.Combine(basePath, "cache", "bricklink");
            _notFoundCache = new NotFoundCache(Path.Combine(_cacheDir, "not-found.db"));
        }

        public async Task InitializeAsync()
        {
            Directory.CreateDirectory(_cacheDir);
            await _notFoundCache.InitializeAsync();
        }

        public async Task<byte[]?> FetchImageAsync(string partId, int colorId)
        {
            if (partId.EndsWith(".dat", StringComparison.OrdinalIgnoreCase))
                partId = partId[..^4];

            var key = $"{partId}-{colorId}";

            if (_failedParts.ContainsKey(key))
                return null;

            if (await _notFoundCache.IsNotFoundAsync(key))
                return null;

            var colorDir = Path.Combine(_cacheDir, colorId.ToString());

            foreach (var ext in s_extensions)
            {
                var filePath = Path.Combine(colorDir, partId + "." + ext);
                if (File.Exists(filePath))
                    return await File.ReadAllBytesAsync(filePath);
            }

            return await FetchFromWebAsync(partId, colorId, colorDir, key);
        }

        public Task<long> GetCacheSizeBytesAsync()
        {
            if (!Directory.Exists(_cacheDir))
                return Task.FromResult(0L);

            var size = Directory.EnumerateFiles(_cacheDir, "*", SearchOption.AllDirectories)
                .Where(f => !f.EndsWith("not-found.db", StringComparison.OrdinalIgnoreCase))
                .Sum(f => new FileInfo(f).Length);

            return Task.FromResult(size);
        }

        public async Task ClearCacheAsync()
        {
            foreach (var colorDir in Directory.EnumerateDirectories(_cacheDir))
            {
                Directory.Delete(colorDir, recursive: true);
            }

            _failedParts.Clear();
            await _notFoundCache.ClearAsync();
        }

        private async Task<byte[]?> FetchFromWebAsync(string partId, int colorId, string colorDir, string key)
        {
            byte[]? bytes = null;
            string lastExt = "jpg";

            foreach (var ext in s_extensions)
            {
                lastExt = ext;
                try
                {
                    bytes = await HttpGetAsync($"http://img.bricklink.com/P/{colorId}/{partId}.{ext}");
                    if (bytes != null)
                        break;
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    continue;
                }
            }

            if (bytes != null && bytes.Length > 0)
            {
                Directory.CreateDirectory(colorDir);
                await File.WriteAllBytesAsync(Path.Combine(colorDir, partId + "." + lastExt), bytes);
            }
            else
            {
                _failedParts.TryAdd(key, true);
                await _notFoundCache.AddNotFoundAsync(key);
            }

            return bytes;
        }

        private static async Task<byte[]?> HttpGetAsync(string url)
        {
            await s_semaphore.WaitAsync();
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0");
                request.Headers.Add("Accept-Language", "hu-HU,hu;q=0.8,en-US;q=0.5,en;q=0.3");

                var sw = Stopwatch.StartNew();
                try
                {
                    var response = await s_httpClient.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    var data = await response.Content.ReadAsByteArrayAsync();
                    sw.Stop();
                    Console.WriteLine("GET: {0}, {1} sec", url, sw.Elapsed);
                    return data;
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    sw.Stop();
                    Console.WriteLine("Error GET: {0}, {1} sec, NOT FOUND", url, sw.Elapsed);
                    return null;
                }
                catch (HttpRequestException ex)
                {
                    sw.Stop();
                    Console.WriteLine("Error GET: {0}, {1} sec, Exception: {2}", url, sw.Elapsed, ex.Message);
                    return null;
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    Console.WriteLine("Unexpected error GET: {0}, {1} sec, Exception: {2}", url, sw.Elapsed, ex.Message);
                    return null;
                }
            }
            finally
            {
                s_semaphore.Release();
            }
        }
    }
}
