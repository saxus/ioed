using ICSharpCode.AvalonEdit.Document;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Xml.Linq;

namespace IoEditor.UI.Converters
{
    public class XDocumentToTextDocumentConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is XDocument xDocument)
            {
                return new TextDocument(xDocument.ToString());
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TextDocument textDocument)
            {
                try
                {
                    return XDocument.Parse(textDocument.Text);
                }
                catch (Exception)
                {
                    // Handle parsing exceptions if necessary
                    return null;
                }
            }
            return null;
        }
    }
}
