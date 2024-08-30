using IoEditor.Models.Studio;

using System.Windows.Media.Imaging;

namespace IoEditor.Models.ImageCache
{
    internal interface IPartImageLoader
    {
        void QueueLoadingImage(Part part, Color color, Action<BitmapImage> callback);
    }
}