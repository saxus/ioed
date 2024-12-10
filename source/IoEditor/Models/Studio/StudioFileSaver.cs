using IoEditor.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var entry = zipFile.CreateEntry("model.ins", CompressionLevel.Optimal);
            using (var entryStream = entry.Open())
            using (var streamWriter = new StreamWriter(entryStream, Encoding.UTF8))
            {
                project.MergedInstruction.Save(streamWriter);
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
                zipEntryInstruction.Delete();
            }
        }
    }
}
