using IoEditor.Model;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Xml.Linq;

namespace IoEditor.Models.Instructions
{
    internal class InstructionMerger
    {
        public static (XDocument instruction, Dictionary<string, byte[]> imageResources)
            Merge(IoEdProject project)
        {
            var mergeLogic = new MergeLogic(project);

            return mergeLogic.Merge();
        }


        private class MergeLogic
        {
            private bool writeDebugInfo = true;

            private readonly IoEdProject project;
            private Instruction referenceInstruction => project.Reference.Instruction;
            private Instruction targetInstruction => project.Target.Instruction;

            private readonly ImageResourceMerger imageResourcesMerger;
            private readonly Dictionary<XElement, List<XElement>> predecessorPages;
            private readonly Dictionary<int, IndexedStepData> referenceStepLookupTable;
            private readonly Dictionary<int, IndexedStepData> targetStepLookupTable;
            
            private readonly XDocument xdoc;            
            private readonly XElement xResultPages;

            private int pageNumber = 0;

            public MergeLogic(IoEdProject project)
            {
                this.project = project;
                this.imageResourcesMerger = new ImageResourceMerger(project);

                this.predecessorPages = FindPredecessorPages(project.Reference.Instruction.Document);

                this.referenceStepLookupTable = BuildIndexedStepData(project.Reference.Instruction.Document);
                this.targetStepLookupTable = BuildIndexedStepData(project.Target.Instruction.Document);

                this.xdoc = CreateBasicStructure();
                this.xResultPages = xdoc.Element("Instruction").Element("Pages");
            }
           

