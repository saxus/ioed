using IoEditor.Models.Model;

namespace IoEditor.Models.Studio
{
    internal class IndexedStepSubmodel: IndexedStepItem
    {
        public string ModelName => Model.Name;

        public LDrawModel Model { get; }

        public IndexedStepSubmodel(LDrawPart part, string parentModel, LDrawModel model)
            :base(part, parentModel)
        {
            Model = model;
        }
    }
}
