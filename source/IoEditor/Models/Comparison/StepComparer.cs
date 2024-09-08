using IoEditor.Models.Model;
using IoEditor.Models.Studio;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            var refStepIndex = 0;
            
            foreach (var targetStep in indexedTargetSteps)
            {
                Console.WriteLine($"COMPARING STEP #{targetStep.Index}, model: {targetStep.LDrawStep}");

                var comparisonResult = new ComparisonStep()
                {
                    Index = targetStep.Index,
                    TargetStep = targetStep,
                };

                var refStep = indexedReferenceSteps[refStepIndex];

                foreach (var targetPart in targetStep.Items)
                {
                    Console.WriteLine($"  Looking for part {targetPart.LDrawPartName} @ {targetPart.LDrawPart.Position} x {targetPart.LDrawPart.TransformationMatrix}");

                    var equalParts = allParts.Where(x => AreModelsEquals(x.ParentModel, targetStep.Model)
                                            && x.LDrawPart.EqualsWithCoordinates(targetPart.LDrawPart))
                                            .ToList();

                    if (equalParts.Count > 1)
                    {
                        Console.WriteLine($"    !!! Possible duplicated parts found {equalParts.Count}");                        
                    }
                    else if (equalParts.Count > 0)
                    {
                        var equalPart = equalParts.FirstOrDefault();

                        Console.WriteLine($"    Part found: {equalPart.LDrawPartName} @ {equalPart.StepIndex}");

                        comparisonResult.UnmodifiedItems.Add(targetPart);
                    }
                    else
                    {
                        Console.WriteLine($"    Part not found.");

                        comparisonResult.UnmodifiedItems.Add(targetPart);
                    }
                }


                result.Steps.Add(comparisonResult);
               
            }

            Console.WriteLine($"==== DONE  ==========================");



            // while (true)
            // {
            //     var refStep = indexRef < countRef ? indexedReferenceSteps[indexRef] : null;
            //     var targetStep = indexTarget < countTarget ? indexedTargetSteps[indexTarget] : null;
            // 
            //     if (refStep == null && targetStep == null)
            //     {
            //         break;
            //     }
            // 
            //     // === EQUALITY DETERMINATION ====
            //     var comparisonResult = new ComparisonStep()
            //     {
            //         Index = result.Steps.Count,
            //     };
            // 
            //     // TODO: current model model check to detect that are we in the same subassembly
            // 
            //     var comparedSteps = CompareSteps(refStep, targetStep);
            // 
            //     if (AreTheTwoStepExactlyTheSame(comparedSteps))
            //     {
            //         comparisonResult.Equality = StepEquality.Equal;
            //     }
            //     else
            //     {
            //         comparisonResult.Equality = StepEquality.ModifiedStep;
            //     }
            // 
            //     comparisonResult.ReferenceStep = refStep;
            //     comparisonResult.TargetStep = targetStep;
            //     comparisonResult.UnmodifiedItems.AddRange(comparedSteps.UnmodifiedItems);
            //     comparisonResult.RemovedItems.AddRange(comparedSteps.RemovedItems);
            //     comparisonResult.AddedItems.AddRange(comparedSteps.AddedItems);
            //     comparisonResult.ModifiedItems.AddRange(comparedSteps.ModifiedItems);
            // 
            //     // === END OF EQUALITY DETERMINATION ===
            //     result.Steps.Add(comparisonResult);
            // 
            //     indexRef++;
            //     indexTarget++;
            // }

            return result;
        }

        private bool AreTheTwoStepExactlyTheSame(PartComparison comparedSteps)
        {
            return !comparedSteps.RemovedItems.Any()
                && !comparedSteps.AddedItems.Any()
                && !comparedSteps.ModifiedItems.Any();
        }

        // private static PartComparison CompareSteps(IndexedStep refStep, IndexedStep targetStep)
        // {
        //     var refItems = CreateComparisonList(refStep).ToList();
        //     var targetItems = CreateComparisonList(targetStep).ToList();
        // 
        //     var unmodifiedItems = new List<IndexedStepItem>();
        //     var removedItems = new List<IndexedStepItem>();
        //     var addedItems = new List<IndexedStepItem>();
        //     var modifiedItems = new List<IndexedStepItem>();
        // 
        //     
        // 
        // 
        //     return new PartComparison(unmodifiedItems, removedItems, addedItems, modifiedItems);
        // }

        // private static IEnumerable<ComparisonItem> CreateComparisonList(IndexedStep step)
        // {
        //     if (step == null)
        //     {
        //         return Array.Empty<ComparisonItem>();
        //     }
        // 
        //     var comparisonItems = step.Items.Select(item => item switch
        //     {
        //         IndexedStepSubmodel submodel => new ComparisonItem(submodel.SimplifiedHash, submodel.ModelName, 0, submodel),
        //         IndexedStepPart part => new ComparisonItem(part.SimplifiedHash, part.Part.BLItemNo, part.Color.BLColorCode.Value, part),
        //         IndexedStepCustomPart customPart => new ComparisonItem(customPart.SimplifiedHash, customPart.Part.PartName, customPart.Color.BLColorCode.Value, customPart),
        //         _ => null
        //     }).Where(item => item != null);
        // 
        //     return comparisonItems.OrderBy(x => x.Hash);
        // }

        private record PartComparison(
            List<IndexedStepItem> UnmodifiedItems,
            List<IndexedStepItem> RemovedItems,
            List<IndexedStepItem> AddedItems,
            List<IndexedStepItem> ModifiedItems
        );

        private record ComparisonItem(
            string Hash,
            string Uncolored,
            int Color,
            IndexedStepItem StepItem
        );
    }

    internal static class LDrawPartExtensions
    {
        public static bool EqualsWithCoordinates(this LDrawPart a, LDrawPart b)
        {
            if (a == null || b == null)
                return false;

            // Check if the model names are equal
            if (!string.Equals(a.Model?.Name, b.Model?.Name, StringComparison.OrdinalIgnoreCase))
                return false;

            // Check if the positions are equal
            if (a.Position != b.Position)
                return false;

            // Check if the rotation matrices are equal
            if (!a.Rotation.Equals(b.Rotation))
                return false;

            return true;
        }
    }
}
