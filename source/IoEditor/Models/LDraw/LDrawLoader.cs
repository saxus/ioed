using IoEditor.Models.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.LDraw
{
    internal class LDrawLoader
    {
        public static LDrawModel LoadFromStream(Stream modelStream)
        {
            using var reader = new LineReader(modelStream);

            var model = ReadModel(reader);

            return model;
        }


        private static LDrawModel ReadModel(LineReader reader)
        {
            var model = new LDrawModel()
            {
                Name = "Untitled Model",
            };

            var isParsingFinished = false;
            var currentStep = new LDrawStep();
            model.Steps.Add(currentStep);

            while (reader.CanRead)
            {
                var text = reader.ReadLine();
                var line = SplitLine(text);

                switch (line.type)
                {
                    case null:
                        continue;

                    case "0":
                        ParseMetaCommand(line.content);
                        break;

                    case "1":
                    case "10":
                        throw new NotImplementedException($"Line type not implemented: {line.type}");
                    case "11":
                        ParseSubFileLine(line.type, line.content);
                        break;
                }
            }

            if (!isParsingFinished)
            {
                throw new InvalidOperationException("Unexpected end of file.");
            }

            return model;


            // ---------------------------------------------------------------
            void ParseMetaCommand(string content)
            {
                content = content.Trim();

                if (content == "NOFILE")
                {
                    isParsingFinished = true;
                    return;
                }
                else if (content.StartsWith("FILE "))
                {
                    model.File = content.Substring(5);

                    if (!reader.TryReadLine(out var nextLine))
                    {
                        throw new InvalidOperationException("Unexpected end of file.");
                    }

                    var s = SplitLine(nextLine);
                    if (s.type == "0")
                    {
                        model.Descrption = s.content;
                    }
                }
                else if (content.StartsWith("Name: "))
                {
                    model.Name = content.Substring(6);
                }
                else if (content.StartsWith("Author: "))
                {
                    model.Author = content.Substring(8);
                }
                else if (content.Equals("STEP"))
                {
                    currentStep = new LDrawStep();
                    model.Steps.Add(currentStep);
                }
                else if (content.StartsWith("STUDIOSTEPDESC"))
                {
                    currentStep.Description = content.Substring(15);
                }
            }

            // ---------------------------------------------------------------
            void ParseSubFileLine(string type, string content)
            {
                var line = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var part = new LDrawPart()
                {
                    BricklinkColorId = ParseToInt(reader, line[0]),
                    PartName = line[16],
                };

                currentStep.Parts.Add(part);
            }
        }

        private static int ParseToInt(LineReader reader, string v)
        {
            if (int.TryParse(v, out int result))
            {
                return result;
            }
             
            throw new FormatException($"Failed to parse '{v}' as an integer at line {reader.CurrentLine}.");
        }

        private static float ParseToFloat(LineReader reader, string v)
        {
            if (float.TryParse(v, out float result))
            {
                return result;
            }

            throw new FormatException($"Failed to parse '{v}' as a float at line {reader.CurrentLine}.");
        }

        private static bool ParseToBool(LineReader reader, string v)
        {
            if (bool.TryParse(v, out bool result))
            {
                return result;
            }

            throw new FormatException($"Failed to parse '{v}' as a bool at line {reader.CurrentLine}.");
        }


        public static (string type, string content) SplitLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return (null, null);
            }

            // Split the line into two parts: the number and the content
            int firstSpaceIndex = line.IndexOf(' ');
            if (firstSpaceIndex == -1)
            {
                throw new FormatException("Line does not contain a space separator.");
            }

            string lineType = line.Substring(0, firstSpaceIndex);
            string lineContent = line.Substring(firstSpaceIndex + 1);

            return (lineType, lineContent);
        }
    }
}