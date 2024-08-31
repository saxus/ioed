﻿using IoEditor.Models.ImageCache;

using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace IoEditor.Models.Studio
{

    internal class IndexedStepPart : IndexedStepItem, INotifyPropertyChanged
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
            _imageProxy.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(BitmapImageProxy.Image))
                {
                    RaisePropertyChanged(nameof(Image));
                }
            };
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
