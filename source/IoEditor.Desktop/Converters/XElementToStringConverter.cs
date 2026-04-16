using System.Globalization;
using System.Xml.Linq;
using Avalonia.Data.Converters;

namespace IoEditor.Desktop.Converters;

internal sealed class XElementToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is XElement el ? el.ToString() : string.Empty;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
