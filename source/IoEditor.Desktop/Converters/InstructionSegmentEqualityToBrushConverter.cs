using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using IoEditor.Models.Comparison;

namespace IoEditor.Desktop.Converters;

internal sealed class InstructionSegmentEqualityToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is InstructionSegmentEquality equality)
        {
            return InstructionSegmentEqualityBrushes.For(equality);
        }

        return Brushes.Black;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
