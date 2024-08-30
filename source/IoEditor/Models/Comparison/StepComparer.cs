using IoEditor.Models.Studio;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                // TODO
                var equality = StepEquality.Equal;

                var comparisonResult = new ComparisonStep()
                {
                    Index = result.Steps.Count,
                    Equality = equality,
                    ReferenceStep = refStep,
                    TargetStep = targetStep,
                };
                result.Steps.Add(comparisonResult);

                var tmpAllItems = targetStep.Submodels.Cast<IndexedStepItem>()
                    .Concat(targetStep.Parts.Cast<IndexedStepItem>());

                comparisonResult.UnmodifiedItems.AddRange(tmpAllItems);

                indexRef++;
                indexTarget++;
            }

            return result;
        }
    }
}