            public (XDocument instruction, Dictionary<string, byte[]> imageResources)
                Merge()
            {
                var globalSettings = xdoc.Element("Instruction").Element("GlobalSetting");
                FindAndRegisterImages(globalSettings, ImageSource.Reference);

                // var xPages = project.Reference.Instruction.Document.Element("Instruction").Element("Pages").Elements("Page").ToList();
                var writeDebugInfo = true;

                

                var serializedIndexLookupTable = new Dictionary<int, int>();

                foreach ((var segment, var segmentIndex) in project.MergeModel.Segments.Select((x, i) => (x, i)))
                {
                    var xlastProcessedPage = (XElement)null;
                    var xlastCreatedPage = (XElement)null;

                    switch (segment.Equality)
                    {
                        case Models.Comparison.InstructionSegmentEquality.RemovedSegment:
                            WriteDebug($"REMOVED Segment #{segmentIndex}, {segment.SegmentName}");
                            continue;

                        case Models.Comparison.InstructionSegmentEquality.NewSegment:
                            WriteDebug($"NEW segment #{segmentIndex}, {segment.SegmentName}");

                            foreach (var (idx, targetStep) in segment.TargetSegment.Steps.Select((x, i) => (i, x)))
                            {
                                var refStepData = targetStepLookupTable[idx];
                                if (refStepData.IsCallout)
                                {
                                    //AddPredecessorPagesIfNecessary(predecessorPages, imageResourcesMerger, ImageSource.Target, xResultPages, refStepData.Page);

                                    continue;
                                }
                                else
                                {
                                    if (xlastProcessedPage != refStepData.Page)
                                    {
                                        //AddPredecessorPagesIfNecessary(predecessorPages, imageResourcesMerger, ImageSource.Target, xResultPages, refStepData.Page);

                                        xlastProcessedPage = refStepData.Page;
                                        xlastCreatedPage = new XElement("Page");
                                        xlastCreatedPage.SetAttributeValue("template", refStepData.Page.Attribute("template")?.Value ?? "OneByOne");
                                        xlastCreatedPage.SetAttributeValue("IsLocked", refStepData.Page.Attribute("IsLocked")?.Value ?? "false");
                                        AddPage(xlastCreatedPage);
                                    }

                                    FindAndRegisterImages(refStepData.Slot, ImageSource.Reference);

                                    xlastCreatedPage.Add(new XElement(refStepData.Slot));
                                }
                            }

                            continue;

                        case Models.Comparison.InstructionSegmentEquality.Equivalent:
                            WriteDebug($"EQUIVALENT segment #{segmentIndex}, {segment.SegmentName}");

                            foreach (var (idx, targetStep) in segment.TargetSegment.Steps.Select((x, i) => (i, x)))
                            {
                                var referenceStep = segment.ReferenceSegment.Steps[idx];

                                var refStepData = referenceStepLookupTable[referenceStep.Index];
                                if (refStepData.IsCallout)
                                {
                                    AddPredecessorPagesIfNecessary(ImageSource.Reference, refStepData.Page);

                                    serializedIndexLookupTable[referenceStep.Index] = targetStep.Index;
                                    continue;
                                }
                                else
                                {
                                    if (xlastProcessedPage != refStepData.Page)
                                    {
                                        AddPredecessorPagesIfNecessary(ImageSource.Reference, refStepData.Page);

                                        xlastProcessedPage = refStepData.Page;
                                        xlastCreatedPage = new XElement("Page");
                                        xlastCreatedPage.SetAttributeValue("template", refStepData.Page.Attribute("template")?.Value ?? "OneByOne");
                                        xlastCreatedPage.SetAttributeValue("IsLocked", refStepData.Page.Attribute("IsLocked")?.Value ?? "false");
                                        CopyAttribute(refStepData.Page, "resizeBars", xlastCreatedPage);
                                        AddPage(xlastCreatedPage);
                                    }

                                    var newSlot = new XElement(refStepData.Slot);
                                    CopyAttributes(refStepData.Slot, newSlot);

                                    // TODO: copy slot and step attributes
                                    var newStep = newSlot.Element("Step");
                                    newStep.AddBeforeSelf(new XComment($"SerializedIndex: {referenceStep.Index} = {targetStep.Index}"));
                                    newStep.SetAttributeValue("SerializedIndex", targetStep.Index);

                                    var callout = newStep.Element("CallOut");
                                    if (callout != null)
                                    {
                                        foreach (var cid in callout.Elements("CallOutItemData"))
                                        {
                                            foreach (var csd in cid.Elements("CallOutStepItemData"))
                                            {
                                                var cstep = csd.Element("Step");
                                                var cstepSerializedIndex = Convert.ToInt32(cstep.Attribute("SerializedIndex").Value);

                                                var targetIndex = serializedIndexLookupTable[cstepSerializedIndex];

                                                cstep.AddBeforeSelf(new XComment($"SerializedIndex: {cstepSerializedIndex} = {targetIndex}"));
                                                cstep.SetAttributeValue("SerializedIndex", targetIndex);
                                            }
                                        }
                                    }

                                    FindAndRegisterImages(newSlot, ImageSource.Reference);

                                    xlastCreatedPage.Add(newSlot);
                                }
                            }

                            continue;

                        case Models.Comparison.InstructionSegmentEquality.Modified:
                            WriteDebug($"MODIFIED segment #{segmentIndex}, {segment.SegmentName}");
                            continue;
                    }
                }

                // Add last pages without steps
                var lastPagesWithoutSteps = FindLastPagesWithoutSteps(project.Reference.Instruction.Document);
                if (lastPagesWithoutSteps.Any())
                {
                    foreach (var p in lastPagesWithoutSteps)
                    {
                        FindAndRegisterImages(p, ImageSource.Reference);

                        AddPage(new XElement(p));
                    }
                }

                return (xdoc, imageResourcesMerger.GetMergedDictionary());
            }

            private void AddPredecessorPagesIfNecessary(
                ImageSource source,
                XElement page)
            {
                if (predecessorPages.TryGetValue(page, out var predPages))
                {
                    foreach (var predPage in predPages)
                    {
                        FindAndRegisterImages(predPage, source);
                        AddPage(new XElement(predPage));
                    }
                }
            }

            private void FindAndRegisterImages(XElement p, ImageSource source)
            {
                if (p.Name.LocalName == "Image")
                {
                    var imagePath = p.Attribute("imagePath")?.Value;
                    if (imagePath != null)
                    {
                        var newPath = imageResourcesMerger.RegisterImage(source, imagePath);
                        Console.WriteLine($"REGISTERED IMAGE: {imagePath} -> {newPath}");
                        
                        p.SetAttributeValue("imagePath", newPath);
                    }
                }
                else if (p.Name.LocalName == "Colors" && p.Attribute("BgImage") != null)
                {
                    var imagePath = p.Attribute("BgImage").Value;

                    var newPath = imageResourcesMerger.RegisterImage(source, imagePath);
                    Console.WriteLine($"REGISTERED IMAGE: {imagePath} -> {newPath}");

                    p.SetAttributeValue("BgImage", newPath);
                }
                else
                {
                    foreach (var e in p.Elements())
                    {
                        FindAndRegisterImages(e, source);
                    }
                }
            }

