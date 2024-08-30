using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoEditor.Models.LDraw;
using IoEditor.Models.InfoFile;
using System.Text.Json;
using IoEditor.Models.Model;
using IoEditor.Models.Instructions;

namespace IoEditor.Models.Studio
{
    internal class StudioFileLoader
    {
        public static StudioFile Load(string filePath)
        {
            Console.WriteLine("Loading Studio file: " + filePath);

            using var zipFile = ZipFile.Open(filePath, ZipArchiveMode.Read);

            ZipArchiveEntry zipEntryInfoFile = null;
            ZipArchiveEntry zipEntryThumbnail = null;
            ZipArchiveEntry zipEntryModel = null;
            ZipArchiveEntry zipEntryInstruction = null;
            List<ZipArchiveEntry> zipEntryImages = new List<ZipArchiveEntry>();

            foreach (var entry in zipFile.Entries)
            {
                if (entry.FullName.Equals(".info", StringComparison.OrdinalIgnoreCase))
                {
                    zipEntryInfoFile = entry;
                }
                else if (entry.FullName.Equals("thumbnail.png", StringComparison.OrdinalIgnoreCase))
                {
                    zipEntryThumbnail = entry;
                }
                else if (entry.FullName.Equals("modelv2.ldr", StringComparison.OrdinalIgnoreCase))
                {
                    zipEntryModel = entry;
                }
                else if (entry.FullName.Equals("model.ins", StringComparison.OrdinalIgnoreCase))
                {
                    zipEntryInstruction = entry;
                }
                else if (entry.FullName.StartsWith("ImageResource/", StringComparison.OrdinalIgnoreCase))
                {
                    zipEntryImages.Add(entry);
                }
            }

            if (zipEntryModel == null)
            {
                throw new InvalidOperationException("No model file found in Studio file. (Probably legacy file.)");
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);

            var infoFileSchema = ReadInfoFileSchema(zipEntryInfoFile);
            var (mainModel, models) = ReadModelFile(zipEntryModel);
            var instruction = ReadInstructionFile(zipEntryInstruction);
            var thumbnailContent = ReadThumbnailContent(zipEntryThumbnail);

            var studioFile = new StudioFile(
               filePath,
               fileName,
               infoFileSchema.Version,
               mainModel,
               models,
               instruction,
               thumbnailContent);

            return studioFile;
        }

        private static byte[] ReadThumbnailContent(ZipArchiveEntry zipEntryThumbnail)
        {
            if (zipEntryThumbnail == null) throw new ArgumentNullException(nameof(zipEntryThumbnail));

            using var thumbnailStream = zipEntryThumbnail.Open();
            using var memoryStream = new MemoryStream();
            thumbnailStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        private static InfoFileSchema ReadInfoFileSchema(ZipArchiveEntry zipEntryInfoFile)
        {
            if (zipEntryInfoFile == null) throw new ArgumentNullException(nameof(zipEntryInfoFile));

            using var infoFileStream = zipEntryInfoFile.Open();
            using var reader = new StreamReader(infoFileStream);
            var jsonString = reader.ReadToEnd();
            return JsonSerializer.Deserialize<InfoFileSchema>(jsonString);
        }

        private static (LDrawModel mainModel, List<LDrawModel> allModels) ReadModelFile(ZipArchiveEntry zipEntryModel)
        {
            if (zipEntryModel == null) throw new ArgumentNullException(nameof(zipEntryModel));

            using var modelStream = zipEntryModel.Open();
            return LDrawLoader.ReadModels(new LineReader(modelStream));
        }

        private static Instruction ReadInstructionFile(ZipArchiveEntry zipEntryInstruction)
        {
            if (zipEntryInstruction == null) throw new ArgumentNullException(nameof(zipEntryInstruction));

            using var instructionStream = zipEntryInstruction.Open();
            return InstructionLoader.LoadFromStream(instructionStream);
        }
    }
}
