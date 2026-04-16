using System.Globalization;
using Avalonia.Data.Converters;

namespace IoEditor.Desktop.Converters;

internal sealed class BoolToMaxHeightConverter : IValueConverter
{
    public double ExpandedHeight { get; set; } = 600;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? ExpandedHeight : 0d;
        }

        return 0d;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
