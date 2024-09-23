using IoEditor.Models.Comparison;
using IoEditor.Models.Merging;
using IoEditor.Models.Studio;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IoEditor.Model
{
    internal class IoEdProject : INotifyPropertyChanged
    {
        public string ReferenceFileName { get; }
        public string TargetFileName { get; }

        public StudioFile Reference { get; }
        public StudioFile Target { get; }

        private ComparisonResult _comparisonResult;
        public ComparisonResult ComparisonResult
        {
            get => _comparisonResult;
            set
            {
                if (_comparisonResult != value)
                {
                    _comparisonResult = value;
                    RaisePropertyChanged(nameof(ComparisonResult));
                }
            }
        }

        private MergeModel _mergeModel;
        public MergeModel MergeModel
        {
            get => _mergeModel;
            set
            {
                if (_mergeModel != value)
                {
                    _mergeModel = value;
                    RaisePropertyChanged(nameof(MergeModel));
                }
            }
        }

        public InterimData InterimData { get; } = new InterimData();
        

        public IoEdProject(string referenceFileName, string targetFileName, StudioFile reference, StudioFile target)
        {
            ReferenceFileName = referenceFileName;
            TargetFileName = targetFileName;

            Reference = reference;
            Target = target;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
