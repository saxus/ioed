using IoEditor.Models.Studio;

using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace IoEditor.Models.ImageCache
{
    internal class BitmapImageProxy : INotifyPropertyChanged
    {
        private readonly Part _part;
        private readonly Color _color;
        private readonly IPartImageLoader _partImageLoader;

        private BitmapImage? _image;
        public BitmapImage Image
        { 
            get
            {
                if (_image == null)
                {
                    _partImageLoader.QueueLoadingImage(_part, _color, (res) =>
                    {
                        if (_image != res)
                        {
                            _image = res;
                            RaisePropertyChanged(nameof(Image));
                        };
                    });
                    return null;
                }

                return _image;
            }
        }

        public BitmapImageProxy(IPartImageLoader partImageLoader, Part part, Color color)
        {
            this._part = part;
            this._color = color;
            this._partImageLoader = partImageLoader;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
