using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Model
{
    internal class StudioProject : INotifyPropertyChanged
    {
        private string _referenceFile;
        public string ReferenceFile
        {
            get { return _referenceFile; }
            set
            {
                if (_referenceFile != value)
                {
                    _referenceFile = value;
                    RaisePropertyChanged(nameof(ReferenceFile));
                }
            }
        }

        private string _targetFile;
        public string TargetFile
        {
            get { return _targetFile; }
            set
            {
                if (_targetFile != value)
                {
                    _targetFile = value;
                    RaisePropertyChanged(nameof(TargetFile));
                }
            }
        }

        public static StudioProject Open(string reference, string target)
        {
            if (string.IsNullOrWhiteSpace(reference)) throw new ArgumentException("Reference file path cannot be null or empty.", nameof(reference));
            if (string.IsNullOrWhiteSpace(target)) throw new ArgumentException("Target file path cannot be null or empty.", nameof(target));
            if (reference == target) throw new ArgumentException("Reference and target files cannot be the same.");
            if (!File.Exists(reference)) throw new FileNotFoundException("Reference file not found.", reference);
            if (!File.Exists(target)) throw new FileNotFoundException("Target file not found.", target);

            return new StudioProject
            {
                ReferenceFile = reference,
                TargetFile = target
            };
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
