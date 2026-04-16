using IoEditor.Models.Studio;

using System.ComponentModel;

namespace IoEditor.Models.ImageCache
{
    internal class PartImageProxy : INotifyPropertyChanged
    {
        private readonly Part _part;
        private readonly Color _color;
        private readonly IPartImageLoader _partImageLoader;

        private byte[]? _imageBytes;
        public byte[]? ImageBytes
        {
            get
            {
                if (_imageBytes == null)
                {
                    _partImageLoader.QueueLoadingImage(_part, _color, res =>
                    {
                        if (_imageBytes != res)
                        {
                            _imageBytes = res;
                            RaisePropertyChanged(nameof(ImageBytes));
                        }
                    });
                    return null;
                }

                return _imageBytes;
            }
        }

        public PartImageProxy(IPartImageLoader partImageLoader, Part part, Color color)
        {
            _part = part;
            _color = color;
            _partImageLoader = partImageLoader;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
