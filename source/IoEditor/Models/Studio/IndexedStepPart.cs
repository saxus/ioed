using IoEditor.Models.Model;

namespace IoEditor.Models.Studio
{
    internal class IndexedStepItem
    {
        public int Quantity { get; set; }
        public List<LDrawPart> LDrawParts { get; set; } = new List<LDrawPart>();
    }

    internal class IndexedStepSubmodel: IndexedStepItem
    {
        public string ModelName => Model.Name;
        public LDrawModel Model { get; set; }
    }

    internal class IndexedStepPart: IndexedStepItem
    {
        public Part Part { get; set; }
        public Color Color { get; set; }
    }
}
