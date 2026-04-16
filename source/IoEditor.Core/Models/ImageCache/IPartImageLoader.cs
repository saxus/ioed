using IoEditor.Models.Studio;

namespace IoEditor.Models.ImageCache
{
    internal interface IPartImageLoader
    {
        void QueueLoadingImage(Part part, Color color, Action<byte[]?> callback);
    }
}
