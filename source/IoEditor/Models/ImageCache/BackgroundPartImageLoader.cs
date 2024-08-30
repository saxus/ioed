using IoEditor.Models.Studio;

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
            this._cache = cache;
            StartProcessingQueueAsync();
        }

        public void QueueLoadingImage(Part part, Color color, Action<BitmapImage> callback)
        {
            if (_queue != null)
            {
                _queue.Add((part, color, callback));
            }
        }


        private async void StartProcessingQueueAsync()
        {
            _queue = new BlockingCollection<(Part, Color, Action<BitmapImage>)>();

            await Task.Run(async () =>
            {
                foreach (var item in _queue.GetConsumingEnumerable(_cancellationTokenSource.Token))
                {
                    await ProcessQueuedItem(item.part, item.color, item.callback);
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

        public void StopProcessingQueue()
        {
            if (_queue != null)
            {
                _queue.CompleteAdding();
                _queue = null;
            }
        }
    }
}
