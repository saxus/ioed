using Avalonia.Media;
using IoEditor.Models.Comparison;

namespace IoEditor.Desktop.Converters;

internal static class InstructionSegmentEqualityBrushes
{
    public static IBrush For(InstructionSegmentEquality equality)
        => equality switch
        {
            InstructionSegmentEquality.Equivalent => Brushes.Wheat,
            InstructionSegmentEquality.NewSegment => Brushes.Lime,
            InstructionSegmentEquality.RemovedSegment => Brushes.Red,
            InstructionSegmentEquality.Modified => Brushes.Orange,
            _ => Brushes.Black,
        };
}
