using IoEditor.Models.Model;

namespace IoEditor.Models.Studio
{
    internal class IndexedStepItem
    {
        public int Quantity { get; set; }
        public List<LDrawPart> LDrawParts { get; set; } = new List<LDrawPart>();
    }
}
