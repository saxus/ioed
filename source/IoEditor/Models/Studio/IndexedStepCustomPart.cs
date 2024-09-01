using IoEditor.Models.Model;

namespace IoEditor.Models.Studio
{
    internal class IndexedStepCustomPart : IndexedStepItem
    {
        public LDrawCustomPart Part { get; }

        public Color Color { get; }

        public override string SimplifiedHash => $"cp::{Color.StudioColorCode}::{Part.PartName}";


        public IndexedStepCustomPart(LDrawCustomPart part, Color color)
        {
            Part = part;
            Color = color;
        }
    }
}
