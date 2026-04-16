using IoEditor.Models.Studio;

namespace IoEditor.Models.ImageCache
{
    internal class PartImageProxyFactory : IPartImageProxyFactory
    {
        private readonly IPartImageLoader _partImageLoader;

        public PartImageProxyFactory(IPartImageLoader partImageLoader)
        {
            _partImageLoader = partImageLoader;
        }

        public PartImageProxy Create(Part part, Color color)
        {
            return new PartImageProxy(_partImageLoader, part, color);
        }
    }
}
