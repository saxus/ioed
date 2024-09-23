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
using System.Xml.Linq;
using System.ComponentModel;

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
            List<ZipArchiveEntry> customPartEntries = new List<ZipArchiveEntry>();

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
                else
                {
                    if (entry.FullName.StartsWith("CustomParts/", StringComparison.OrdinalIgnoreCase))
                    {
                        var name = entry.FullName.Substring(12).ToLowerInvariant();
                        if (name.EndsWith(".dat") && !name.Contains("/"))
                        {
                            customPartEntries.Add(entry);
                        }
                    }
                }
            }

            if (zipEntryModel == null)
            {
                throw new InvalidOperationException("No model file found in Studio file. (Probably legacy file.)");
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);

            var infoFileSchema = ReadInfoFileSchema(zipEntryInfoFile);

            var customParts = ReadCustomParts(customPartEntries);

            var (mainModel, models) = ReadModelFile(zipEntryModel, customParts);
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

        private static List<LDrawCustomPart> ReadCustomParts(List<ZipArchiveEntry> customPartEntries)
        {
            var res = new List<LDrawCustomPart>();

            foreach (var entry in customPartEntries)
            {
                var part = ReadCustomPart(entry);
                res.Add(part);
            }

            return res;
        }

        private static LDrawCustomPart ReadCustomPart(ZipArchiveEntry entry)
        {
            using var stream = entry.Open();
            using var reader = new StreamReader(stream);

            string partName = null;
            string description = null;

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                if (line.StartsWith("0 FILE", StringComparison.OrdinalIgnoreCase))
                {
                    description = reader.ReadLine();
                    continue;
                }

                if (line.StartsWith("0 Name:  ", StringComparison.OrdinalIgnoreCase))
                {
                    partName = line.Substring(9).Trim();
                }

                if (description != null && partName != null)
                {
                    break; 
                }
            }

            // Use filename as fallback if necessary
            string fallbackName = entry.Name;
            if (fallbackName.StartsWith("CustomParts/"))
            {
                fallbackName = fallbackName.Substring("CustomParts/".Length);
            }

            description = description ?? fallbackName;
            partName = partName ?? fallbackName;

            return new LDrawCustomPart()
            {
                PartName = partName,
                Description = description
            };
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

        private static (LDrawModel mainModel, List<LDrawModel> allModels) 
            ReadModelFile(ZipArchiveEntry zipEntryModel, List<LDrawCustomPart> customParts)
        {
            if (zipEntryModel == null) throw new ArgumentNullException(nameof(zipEntryModel));

            using var modelStream = zipEntryModel.Open();
            return LDrawLoader.ReadModels(new LineReader(modelStream), customParts);
        }

        private static Instruction ReadInstructionFile(ZipArchiveEntry zipEntryInstruction)
        {
            if (zipEntryInstruction == null)
            {
                return null;
            }

            using var instructionStream = zipEntryInstruction.Open();
            return InstructionCreator.LoadFromStream(instructionStream);
        }
    }
}
