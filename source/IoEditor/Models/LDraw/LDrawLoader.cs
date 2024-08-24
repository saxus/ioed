using IoEditor.Models.Model;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.LDraw
{
    internal class LDrawLoader
    {
        internal static LDrawModel LoadFromZipEntry(ZipArchiveEntry zipEntryModel)
        {
            return new LDrawModel();
        }
    }
}
