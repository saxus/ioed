using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.Model
{
    internal class LDrawModel
    {
        public string File { get; set; }
        public string Name { get; set; }
        public string Descrption { get; set; }
        public string Author { get; set; }

        public bool IsSubModel { get; set; }

        public List<LDrawStep> Steps { get; } = new List<LDrawStep>();

        public override string ToString() => Name;
    }
}
