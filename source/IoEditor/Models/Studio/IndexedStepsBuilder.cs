using IoEditor.Models.Model;

namespace IoEditor.Models.Studio
{
    internal class IndexedStepsBuilder
    {
        private readonly PartLibrary _partLibrary;
        private readonly ColorLibrary _colorLibrary;

        public IndexedStepsBuilder(PartLibrary partLibrary, ColorLibrary colorLibrary)
        {
            _partLibrary = partLibrary;
            _colorLibrary = colorLibrary;
        }

        public List<IndexedStep> CreateIndexedSteps(LDrawModel ldrawModel, StudioFile studioFile)
        {
            var indexedSteps = new List<IndexedStep>();
            
            ExtractModel(ldrawModel);

            for (int i = 0; i < indexedSteps.Count; i++)
            {
                indexedSteps[i].Index = i;
            }

            return indexedSteps;

            void ExtractModel(LDrawModel ldrawModel)
            {
                foreach (var step in ldrawModel.Steps)
                {
                    var indexedStep = new IndexedStep
                    {
                        LDrawStep = step
                    };

                    foreach (var ldrawPart in step.Parts)
                    {
                        if (ldrawPart.IsOfficialPart)
                        {
                            AddPartToIndexedStep(ldrawPart, indexedStep);
                        }
                        else if (ldrawPart.Model != null)
                        {
                            if (!indexedStep.Submodels.Any(x => x.Model == ldrawPart.Model))
                            {
                                ExtractModel(ldrawPart.Model);
                            }
                            AddSubModelToIndexedStep(ldrawPart, indexedStep, studioFile);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Not an official part without model: {ldrawPart.PartName}");
                        }
                    }

                    indexedSteps.Add(indexedStep);
                }
            }
        }

        private void AddSubModelToIndexedStep(LDrawPart ldrawPart, IndexedStep indexedStep, StudioFile studioFile)
        {
            var model = studioFile.GetModel(ldrawPart.PartName) ?? throw new InvalidOperationException($"model not found: {ldrawPart.PartName}!");

            var existingSubModel = indexedStep.Submodels.FirstOrDefault(x => x.Model == model);
            if (existingSubModel == null)
            {
                existingSubModel = new IndexedStepSubmodel()
                {
                    Model = model
                };
                indexedStep.Submodels.Add(existingSubModel);
            }

            existingSubModel.Quantity++;
            existingSubModel.LDrawParts.Add(ldrawPart);
        }

        private void AddPartToIndexedStep(LDrawPart ldrawPart, IndexedStep indexedStep)
        {
            Part part = ldrawPart.IsOfficialPart
                ? _partLibrary.GetPartByLDrawItemNo(ldrawPart.PartName)
                : null;

            var color = _colorLibrary.GetColorByLDrawColorCode(ldrawPart.LDrawColorId);

            var existingPart = indexedStep.Parts.FirstOrDefault(p => p.Part == part && p.Color == color);
            if (existingPart != null)
            {
                existingPart.Quantity++;
                existingPart.LDrawParts.Add(ldrawPart);
            }
            else
            {
                indexedStep.Parts.Add(new IndexedStepPart
                {
                    Part = part,
                    Color = color,
                    Quantity = 1,
                    LDrawParts = new List<LDrawPart> { ldrawPart }
                });
            }
        }
    }
}
