using IoEditor.Models.Studio;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace IoEditor.UI.MainWindow
{
    public class IndexedStepItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate IndexedStepPartTemplate { get; set; }
        public DataTemplate IndexedStepSubmodelTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IndexedStepSubmodel)
            {
                return IndexedStepSubmodelTemplate;
            }
            else if (item is IndexedStepPart)
            {
                return IndexedStepPartTemplate;
            }
            else
            {
                Console.WriteLine($"Unknown item type: {item.GetType()}");
            }

            return base.SelectTemplate(item, container);
        }
    }
}
