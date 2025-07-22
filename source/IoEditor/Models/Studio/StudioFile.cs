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
            LDrawModel mainModel, 
            List<LDrawModel> models,
            List<LDrawCustomPart> customParts,
            Instruction instruction,
            byte[] thumbnailContent,
            Dictionary<string, byte[]> imageResources)
        {
            this.FileName = fileName;
            this.Name = name;
            this.Version = version;
            this.MainModel = mainModel;
            this.Instruction = instruction;
            this.ThumbnailContent = thumbnailContent;
            this.ImageResources = imageResources;

            foreach (var model in models)
            {
                this.Models[model.Name.ToLower()] = model;
            }

            foreach (var customPart in customParts)
            {
                this.CustomParts[customPart.PartName.ToLower()] = customPart;
            }

            UpdateModelReferences();
        }

        private void UpdateModelReferences()
        {
            foreach (var model in Models.Values)
            {
                foreach (var step in model.Steps)
                {
                    foreach (var part in step.Parts)
                    {
                        if (!part.IsOfficialPart)
                        {
                            var partModel = GetModel(part.PartName);
                            part.Model = partModel;
                        }
                    }
                }
            }
        }

        public string FileName { get; }

        public string Name { get; }

        public string Version { get; }

        public LDrawModel MainModel { get; }

        public Dictionary<string, LDrawModel> Models { get; } = new Dictionary<string, LDrawModel>();

        public Dictionary<string, LDrawCustomPart> CustomParts { get; } = new Dictionary<string, LDrawCustomPart>();

        public Instruction Instruction { get; set; }

        public byte[] ThumbnailContent { get; }

        public Dictionary<string, byte[]> ImageResources { get; }

        internal LDrawModel GetModel(string partName)
        {
            string lowercasedPartName = partName.ToLower();
            if (Models.TryGetValue(lowercasedPartName, out LDrawModel model))
            {
                return model;
            }
            else
            {
                return null;
            }
        }
    }
}
