using IoEditor.Models.Studio;

namespace IoEditor.Models.ImageCache
{
    internal class BitmapImageProxyFactory : IBitmapImageProxyFactory
    {
        private readonly IPartImageLoader _partImageLoader;

        public BitmapImageProxyFactory(IPartImageLoader partImageLoader)
        {
            this._partImageLoader = partImageLoader;
        }

        public BitmapImageProxy Create(Part part, Color color)
        {
            return new BitmapImageProxy(_partImageLoader, part, color);
        }
    }
}