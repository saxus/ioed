using System;
using System.Globalization;

namespace IoEditor.Models.Studio
{
    internal static class Parsers
    {

        public static int IntParser(string[] values, string[] headers, int index)
        {
            if (!int.TryParse(values[index], out int result))
            {
                throw new FormatException($"Error parsing column '{headers[index]}' with value '{values[index]}' as an integer.");
            }
            return result;
        }

        public static int? NullableIntParser(string[] values, string[] headers, int index)
        {
            if (string.IsNullOrWhiteSpace(values[index]))
            {
                return null;
            }

            if (!int.TryParse(values[index], out int result))
            {
                throw new FormatException($"Error parsing column '{headers[index]}' with value '{values[index]}' as an integer.");
            }
            return result;
        }

        public static bool BoolParser(string[] values, string[] headers, int index)
        {
            string value = values[index].Trim();
            if (bool.TryParse(value, out bool result))
            {
                return result;
            }
            else if (value == "0")
            {
                return false;
            }
            else if (value == "1")
            {
                return true;
            }
            else
            {
                throw new FormatException($"Error parsing column '{headers[index]}' with value '{values[index]}' as a boolean.");
            }
        }

        internal static float FloatParser(string[] values, string[] headers, int index)
        {
            if (!float.TryParse(values[index], CultureInfo.InvariantCulture, out float result ))
            {
                throw new FormatException($"Error parsing column '{headers[index]}' with value '{values[index]}' as a float.");
            }
            return result;
        }
    }
}
