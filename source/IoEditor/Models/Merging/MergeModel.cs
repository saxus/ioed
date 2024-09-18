using IoEditor.Models.Comparison;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.Merging
{
    internal class MergeModel
    {
        public List<MergedSegment> Segments { get; } = new List<MergedSegment>();
    }

    internal class MergedSegment: INotifyPropertyChanged
    {
        public InstructionSegmentEquality Equality => InstructionSegmentComparison.equality;
        public InstructionSegment ReferenceSegment => InstructionSegmentComparison.reference;
        public InstructionSegment TargetSegment => InstructionSegmentComparison.target;
        public string Differences => InstructionSegmentComparison.diff;

        public string SegmentName => TargetSegment?.ModelName ?? ReferenceSegment?.ModelName;


        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    RaisePropertyChanged(nameof(IsExpanded));
                }
            }
        }


        public MergedSegment(InstructionSegmentComparison instructionSegment)
        {
            this.InstructionSegmentComparison = instructionSegment;
        }

        public InstructionSegmentComparison InstructionSegmentComparison { get; }



        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
