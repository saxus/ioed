﻿using IoEditor.Models.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.LDraw
{
    internal class LDrawLoader
    {
        internal static LDrawModel LoadFromStream(Stream modelStream)
        {
            return new LDrawModel();
        }
    }
}
