namespace IoEditor.Models.ImageCache
{
    public interface IPartImageSourceSelector
    {
        IReadOnlyList<IPartImageSource> AvailableSources { get; }
        IPartImageSource ActiveSource { get; set; }
        event Action? ActiveSourceChanged;
    }
}
