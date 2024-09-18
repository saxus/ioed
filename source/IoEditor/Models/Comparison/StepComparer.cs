using IoEditor.Models.Model;
using IoEditor.Models.Studio;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace IoEditor.Models.Comparison
{
    internal class StepComparer
    {
        private IndexedStepsBuilder _stepBuilder;

        public StepComparer(IndexedStepsBuilder stepBuilder)
        {
            this._stepBuilder = stepBuilder;
        }

        public ComparisonResult Compare(StudioFile reference, StudioFile target)
        {
            var result = new ComparisonResult();

            var indexedReferenceSteps = this._stepBuilder.CreateIndexedSteps(reference.MainModel, reference);
            var indexedTargetSteps = this._stepBuilder.CreateIndexedSteps(target.MainModel, target);

            var allParts = indexedReferenceSteps.SelectMany(x => x.Items).ToList();

            var countRef = indexedReferenceSteps.Count;
            var countTarget = indexedTargetSteps.Count; 

            var indexRef = 0;
            var indexTarget = 0;

            Console.WriteLine($"==== COMPARING STEPS  ==========================");

            bool AreModelsEquals(string referenceModel, string targetModel)
            {
                var isBaseReferenceModel = indexedReferenceSteps.FirstOrDefault()?.Model == referenceModel;
                var isBaseTargetModel = indexedTargetSteps.FirstOrDefault()?.Model == targetModel;

                if (isBaseReferenceModel && isBaseTargetModel)
                {
                    return true;
                }

                return string.Equals(referenceModel, targetModel, StringComparison.OrdinalIgnoreCase);
            }
            
            Console.WriteLine($"==== COMPARING CONTINOUS MODEL SECTIONS  ==========================");

            var referenceSections = SplitToSections(reference.MainModel.Name, indexedReferenceSteps);
            var targetSections = SplitToSections(target.MainModel.Name, indexedTargetSteps);

            var comparedSections = CompareSections(referenceSections, targetSections);

            Console.WriteLine($"==== DONE  ==========================");

            result.InstructionSegments.AddRange(comparedSections);

            return result;
        }

        private List<InstructionSegmentComparison> 
            CompareSections(List<InstructionSegment> referenceSections, List<InstructionSegment> targetSections)
        {
            var referenceNames = referenceSections.Select(x => x.ModelName).ToArray();
            var targetNames = targetSections.Select(x => x.ModelName).ToArray();

            var diffResult = Differ.Diff(referenceNames, targetNames);

            var idxTarget = 0;
            var idxReference = 0;

            var result = new List<InstructionSegmentComparison>();

            void HandlePotentiallySymmetricSegment()
            {
                var (segmentEquality, diffMessage) = CompareSegments(referenceSections[idxReference], targetSections[idxTarget]);
                var prefix = (segmentEquality == InstructionSegmentEquality.Equivalent)
                    ? "   "
                    : " ? ";

                Console.WriteLine($"{prefix} {targetNames[idxTarget]}");
                if (segmentEquality == InstructionSegmentEquality.Modified)
                {
                    foreach (var line in diffMessage.Split('\n'))
                    {
                        Console.WriteLine($"    | {line}");
                    }
                }

                result.Add(new InstructionSegmentComparison(segmentEquality, 
                                                            referenceSections[idxReference], 
                                                            targetSections[idxTarget],
                                                            diffMessage));

                idxTarget++;
                idxReference++;
            }

            foreach (var diff in diffResult)
            {
                while (idxTarget < diff.StartB)
                {
                    HandlePotentiallySymmetricSegment();
                }

                for (int i = 0; i < diff.deletedA; i++)
                {
                    Console.WriteLine($"--- {referenceNames[diff.StartA + i]}");

                    result.Add(new InstructionSegmentComparison(InstructionSegmentEquality.RemovedSegment, 
                                                                referenceSections[diff.StartA + i], 
                                                                null,
                                                                null));
                    idxReference++;

                }

                for (int i = 0; i < diff.insertedB; i++)
                {
                    Console.WriteLine($"+++ {targetNames[idxTarget]}");
                    result.Add(new InstructionSegmentComparison(InstructionSegmentEquality.RemovedSegment,
                                                                null, 
                                                                targetSections[idxTarget],
                                                                null));

                    idxTarget++;
                }
            }

            while (idxTarget < targetNames.Length)
            {
                HandlePotentiallySymmetricSegment();
            }

            return result;
        }

        private (InstructionSegmentEquality equality, string debugResult) 
            CompareSegments(InstructionSegment reference, InstructionSegment target)
        {
            var sb = new StringBuilder();

            if (reference.Steps.Count != target.Steps.Count)
            {
                sb.AppendLine($"Step count difference: {reference.Steps.Count} -> {target.Steps.Count}");

                return (InstructionSegmentEquality.Modified, sb.ToString());
            }

            var result = InstructionSegmentEquality.Equivalent;
            var index = 0;

            foreach (var (refStep, targetStep) in reference.Steps.Zip(target.Steps))
            {
                index++;

                if (refStep.Items.Count != targetStep.Items.Count)
                {
                    sb.AppendLine($"Step #{index}: different part count in step: {refStep.Items.Count} -> {refStep.Items.Count}");
                    result = InstructionSegmentEquality.Modified;
                }
                else
                {
                    sb.AppendLine($"Step #{index}");

                    var orderedRefStep = refStep.Items.OrderBy(item => item, new IndexedStepItemComparer());
                    var orderedTargetStep = targetStep.Items.OrderBy(item => item, new IndexedStepItemComparer());

                    foreach (var (refItem, targetItem) in orderedRefStep.Zip(orderedTargetStep))
                    {
                        sb.Append($"  {refItem.LDrawPartName} ");

                        if (!StepItemsAreEquivalent(refItem, targetItem))
                        {
                            result = InstructionSegmentEquality.Modified;
                            
                            sb.AppendLine($"[{refItem.LDrawPart.Position}] -> {targetItem.LDrawPartName} [{targetItem.LDrawPart.Position}]");
                        }
                        else
                        {
                            sb.AppendLine();
                        }
                    }
                }
            }

            return (result, sb.ToString());

        }

        private bool StepItemsAreEquivalent(IndexedStepItem refItem, IndexedStepItem targetItem)
        {
            const double tolerance = 0.01;

            bool ArePositionsEqual(Vector3 refPosition, Vector3 targetPosition)
            {
                return Math.Abs(refPosition.X - targetPosition.X) < tolerance &&
                       Math.Abs(refPosition.Y - targetPosition.Y) < tolerance &&
                       Math.Abs(refPosition.Z - targetPosition.Z) < tolerance;
            }

            bool AreRotationsEqual(Matrix3x3 refRotation, Matrix3x3 targetRotation)
            {
                // Assuming Matrix3x3 has a method to get individual elements or a way to compare with tolerance
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (Math.Abs(refRotation[i, j] - targetRotation[i, j]) >= tolerance)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }

            return refItem.LDrawPartName == targetItem.LDrawPartName
                   && ArePositionsEqual(refItem.LDrawPart.Position, targetItem.LDrawPart.Position)
                   && AreRotationsEqual(refItem.LDrawPart.Rotation, targetItem.LDrawPart.Rotation);
        }

        private List<InstructionSegment> SplitToSections(string mainModel, List<IndexedStep> indexedTargetSteps)
        {
            var result = new List<InstructionSegment>();
            if (indexedTargetSteps == null || indexedTargetSteps.Count == 0)
            {
                return result;
            }

            List<IndexedStep> currentSegmentSteps = new List<IndexedStep>();
            string currentModelName = null;

            const string mainModelPlaceholder = "**[main]**";

            foreach (var step in indexedTargetSteps)
            {
                if (currentModelName == null || step.Model != currentModelName)
                {
                    if (currentSegmentSteps.Count > 0)
                    {
                        result.Add(new InstructionSegment(
                            currentModelName == mainModel ? mainModelPlaceholder : currentModelName,
                            new List<IndexedStep>(currentSegmentSteps)
                        ));
                        currentSegmentSteps.Clear();
                    }
                    currentModelName = step.Model;
                }
                currentSegmentSteps.Add(step);
            }

            // Add the last segment
            if (currentSegmentSteps.Count > 0)
            {
                result.Add(new InstructionSegment(
                    currentModelName == mainModel ? mainModelPlaceholder : currentModelName,
                    new List<IndexedStep>(currentSegmentSteps)
                ));
            }

            return result;
        }
    }
}