            private static void CopyAttribute(XElement source, string attributeName, XElement target)
            {
                var attr = source.Attribute(attributeName);
                if (attr != null)
                {
                    target.SetAttributeValue(attr.Name, attr.Value);
                }
            }

            private static void CopyAttributes(XElement source, XElement target)
            {
                foreach (var attr in source.Attributes())
                {
                    target.SetAttributeValue(attr.Name, attr.Value);
                }
            }

            private static Dictionary<int, IndexedStepData> BuildIndexedStepData(XDocument document)
            {
                var result = new Dictionary<int, IndexedStepData>();

                var xpages = document.Element("Instruction").Element("Pages").Elements("Page");

                foreach (var xpage in xpages)
                {
                    foreach (var xslot in xpage.Elements("Slot"))
                    {
                        var xstep = xslot.Element("Step");
                        if (xstep == null)
                        {
                            continue;
                        }

                        var serializedIndex = Convert.ToInt32(xstep.Attribute("SerializedIndex").Value);

                        result.Add(serializedIndex, new IndexedStepData(serializedIndex, xpage, xslot, xstep, null, null));

                        var xcallout = xstep.Element("CallOut");
                        if (xcallout != null)
                        {
                            var xcalloutItemData = xcallout.Elements("CallOutItemData");

                            foreach (var xcalloutItem in xcalloutItemData)
                            {
                                var xcalloutSteps = xcalloutItem.Elements("CallOutStepItemData").Select(x => x.Element("Step"));
                                foreach (var xcalloutStep in xcalloutSteps)
                                {
                                    serializedIndex = Convert.ToInt32(xcalloutStep.Attribute("SerializedIndex").Value);

                                    result.Add(serializedIndex, new IndexedStepData(serializedIndex, xpage, xslot, xcalloutStep, xcallout, xstep));
                                }
                            }
                        }
                    }
                }

                var maxIndex = result.Keys.Max();
                for (int i = 0; i <= maxIndex; i++)
                {
                    if (!result.ContainsKey(i))
                    {
                        throw new InvalidOperationException($"Lookup table verification failed, serializedIndex {i} not found in the lookup table!");
                    }
                }

                return result;
            }

            private static Dictionary<XElement, List<XElement>> FindPredecessorPages(XDocument document)
            {
                var result = new Dictionary<XElement, List<XElement>>();
                var xpages = document.Element("Instruction").Element("Pages").Elements("Page");

                var pagesBefore = new List<XElement>();

                foreach (var xpage in xpages)
                {
                    if (!PageHaveSteps(xpage))
                    {
                        pagesBefore.Add(xpage);
                    }
                    else
                    {
                        result.Add(xpage, pagesBefore);
                        pagesBefore = new List<XElement>();
                    }
                }

                return result;
            }

            private static List<XElement> FindLastPagesWithoutSteps(XDocument document)
            {
                var xpages = document.Element("Instruction").Element("Pages").Elements("Page");

                return xpages.Reverse().TakeWhile(x => !PageHaveSteps(x)).Reverse().ToList();
            }


            internal record class IndexedStepData
                (int Index,
                 XElement Page,
                 XElement Slot,
                 XElement Step,
                 XElement Callout,
                 XElement ParentStep)
            {
                public bool IsCallout => Callout != null;
            }


            private static bool SlotHaveStep(XElement xSlot, out XElement? xStep)
            {
                xStep = xSlot.Element("Step");
                return xStep != null;
            }

            private static bool PageHaveSteps(XElement xPage)
                => xPage.Elements("Slot").Any(x => x.Element("Step") != null);

            
            private void WriteDebug(string msg)
            {
                Console.WriteLine(msg);
                if (writeDebugInfo)
                {
                    xResultPages.Add(new XComment(msg));
                }
            }

            private void AddPage(XElement element)
            {
                if (writeDebugInfo)
                {
                    xResultPages.Add(new XComment($"Page #{pageNumber}"));
                }
                pageNumber++;
                xResultPages.Add(element);
            }


            private XDocument CreateBasicStructure()
            {
                var result = InstructionCreator.CreateEmptyInstructionFromTemplate(referenceInstruction).Document;
                return result;
            }
        }
    }
}
