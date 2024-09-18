using IoEditor.Models.Comparison;

using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace IoEditor.UI.Converters
{
    public class InstructionSegmentEqualityToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is InstructionSegmentEquality equality)
            {
                switch (equality)
                {
                    case InstructionSegmentEquality.Equivalent:
                        return Brushes.Wheat;
                    case InstructionSegmentEquality.NewSegment:
                        return Brushes.Lime;
                    case InstructionSegmentEquality.RemovedSegment:
                        return Brushes.Red;
                    case InstructionSegmentEquality.Modified:
                        return Brushes.Orange;
                    default:
                        return Brushes.Black;
                }
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
