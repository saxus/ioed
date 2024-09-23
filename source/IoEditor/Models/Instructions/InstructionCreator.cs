using IoEditor.Models.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IoEditor.Models.Instructions
{
    internal class InstructionCreator
    {
        internal static Instruction LoadFromStream(Stream modelStream)
        {
            var xdoc = XDocument.Load(modelStream);

            return new Instruction(xdoc);
        }

        internal static Instruction CreateEmptyInstructionFromTemplate(Instruction template)
        {
            var globalSettings = template.Document.Root.Element("GlobalSetting");
            var customLayouts = template.Document.Root.Element("CustomLayouts");

            var xdoc = new XDocument(
                new XElement("Instruction", 
                [
                    new XElement(globalSettings),
                    new XElement("Pages"),
                    new XElement(customLayouts)
                ])
            );
            
            return new Instruction(xdoc);
        }
    }
}

