using IoEditor.Models.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IoEditor.Models.Instructions
{
    internal class InstructionCreator
    {
        internal static Instruction LoadFromStream(Stream modelStream)
        {
            // This is important. One of Bricklink studio author
            // was an idiot who put a complete TSV file into an XML Attribute
            // in a non-standard way. The XDocument.Load wouldn't read it properly
            // so we have to use XmlDocument to load the stream and then convert it
            // to XDocument
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(modelStream);

            using var nodeReader = new XmlNodeReader(xmlDocument);
            nodeReader.MoveToContent();
            var xdoc = XDocument.Load(nodeReader);

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

