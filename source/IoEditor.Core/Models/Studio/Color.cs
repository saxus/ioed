using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.Studio
{
    internal class Color
    {
        public int StudioColorCode { get; }
        public int? BLColorCode { get; } // TODO: make non-nullable and ignore nulled values
        public int LDrawColorCode { get; }
        public int LDDColorCode { get; }
        public string StudioColorName { get; }
        public string BLColorName { get; }
        public string LDrawColorName { get; }
        public string LDDColorName { get; }
        public string RGBValue { get; }
        public float Alpha { get; }
        public string CategoryName { get; }
        public int ColorGroupIndex { get; }
        public string Note { get; }
        public string InsRGB { get; }
        public string InsCMYK { get; }

        public Color(
            int studioColorCode, 
            int? blColorCode, 
            int lDrawColorCode, 
            int lddColorCode, 
            string studioColorName, 
            string blColorName, 
            string lDrawColorName, 
            string lddColorName, 
            string rgbValue, 
            float alpha, 
            string categoryName, 
            int colorGroupIndex, 
            string note, 
            string insRGB, 
            string insCMYK)
        {
            StudioColorCode = studioColorCode;
            BLColorCode = blColorCode;
            LDrawColorCode = lDrawColorCode;
            LDDColorCode = lddColorCode;
            StudioColorName = studioColorName;
            BLColorName = blColorName;
            LDrawColorName = lDrawColorName;
            LDDColorName = lddColorName;
            RGBValue = rgbValue;
            Alpha = alpha;
            CategoryName = categoryName;
            ColorGroupIndex = colorGroupIndex;
            Note = note;
            InsRGB = insRGB;
            InsCMYK = insCMYK;
        }

        public override string ToString() => StudioColorName;
    }
}
