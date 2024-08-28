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
        public List<IndexedStepSubmodel> Submodels { get; } = new List<IndexedStepSubmodel>();
        public List<IndexedStepPart> Parts { get; } = new List<IndexedStepPart>();
    }
}
