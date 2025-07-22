using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.Model
{
    [DebuggerDisplay("{File}: {Name}, {Description}")]
    internal class LDrawModel
    {
        public string File { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }

        public bool IsSubModel { get; set; }

        // Flexible brick properties
        public string FlexibleBrick { get; set; }
        public string BLItemNo { get; set; }

        public List<LDrawStep> Steps { get; } = new List<LDrawStep>();

        public override string ToString() => Name;
    }
}
