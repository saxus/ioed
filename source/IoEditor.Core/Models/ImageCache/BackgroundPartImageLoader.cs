using IoEditor.Models.Studio;

using System.Collections.Concurrent;

namespace IoEditor.Models.ImageCache
{
    internal class BackgroundPartImageLoader : IPartImageLoader
    {
        private readonly PartImageCache _cache;
        private readonly IPartImageSourceSelector _selector;
        private BlockingCollection<(Part part, Color color, Action<byte[]?> callback)>? _queue;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public BackgroundPartImageLoader(PartImageCache cache, IPartImageSourceSelector selector)
        {
            _cache = cache;
            _selector = selector;
        }

        public void QueueLoadingImage(Part part, Color color, Action<byte[]?> callback)
        {
            _queue?.Add((part, color, callback));
        }

        private async Task StartProcessingQueueAsync()
        {
            _queue = new BlockingCollection<(Part, Color, Action<byte[]?>)>();

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

        private async Task ProcessQueuedItem(Part part, Color color, Action<byte[]?> callback)
        {
            if (color == null)
            {
                var partInfo = part != null ? $" (part: {part.BLItemNo} – {part.Description})" : string.Empty;
                Console.WriteLine($"Cannot load image, because color is null{partInfo}!");
                return;
            }

            if (!color.BLColorCode.HasValue)
                return;

            if (part == null)
            {
                Console.WriteLine("Cannot load image, because part is null!");
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

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _selector.ActiveSource.InitializeAsync();
            _ = StartProcessingQueueAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();
            StopProcessingQueue();
            return Task.CompletedTask;
        }
    }
}
