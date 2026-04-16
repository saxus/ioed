using IoEditor.Models.ImageCache;
using IoEditor.Models.Model;

using System.ComponentModel;

namespace IoEditor.Models.Studio
{
    internal class IndexedStepPart : IndexedStepItem, INotifyPropertyChanged
    {
        public Part Part { get; }

        public Color Color { get; }

        private readonly PartImageProxy _imageProxy;

        public byte[]? Image => _imageProxy?.ImageBytes;

        public IndexedStepPart(LDrawPart lDrawPart, string parentModel, Part part, Color color, PartImageProxy imageProxy)
            : base(lDrawPart, parentModel)
        {
            Part = part;
            Color = color;

            _imageProxy = imageProxy;
            _imageProxy.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PartImageProxy.ImageBytes))
                {
                    RaisePropertyChanged(nameof(Image));
                }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
