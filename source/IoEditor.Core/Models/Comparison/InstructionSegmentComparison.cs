using IoEditor.Models.Model;

namespace IoEditor.Models.Comparison
{
    internal record class InstructionSegmentComparison(
        InstructionSegmentEquality equality,
        InstructionSegment reference,
        InstructionSegment target,
        string diff);
}
