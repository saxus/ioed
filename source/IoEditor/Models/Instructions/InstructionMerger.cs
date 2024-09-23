using IoEditor.Model;

using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace IoEditor.Models.Instructions
{
    internal class InstructionMerger
    {
        public static void Merge(IoEdProject project)
        {
            var referenceInstruction = project.Reference.Instruction;
            var targetInstruction = project.Target.Instruction;

            var mergeModel = project.MergeModel;

            var stepDictionary = CreateStepDictionary(referenceInstruction);
            InterimStepData GetStepData(int index) => stepDictionary[index];

            project.InterimData.StepDictionary = stepDictionary;

            foreach (var segment in project.MergeModel.Segments)
            {
                Console.Write($"Merge segment {segment.SegmentName}: ");

                switch (segment.InstructionSegmentComparison.equality)
                {
                    case Models.Comparison.InstructionSegmentEquality.RemovedSegment:
                        Console.WriteLine("Removed, skip");
                        continue;

                    case Models.Comparison.InstructionSegmentEquality.NewSegment:
                        Console.WriteLine("New, insert new pages");
                        continue;

                    case Models.Comparison.InstructionSegmentEquality.Equivalent:
                        
                        foreach (var segmentStep in segment.TargetSegment.Steps)
                        {
                            var stepData = GetStepData(segmentStep.Index);

                            // TODO Find pages before this without steps
                            // TODO Create page with a proper layout
                            // TODO Copy step
                        }

                        Console.WriteLine("Equivalent, copying");
                        continue;

                    case Models.Comparison.InstructionSegmentEquality.Modified:
                        Console.WriteLine("Modified, thinking about that...");
                        continue;
                }
            }


        }

        private static Dictionary<int, InterimStepData> CreateStepDictionary(Instruction referenceInstruction)
        {
            var result = new Dictionary<int, InterimStepData>();
            var xpages = referenceInstruction.Document.Root.GetElement("Pages").GetElements("Page");

            foreach (var xpage in xpages)
            {
                var xslots = xpage.GetElements("Slot");
                foreach (var xslot in xslots)
                {
                    var xstep = xslot.GetElement("Step");
                    
                    var stepNumber = int.Parse(xstep.GetAttribute("SerializedIndex").Value);
                    result[stepNumber] = new InterimStepData(
                        xstep,
                        stepNumber,
                        false);
                    
                    var xcallout = xslot.Element("Callout");
                    if (xcallout != null)
                    {
                        var xcalloutSteps = xcallout.GetElement("CallOutItemData")
                                                   .GetElements("CallOutStepItemData")
                                                   .SelectMany(x => x.GetElements("Step"));

                        foreach (var xcalloutStep in xcalloutSteps)
                        {
                            var costepNumber = int.Parse(xstep.GetAttribute("SerializedIndex").Value);
                            result[costepNumber] = new InterimStepData(
                                xcalloutStep,
                                costepNumber,
                                true);
                        }
                    }
                }
            }

            return result;
        }
    }

    internal static class XExtensions
    {
        internal static XElement GetElement(this XElement element, string name) => element.Element(name) ?? throw new ArgumentNullException($"Missing element: {name}");
        internal static IEnumerable<XElement> GetElements(this XElement element, string name) => element.Elements(name) ?? throw new ArgumentNullException($"Missing elements: {name}");
        internal static XAttribute GetAttribute(this XElement element, string name) => element.Attribute(name) ?? throw new ArgumentNullException($"Missing attribute: {name}");
    }
}

