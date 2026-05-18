using IoEditor.Models.Comparison;
using IoEditor.Models.Studio;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.Merging
{
    internal record StepPair(IndexedStep? Reference, IndexedStep? Target);

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
        public int SegmentIndex { get; init; }


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


        public IEnumerable<StepPair> PairedSteps
        {
            get
            {
                var refSteps = ReferenceSegment?.Steps ?? (IList<IndexedStep>)Array.Empty<IndexedStep>();
                var tgtSteps = TargetSegment?.Steps ?? (IList<IndexedStep>)Array.Empty<IndexedStep>();
                int count = Math.Max(refSteps.Count, tgtSteps.Count);
                for (int i = 0; i < count; i++)
                    yield return new StepPair(
                        i < refSteps.Count ? refSteps[i] : null,
                        i < tgtSteps.Count ? tgtSteps[i] : null);
            }
        }

        public MergedSegment(InstructionSegmentComparison instructionSegment, int segmentIndex)
        {
            this.InstructionSegmentComparison = instructionSegment;
            this.SegmentIndex = segmentIndex;
        }

        public InstructionSegmentComparison InstructionSegmentComparison { get; }



        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
