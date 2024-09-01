using IoEditor.Models.ImageCache;
using IoEditor.Models.Model;

namespace IoEditor.Models.Studio
{
    internal class IndexedStepsBuilder
    {
        private readonly PartLibrary _partLibrary;
        private readonly ColorLibrary _colorLibrary;
        private readonly IBitmapImageProxyFactory _imageProxyFactory;

        public IndexedStepsBuilder(PartLibrary partLibrary, ColorLibrary colorLibrary, IBitmapImageProxyFactory imageProxyFactory)
        {
            _partLibrary = partLibrary;
            _colorLibrary = colorLibrary;
            _imageProxyFactory = imageProxyFactory;
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
                        else if (ldrawPart.IsCustomPart)
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
                existingSubModel = new IndexedStepSubmodel(model);

                indexedStep.Items.Add(existingSubModel);
            }

            existingSubModel.LDrawParts.Add(ldrawPart);
        }

        private void AddPartToIndexedStep(LDrawPart ldrawPart, IndexedStep indexedStep)
        {
            if (ldrawPart.IsCustomPart)
            {
                var color = _colorLibrary.GetColorByLDrawColorCode(ldrawPart.LDrawColorId);

                var existingCustomPart = indexedStep.CustomParts.FirstOrDefault(p => p.Part.PartName == ldrawPart.PartName && p.Color == color);
                if (existingCustomPart != null)
                {
                    existingCustomPart.LDrawParts.Add(ldrawPart);
                }
                else
                {
                    var stepItem = new IndexedStepCustomPart(ldrawPart.CustomPart, color);

                    stepItem.LDrawParts.Add(ldrawPart);
                    
                    indexedStep.Items.Add(stepItem);
                }
            }
            else
            {

                Part part = ldrawPart.IsOfficialPart
                    ? _partLibrary.GetPartByLDrawItemNo(ldrawPart.PartName)
                    : null;

                if (part == null)
                {
                    Console.WriteLine($"ERROR: PART NOT FOUND: {ldrawPart.PartName}, Line: {ldrawPart.LineInFile}");
                }

                var color = _colorLibrary.GetColorByLDrawColorCode(ldrawPart.LDrawColorId);

                var existingPart = indexedStep.Parts.FirstOrDefault(p => p.Part == part && p.Color == color);
                if (existingPart != null)
                {
                    existingPart.LDrawParts.Add(ldrawPart);
                }
                else
                {
                    var imageProxy = _imageProxyFactory.Create(part, color);
                    var stepItem = new IndexedStepPart(part, color, imageProxy);

                    stepItem.LDrawParts.Add(ldrawPart);

                    indexedStep.Items.Add(stepItem);
                }
            }
        }
    }
}
