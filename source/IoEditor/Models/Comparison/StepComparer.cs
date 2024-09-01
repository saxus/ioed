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

            var countRef = indexedReferenceSteps.Count;
            var countTarget = indexedTargetSteps.Count; 

            var indexRef = 0;
            var indexTarget = 0;

            while (true)
            {
                var refStep = indexRef < countRef ? indexedReferenceSteps[indexRef] : null;
                var targetStep = indexTarget < countTarget ? indexedTargetSteps[indexTarget] : null;

                if (refStep == null && targetStep == null)
                {
                    break;
                }

                // === EQUALITY DETERMINATION ====
                var comparisonResult = new ComparisonStep()
                {
                    Index = result.Steps.Count,
                };

                // TODO: current model model check to detect that are we in the same subassembly

                var comparedSteps = CompareSteps(refStep, targetStep);

                if (AreTheTwoStepExactlyTheSame(comparedSteps))
                {
                    comparisonResult.Equality = StepEquality.Equal;
                }
                else
                {
                    comparisonResult.Equality = StepEquality.ModifiedStep;
                }

                comparisonResult.ReferenceStep = refStep;
                comparisonResult.TargetStep = targetStep;
                comparisonResult.UnmodifiedItems.AddRange(comparedSteps.UnmodifiedItems);
                comparisonResult.RemovedItems.AddRange(comparedSteps.RemovedItems);
                comparisonResult.AddedItems.AddRange(comparedSteps.AddedItems);
                comparisonResult.ModifiedItems.AddRange(comparedSteps.ModifiedItems);

                // === END OF EQUALITY DETERMINATION ===
                result.Steps.Add(comparisonResult);

                indexRef++;
                indexTarget++;
            }

            return result;
        }

        private bool AreTheTwoStepExactlyTheSame(PartComparison comparedSteps)
        {
            return !comparedSteps.RemovedItems.Any()
                && !comparedSteps.AddedItems.Any()
                && !comparedSteps.ModifiedItems.Any();
        }

        private static PartComparison CompareSteps(IndexedStep refStep, IndexedStep targetStep)
        {
            var refItems = CreateComparisonList(refStep).ToList();
            var targetItems = CreateComparisonList(targetStep).ToList();

            var unmodifiedItems = new List<IndexedStepItem>();
            var removedItems = new List<IndexedStepItem>();
            var addedItems = new List<IndexedStepItem>();
            var modifiedItems = new List<IndexedStepItem>();

            var refItemCounts = refItems.GroupBy(item => item.Hash)
                                .ToDictionary(g => g.Key, g => g.ToList());

            var targetItemCounts = targetItems.GroupBy(item => item.Hash)
                                              .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var refItem in refItemCounts)
            {
                if (targetItemCounts.TryGetValue(refItem.Key, out var targetItemList))
                {
                    int refCount = refItem.Value.Count;
                    int targetCount = targetItemList.Count;

                    int unmodifiedCount = Math.Min(refCount, targetCount);
                    int addedCount = targetCount - unmodifiedCount;
                    int removedCount = refCount - unmodifiedCount;

                    unmodifiedItems.AddRange(refItem.Value.Take(unmodifiedCount).Select(x => x.StepItem));
                    removedItems.AddRange(refItem.Value.Skip(unmodifiedCount).Select(x => x.StepItem));
                    addedItems.AddRange(targetItemList.Skip(unmodifiedCount).Select(x => x.StepItem));

                    targetItemCounts.Remove(refItem.Key);
                }
                else
                {
                    removedItems.AddRange(refItem.Value.Select(x => x.StepItem));
                }
            }

            foreach (var targetItem in targetItemCounts)
            {
                addedItems.AddRange(targetItem.Value.Select(x => x.StepItem));
            }

            // TODO: DETECT RECOLOR based on Add/Remove


            return new PartComparison(unmodifiedItems, removedItems, addedItems, modifiedItems);
        }

        private static IEnumerable<ComparisonItem> CreateComparisonList(IndexedStep step)
        {
            if (step == null)
            {
                return Array.Empty<ComparisonItem>();
            }

            var comparisonItems = step.Items.Select(item => item switch
            {
                IndexedStepSubmodel submodel => new ComparisonItem(submodel.SimplifiedHash, submodel.ModelName, 0, submodel),
                IndexedStepPart part => new ComparisonItem(part.SimplifiedHash, part.Part.BLItemNo, part.Color.BLColorCode.Value, part),
                IndexedStepCustomPart customPart => new ComparisonItem(customPart.SimplifiedHash, customPart.Part.PartName, customPart.Color.BLColorCode.Value, customPart),
                _ => null
            }).Where(item => item != null);

            return comparisonItems.OrderBy(x => x.Hash);
        }

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
}
