using IoEditor.Models.Studio;

namespace IoEditor.Models.Comparison
{
    internal record class InstructionSegment(
            string ModelName,
            List<IndexedStep> Steps);
}
