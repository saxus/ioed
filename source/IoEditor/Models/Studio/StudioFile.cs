using IoEditor.Models.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.Studio
{
    internal class StudioFile
    {
        public StudioFile(string fileName, string name, LDrawModel model, byte[] thumbnailContent)
        {
            this.FileName = fileName;
            this.Name = name;
            this.Model = model;
            this.ThumbnailContent = thumbnailContent;
        }

        public string FileName { get; }
        public string Name { get; }
        
        public LDrawModel Model { get; }

        public byte[] ThumbnailContent { get; }
    }
}
