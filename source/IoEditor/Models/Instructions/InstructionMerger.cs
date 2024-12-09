using IoEditor.Model;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IoEditor.Models.Instructions
{
    internal class InstructionMerger
    {
        public static XDocument Merge(IoEdProject project)
        {
            var referenceInstruction = project.Reference.Instruction;
            var targetInstruction = project.Target.Instruction;

            var mergeModel = project.MergeModel;

            var xdoc = CreateBasicStructure(project);

            return xdoc;
        }

        private static XDocument CreateBasicStructure(IoEdProject project)
        {
            return InstructionCreator.CreateEmptyInstructionFromTemplate(project.Target.Instruction).Document;
        }
    }
}
