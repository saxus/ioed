using IoEditor.Models.Model;

namespace IoEditor.Models.Studio
{
    internal class IndexedStepCustomPart : IndexedStepItem
    {
        public LDrawCustomPart Part { get; }

        public Color Color { get; }

        public IndexedStepCustomPart(LDrawPart lDrawPart, string parentModel, LDrawCustomPart part, Color color)
            : base(lDrawPart, parentModel)
        {
            Part = part;
            Color = color;
        }
    }
}
