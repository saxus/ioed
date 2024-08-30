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
        public DataTemplate IndexedStepItemTemplate { get; set; }
        public DataTemplate IndexedStepSubmodelTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IndexedStepSubmodel)
            {
                return IndexedStepSubmodelTemplate;
            }
            else if (item is IndexedStepItem)
            {
                return IndexedStepItemTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
