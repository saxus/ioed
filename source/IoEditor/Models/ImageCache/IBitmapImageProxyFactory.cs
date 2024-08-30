using IoEditor.Models.Studio;

namespace IoEditor.Models.ImageCache
{
    internal interface IBitmapImageProxyFactory
    {
        BitmapImageProxy Create(Part part, Color color);
    }
}