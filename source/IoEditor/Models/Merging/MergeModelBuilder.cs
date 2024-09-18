using IoEditor.Models.Comparison;
using IoEditor.Models.Instructions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.Merging
{
    internal class MergeModelBuilder
    {
        internal MergeModel Build(ComparisonResult comparisonResult, Instruction instruction)
        {
            var mergedSegments = comparisonResult.InstructionSegments.Select(x => new MergedSegment(x)).ToList();

            var model = new MergeModel();
            model.Segments.AddRange(mergedSegments);
            return model;
        }
    }
}
