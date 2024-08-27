using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.Configuration
{
    public static class ConfigurationValidator
    {
        public static bool Validate(StudioOptions options)
        {
            return !string.IsNullOrEmpty(options.StudioFolder);
        }
    }
}
