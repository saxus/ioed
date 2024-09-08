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

                foreach (var item in indexedSteps[i].Items)
                {
                    item.StepIndex = i;
                }
            }

            return indexedSteps;

            void ExtractModel(LDrawModel ldrawModel)
            {
                foreach (var step in ldrawModel.Steps)
                {
                    var indexedStep = new IndexedStep
                    {
                        Model = ldrawModel.Name,
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
            var stepItem = new IndexedStepSubmodel(ldrawPart, indexedStep.Model, model);

            indexedStep.Items.Add(stepItem);
        }

        private void AddPartToIndexedStep(LDrawPart ldrawPart, IndexedStep indexedStep)
        {
            if (ldrawPart.IsCustomPart)
            {
                var color = _colorLibrary.GetColorByLDrawColorCode(ldrawPart.LDrawColorId);
                var stepItem = new IndexedStepCustomPart(ldrawPart, indexedStep.Model, ldrawPart.CustomPart, color);

                indexedStep.Items.Add(stepItem);
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

                var imageProxy = _imageProxyFactory.Create(part, color);
                var stepItem = new IndexedStepPart(ldrawPart, indexedStep.Model, part, color, imageProxy);

                indexedStep.Items.Add(stepItem);
            }
        }
    }
}
