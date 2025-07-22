using System.Diagnostics;

namespace IoEditor.Models.Model
{
    [DebuggerDisplay("CustomPart {PartName} {Description}")]
    internal class LDrawCustomPart
    {
        public string PartName { get; set; }
        public string Description { get; set; }

        public string Filename { get; set; }
        public byte[] Bytes { get; set; }

        // Special models like flexible bricks
        public LDrawModel SpecialModel { get; set; }
    }
}
