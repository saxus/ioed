using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.Configuration
{
    public class StudioOptions
    {
        public string StudioFolder { get; set; }

        /// <summary>When true, main window shows Reference/Target/Generated XML and Step dictionary tabs.</summary>
        public bool ShowXmlDebugTabs { get; set; }
    }
}
