using IoEditor.Models.Instructions;
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
        public StudioFile(
            string fileName, 
            string name, 
            string version,
            LDrawModel model, 
            Instruction instruction,
            byte[] thumbnailContent)
        {
            this.FileName = fileName;
            this.Name = name;
            this.Version = version;
            this.Model = model;
            this.Instruction = instruction;
            this.ThumbnailContent = thumbnailContent;
        }

        public string FileName { get; }

        public string Name { get; }

        public string Version { get; }

        public LDrawModel Model { get; }

        public Instruction Instruction { get; }

        public byte[] ThumbnailContent { get; }
    }
}
