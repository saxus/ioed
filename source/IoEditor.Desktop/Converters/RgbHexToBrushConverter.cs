using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace IoEditor.Desktop.Converters;

internal sealed class RgbHexToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string hex && hex.Length == 6)
        {
            try
            {
                var r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                var g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                var b = System.Convert.ToByte(hex.Substring(4, 2), 16);
                return new SolidColorBrush(Avalonia.Media.Color.FromRgb(r, g, b));
            }
            catch { }
        }

        return Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
