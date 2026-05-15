using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.Configuration
{
    public enum AppThemeMode
    {
        System,
        Light,
        Dark
    }

    public class StudioOptions
    {
        public string StudioFolder { get; set; }

        /// <summary>When true, main window shows Reference/Target/Generated XML and Step dictionary tabs.</summary>
        public bool ShowXmlDebugTabs { get; set; }

        public AppThemeMode ThemeMode { get; set; } = AppThemeMode.System;
    }
}
