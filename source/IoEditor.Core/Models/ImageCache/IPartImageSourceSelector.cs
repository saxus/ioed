namespace IoEditor.Models.ImageCache
{
    internal interface IPartImageSourceSelector
    {
        IReadOnlyList<IPartImageSource> AvailableSources { get; }
        IPartImageSource ActiveSource { get; set; }
        event Action? ActiveSourceChanged;
    }
}
