using IoEditor.Models.Studio;

namespace IoEditor.Models.ImageCache
{
    internal interface IPartImageProxyFactory
    {
        PartImageProxy Create(Part part, Color color);
    }
}
