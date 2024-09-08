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

        private Matrix4x4 _transformationMatrix;
        public Matrix4x4 TransformationMatrix 
        {
            get { return _transformationMatrix; }
            set { _transformationMatrix = value; } 
        }

        public Vector3 Position
        {
            get
            {
                return new Vector3(TransformationMatrix.M14, TransformationMatrix.M24, TransformationMatrix.M34);
            }
            set
            {
                _transformationMatrix.M14 = value.X;
                _transformationMatrix.M24 = value.Y;
                _transformationMatrix.M34 = value.Z;
                RaisePropertyChanged(nameof(Position));
                RaisePropertyChanged(nameof(TransformationMatrix));
            }
        }

        public Matrix3x3 Rotation
        {
            get
            {
                return new Matrix3x3(
                    _transformationMatrix.M11, _transformationMatrix.M12, _transformationMatrix.M13,
                    _transformationMatrix.M21, _transformationMatrix.M22, _transformationMatrix.M23,
                    _transformationMatrix.M31, _transformationMatrix.M32, _transformationMatrix.M33
                );
            }
            set
            {
                _transformationMatrix.M11 = value.M11;
                _transformationMatrix.M12 = value.M12;
                _transformationMatrix.M13 = value.M13;
                _transformationMatrix.M21 = value.M21;
                _transformationMatrix.M22 = value.M22;
                _transformationMatrix.M23 = value.M23;
                _transformationMatrix.M31 = value.M31;
                _transformationMatrix.M32 = value.M32;
                _transformationMatrix.M33 = value.M33;
                RaisePropertyChanged(nameof(Rotation));
                RaisePropertyChanged(nameof(TransformationMatrix));
            }
        }



        public bool IsOfficialPart => !IsCustomPart && PartName.EndsWith(".dat", StringComparison.OrdinalIgnoreCase);
        public bool IsCustomPart => CustomPart != null;


        public LDrawPart()
        {
            _transformationMatrix.M44 = 1f;
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
