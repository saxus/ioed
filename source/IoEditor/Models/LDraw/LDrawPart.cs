using System.ComponentModel;
using System.Numerics;
using System.Windows.Media.Imaging;

namespace IoEditor.Models.Model
{
    internal class LDrawPart: INotifyPropertyChanged
    {
        public int LineInFile { get; set; }

        public int LDrawColorId { get; set; }
        public string PartName { get; set; }
        public string IsInverted { get; set; }
        public LDrawModel Model { get; set; }

        public LDrawCustomPart CustomPart { get; set; }

        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public bool IsOfficialPart => !IsCustomPart && PartName.EndsWith(".dat", StringComparison.OrdinalIgnoreCase);
        public bool IsCustomPart => CustomPart != null;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
