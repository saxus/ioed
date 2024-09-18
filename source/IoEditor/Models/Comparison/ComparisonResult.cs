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
    }
}
