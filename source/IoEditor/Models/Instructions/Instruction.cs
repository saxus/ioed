using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IoEditor.Models.Instructions
{
    internal class Instruction
    {
        public XmlDocument Document { get; }

        public XmlNode GlobalSettings { get; }
        public XmlNode CustomLayouts { get; }

        public List<Page> Pages { get; } = new List<Page>();
    }

    internal class Page : INotifyPropertyChanged
    {
        public PageLayout Layout { get; set; }

        public List<Slot> Slots { get; } = new List<Slot>();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal class Slot
    {
        public XmlNode XmlFragment { get; }

    }


    public enum PageLayout
    {
        Custom,
        Empty,
        OneByOne,
        OneByTwo,
        TwoByOne,
        OneByThree,
        ThreeByOne,
        TwoByTwo,
        TwoByTwo_Col,
        TwoByThree,
        ThreeByTwo
    }
}
