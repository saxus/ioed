using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IoEditor.Models.Model;

namespace IoEditor.Models.Studio
{
    internal class IndexedStep
    {
        public int Index { get; set; }

        public LDrawStep LDrawStep { get; set; }

        public List<IndexedStepItem> Items { get; } = new List<IndexedStepItem>();

        public IEnumerable<IndexedStepSubmodel> Submodels => Items.OfType<IndexedStepSubmodel>();
        public IEnumerable<IndexedStepPart> Parts => Items.OfType<IndexedStepPart>();
        public IEnumerable<IndexedStepCustomPart> CustomParts => Items.OfType<IndexedStepCustomPart>();
    }
}
