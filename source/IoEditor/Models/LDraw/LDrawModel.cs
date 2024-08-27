using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace IoEditor.Models.Model
{
    internal class LDrawModel
    {
        public string File { get; set; }
        public string Name { get; set; }
        public string Descrption { get; set; }
        public string Author { get; set; }

        public bool IsSubModel { get; set; }

        public List<LDrawStep> Steps { get; } = new List<LDrawStep>();
    }

    internal class LDrawStep
    {
        public int Index { get; set; }
        public string Description { get; set; }
        public List<LDrawPart> Parts { get; } = new List<LDrawPart>();
    }

    internal class LDrawPart: INotifyPropertyChanged
    {
        public int LDrawColorId { get; set; }
        public string PartName { get; set; }
        public string IsInverted { get; set; }
        public LDrawModel Model { get; set; }

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        private BitmapImage _image;
        public BitmapImage Image
        {
            get => _image;
            set
            {
                if (_image != value)
                {
                    _image = value;
                    RaisePropertyChanged(nameof(Image));
                }
            }
        }

        public bool IsOfficialPart =>
            PartName.EndsWith(".dat", StringComparison.OrdinalIgnoreCase);

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
