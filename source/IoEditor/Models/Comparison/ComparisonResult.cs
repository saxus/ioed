using IoEditor.Models.Studio;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.Comparison
{
    internal class ComparisonResult
    {
        public List<InstructionSegmentComparison> InstructionSegments { get; }
             = new List<InstructionSegmentComparison>();

        public List<IndexedStep> IndexedReferenceSteps { get; internal set; }
        public List<IndexedStep> IndexedTargetSteps { get; internal set; }
    }
}
