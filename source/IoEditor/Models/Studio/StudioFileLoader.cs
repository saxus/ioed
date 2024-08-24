using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoEditor.Models.LDraw;

namespace IoEditor.Models.Studio
{
    internal class StudioFileLoader
    {
        public static StudioFile Load(string filePath)
        {
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

            var model = LDrawLoader.LoadFromZipEntry(zipEntryModel);

            var thumbnailContent = ReadThumbnailContent(zipEntryThumbnail);

            return new StudioFile(
                filePath, 
                fileName, 
                model,
                thumbnailContent);
        }

        private static byte[] ReadThumbnailContent(ZipArchiveEntry zipEntryThumbnail)
        {
            if (zipEntryThumbnail == null)
            {
                return null;
            }

            using var thumbnailStream = zipEntryThumbnail.Open();
            using var memoryStream = new MemoryStream();
            thumbnailStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
