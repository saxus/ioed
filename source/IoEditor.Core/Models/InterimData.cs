using System.ComponentModel;
using System.Xml.Linq;

namespace IoEditor.Model
{
    /// <summary>Builds the per-step XML map used by the Step dictionary UI (same indexing rules as <c>InstructionMerger</c>).</summary>
    internal static class StepDictionaryBuilder
    {
        public static Dictionary<int, InterimStepData> FromInstructionDocument(XDocument document)
        {
            var result = new Dictionary<int, InterimStepData>();

            var xpages = document.Element("Instruction")?.Element("Pages")?.Elements("Page");
            if (xpages is null)
            {
                return result;
            }

            foreach (var xpage in xpages)
            {
                foreach (var xslot in xpage.Elements("Slot"))
                {
                    var xstep = xslot.Element("Step");
                    if (xstep is null)
                    {
                        continue;
                    }

                    var serializedIndex = Convert.ToInt32(xstep.Attribute("SerializedIndex")!.Value);

                    result.Add(serializedIndex, new InterimStepData(xstep, serializedIndex, false));

                    var xcallout = xstep.Element("CallOut");
                    if (xcallout is null)
                    {
                        continue;
                    }

                    foreach (var xcalloutItem in xcallout.Elements("CallOutItemData"))
                    {
                        var xcalloutSteps = xcalloutItem.Elements("CallOutStepItemData").Select(x => x.Element("Step"));
                        foreach (var xcalloutStep in xcalloutSteps)
                        {
                            if (xcalloutStep is null)
                            {
                                continue;
                            }

                            serializedIndex = Convert.ToInt32(xcalloutStep.Attribute("SerializedIndex")!.Value);

                            result.Add(serializedIndex, new InterimStepData(xcalloutStep, serializedIndex, true));
                        }
                    }
                }
            }

            if (result.Count == 0)
            {
                return result;
            }

            var maxIndex = result.Keys.Max();
            for (var i = 0; i <= maxIndex; i++)
            {
                if (!result.ContainsKey(i))
                {
                    throw new InvalidOperationException($"Step dictionary verification failed, serializedIndex {i} not found in the lookup table.");
                }
            }

            return result;
        }
    }

    internal class InterimData : INotifyPropertyChanged
    {
        private Dictionary<int, InterimStepData> _stepDictionary;
        public Dictionary<int, InterimStepData> StepDictionary
        {
            get => _stepDictionary;
            set
            {
                if (_stepDictionary != value)
                {
                    _stepDictionary = value;
                    RaisePropertyChanged(nameof(StepDictionary));
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal record class InterimStepData(
        XElement Element,
        int SerializedStepIndex,
        bool IsCallout);
}