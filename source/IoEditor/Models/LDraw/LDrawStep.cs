namespace IoEditor.Models.Model
{
    internal class LDrawStep
    {
        public int Index { get; set; }
        public string Description { get; set; }
        public List<LDrawPart> Parts { get; } = new List<LDrawPart>();
    }
}
