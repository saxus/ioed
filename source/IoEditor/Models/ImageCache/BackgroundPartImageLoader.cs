using IoEditor.Models.Studio;

using Microsoft.Extensions.Hosting;

using System.Collections.Concurrent;
using System.Windows.Media.Imaging;

namespace IoEditor.Models.ImageCache
{
    internal class BackgroundPartImageLoader: IPartImageLoader
    {
        private PartImageCache _cache;
        private BlockingCollection<(Part part, Color color, Action<BitmapImage> callback)> _queue;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public BackgroundPartImageLoader(PartImageCache cache)
        {
            _cache = cache;
        }

        public void QueueLoadingImage(Part part, Color color, Action<BitmapImage> callback)
        {
            _queue?.Add((part, color, callback));
        }

        private async Task StartProcessingQueueAsync()
        {
            _queue = new BlockingCollection<(Part, Color, Action<BitmapImage>)>();

            await Task.Run(async () =>
            {
                foreach (var (part, color, callback) in _queue.GetConsumingEnumerable(_cancellationTokenSource.Token))
                {
                    try
                    {
                        await ProcessQueuedItem(part, color, callback);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                    }
                }
            }, _cancellationTokenSource.Token);
        }

        private async Task ProcessQueuedItem(Part part, Color color, Action<BitmapImage> callback)
        {
            if (!color.BLColorCode.HasValue)
            {
                return;
            }

            var image = await _cache.LoadImageAsync(part.BLItemNo, color.BLColorCode.Value);
            callback?.Invoke(image);
        }

        private void StopProcessingQueue()
        {
            _queue?.CompleteAdding();
            _queue = null;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return StartProcessingQueueAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            StopProcessingQueue();
            return Task.CompletedTask;
        }
    }
}
