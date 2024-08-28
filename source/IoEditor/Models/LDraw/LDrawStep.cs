namespace IoEditor.Models.Model
{
    internal class LDrawStep
    {
        public string Description { get; set; }
        public List<LDrawPart> Parts { get; } = new List<LDrawPart>();
    }
}
