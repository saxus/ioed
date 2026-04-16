using IoEditor.Model;

using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IoEditor.Models.Studio
{
    internal class StudioFileSaver
    {
        public static void Save(string filePath, IoEdProject project)
        {
            Console.WriteLine("Saving Studio file: " + filePath);

            File.Copy(project.Target.FileName, filePath, true);

            using var zipFile = ZipFile.Open(filePath, ZipArchiveMode.Update);
            RemoveOldInsturction(zipFile);
            var xws = new XmlWriterSettings()
            {
                OmitXmlDeclaration = true,
                Indent = true,
                Encoding = new UTF8Encoding(false)
            };

            var entry = zipFile.CreateEntry("model.ins", CompressionLevel.Optimal);

            using (var entryStream = entry.Open())            
            using (var xw = XmlWriter.Create(entryStream, xws))
            {
                Console.WriteLine("Save: model.ins");
                project.MergedInstruction.Save(xw);
            }

#if DEBUG
            // TODO: REMOVE
            using var ms = new MemoryStream();
            using (var xw = XmlWriter.Create(ms, xws))
            {
                project.MergedInstruction.Save(ms);
            }
            File.WriteAllBytes(filePath + ".ins.xml", ms.ToArray());
#endif

            RemoveOldImages(zipFile);
            foreach (var images in project.MergedImageResources)
            {
                Console.WriteLine($"Save: {images.Key}");
            
                var imgEntry = zipFile.CreateEntry(images.Key, CompressionLevel.Optimal);
                using var es = imgEntry.Open();
                es.Write(images.Value, 0, images.Value.Length);
            }

            Console.WriteLine("Done");
        }

        private static void RemoveOldImages(ZipArchive zipFile)
        {
            var entriesToRemove = new List<ZipArchiveEntry>();

            foreach (var entry in zipFile.Entries)
            {
                if (entry.FullName.StartsWith("ImageResource/", StringComparison.OrdinalIgnoreCase))
                {
                    entriesToRemove.Add(entry);                    
                }
            }

            foreach (var entry in entriesToRemove)
            {
                Console.WriteLine($"Remove: {entry.FullName}");
                entry.Delete();
            }
        }

        private static void RemoveOldInsturction(ZipArchive zipFile)
        {
            ZipArchiveEntry zipEntryInstruction = null;

            foreach (var entry in zipFile.Entries)
            {
                if (entry.FullName.Equals("model.ins", StringComparison.OrdinalIgnoreCase))
                {
                    zipEntryInstruction = entry;
                }
            }

            if (zipEntryInstruction != null)
            {
                Console.WriteLine($"Remove: {zipEntryInstruction.FullName}");

                zipEntryInstruction.Delete();
            }
        }
    }
}
