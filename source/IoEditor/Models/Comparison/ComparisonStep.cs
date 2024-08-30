using IoEditor.Models.Model;
using IoEditor.Models.Studio;

namespace IoEditor.Models.Comparison
{
    internal class ComparisonStep
    {
        public int Index { get; set; }

        public StepEquality Equality { get; set; }

        public IndexedStep ReferenceStep { get; set;}

        public IndexedStep TargetStep { get; set; }

        public List<IndexedStepItem> UnmodifiedItems { get; } = new List<IndexedStepItem>();
        public List<IndexedStepItem> RemovedItems { get; } = new List<IndexedStepItem>();
        public List<IndexedStepItem> AddedItems { get; } = new List<IndexedStepItem>();
        public List<IndexedStepItem> ModifiedItems { get; } = new List<IndexedStepItem>();   
    }
}
