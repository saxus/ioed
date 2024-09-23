using IoEditor.Models.Instructions;
using IoEditor.Models.Studio;

using System.IO;

namespace IoEditor.Model
{
    internal static class IoEdProjectLoader
    {
        public static IoEdProject Load(string referenceFilePath, string targetFilePath)
        {
            if (string.IsNullOrWhiteSpace(referenceFilePath)) throw new ArgumentException("Reference file path cannot be null or empty.", nameof(referenceFilePath));
            if (string.IsNullOrWhiteSpace(targetFilePath)) throw new ArgumentException("Target file path cannot be null or empty.", nameof(targetFilePath));
            if (referenceFilePath == targetFilePath) throw new ArgumentException("Reference and target files cannot be the same.");
            if (!File.Exists(referenceFilePath)) throw new FileNotFoundException("Reference file not found.", referenceFilePath);
            if (!File.Exists(targetFilePath)) throw new FileNotFoundException("Target file not found.", targetFilePath);

            var referenceStudioFile = StudioFileLoader.Load(referenceFilePath);
            var targetStudioFile = StudioFileLoader.Load(targetFilePath);

            if (referenceStudioFile.Instruction == null)
            {
                throw new InvalidOperationException("Reference file does not contain an instruction file.");
            }

            targetStudioFile.Instruction = InstructionCreator.CreateEmptyInstructionFromTemplate(referenceStudioFile.Instruction);

            return new IoEdProject(referenceFilePath, targetFilePath, referenceStudioFile, targetStudioFile);
        }
    }
}
