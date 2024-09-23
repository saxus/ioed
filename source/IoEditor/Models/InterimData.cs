using System.ComponentModel;
using System.Xml.Linq;

namespace IoEditor.Model
{
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