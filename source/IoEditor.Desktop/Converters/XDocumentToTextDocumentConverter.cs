using System.Globalization;
using System.Xml.Linq;
using Avalonia.Data.Converters;
using AvaloniaEdit.Document;

namespace IoEditor.Desktop.Converters;

internal sealed class XDocumentToTextDocumentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is XDocument xDocument)
        {
            return new TextDocument(xDocument.ToString());
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TextDocument textDocument)
        {
            try
            {
                return XDocument.Parse(textDocument.Text);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }
}
