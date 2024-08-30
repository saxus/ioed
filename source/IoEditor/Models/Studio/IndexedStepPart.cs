using IoEditor.Models.ImageCache;

using System.Windows.Media.Imaging;

namespace IoEditor.Models.Studio
{

    internal class IndexedStepPart : IndexedStepItem
    {
        public Part Part { get; }

        public Color Color { get; }

        private BitmapImageProxy _imageProxy;
        public BitmapImage Image => _imageProxy?.Image;

        public IndexedStepPart(Part part, Color color, BitmapImageProxy imageProxy)
        {
            Part = part;
            Color = color;

            _imageProxy = imageProxy;
        }
    }
}
