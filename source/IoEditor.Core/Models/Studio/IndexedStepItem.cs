using IoEditor.Models.Model;
using System.Collections.Specialized;
using System.ComponentModel;

namespace IoEditor.Models.Studio
{
    internal abstract class IndexedStepItem
    {
        public LDrawPart LDrawPart { get; }

        public int StepIndex { get; set; }

        public string LDrawPartName => LDrawPart.PartName;
        public string ParentModel { get; }

        protected IndexedStepItem(LDrawPart lDrawPart, string parentModel)
        {
            LDrawPart = lDrawPart;
            this.ParentModel = parentModel;
        }
    }

    
}
