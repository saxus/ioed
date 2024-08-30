using IoEditor.Models.Model;

using System.Collections.Specialized;
using System.ComponentModel;

namespace IoEditor.Models.Studio
{
    internal class IndexedStepItem
    {
        public int Quantity => LDrawParts.Count;

        public List<LDrawPart> LDrawParts { get; set; } = new List<LDrawPart>();
    }
}
