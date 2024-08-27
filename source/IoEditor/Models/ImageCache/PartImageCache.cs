using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IoEditor.Models.ImageCache
{

    internal class PartImageCache
    {
        private static readonly string[] s_extensions = { "jpg", "JPG", "GIF" };
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(4); // Limit to 3 concurrent requests

        public async Task<BitmapImage> LoadImageAsync(string partName, int BLColorId)
        {
            if (partName.EndsWith(".dat", StringComparison.OrdinalIgnoreCase))
            {
                partName = partName.Substring(0, partName.Length - 4);
            }

            if (BLColorId <= 0)
            {
                return null;
            }

            byte[] bytes = null;

            var path = Path.GetDirectoryName(this.GetType().Assembly.Location);
            path = Path.Combine(path, "cache", BLColorId.ToString());

            foreach (var ext in s_extensions)
            {
                var fn = Path.Combine(path, partName + "." + ext);

                if (File.Exists(fn))
                {
                    bytes = File.ReadAllBytes(fn);
                    break;
                }
            }

            if (bytes == null)
            {
                bytes = await FetchImageFromWebAsync(partName, BLColorId, path);
            }

            if (bytes == null || bytes.Length < 1)
            {
                return null;
            }

            using var ms = new MemoryStream(bytes);
            var bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.CacheOption = BitmapCacheOption.OnLoad;
            bi.EndInit();
            bi.Freeze(); // To make it cross-thread accessible

            return bi;
        }

        private async Task<byte[]> FetchImageFromWebAsync(string partName, int colorId, string path)
        {
            byte[] bytes = null;
            string lastExt = "jpg";

            foreach (var ext in s_extensions)
            {
                lastExt = ext;

                try
                {
                    bytes = await HttpGetAsync($"http://img.bricklink.com/P/{colorId}/{partName}.{ext}");

                    if (bytes != null)
                    {
                        break;
                    }
                }
                catch (HttpRequestException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        continue;
                    }
                    throw;
                }
            }

            if (bytes != null && bytes.Length > 0)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                File.WriteAllBytes(Path.Combine(path, partName + "." + lastExt), bytes);
            }

            return bytes;
        }

        private async Task<byte[]> HttpGetAsync(string url)
        {
            await _semaphore.WaitAsync();

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:37.0) Gecko/20100101 Firefox/37.0");
                request.Headers.Add("Accept-Language", "hu-HU,hu;q=0.8,en-US;q=0.5,en;q=0.3");

                var sw = Stopwatch.StartNew();

                try
                {
                    var response = await _httpClient.SendAsync(request);
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
                PartImageCache._semaphore.Release();
            }
        }
    }

}
