using IoEditor.Models.Model;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.LDraw
{
    internal class LDrawLoader
    {
        public static (LDrawModel mainModel, List<LDrawModel> allModels) 
            ReadModels(LineReader reader, List<LDrawCustomPart>  customParts)
        {
            var customPartsDic = customParts.ToDictionary(x => x.PartName, x => x);

            var models = new List<LDrawModel>();
            LDrawModel currentModel = null;
            LDrawModel mainModel = null;

            var isParsingFinished = false;
            var currentStep = new LDrawStep();

            var lineIndex = 0;

            while (reader.CanRead)
            {
                lineIndex++;
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

            // Add the last model if it exists
            if (currentModel != null)
            {
                models.Add(currentModel);
            }

            foreach (var model in models)
            {
                FixModelsLastStep(model);
            }

            return (mainModel, models);

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
                    if (currentModel != null)
                    {
                        models.Add(currentModel);
                    }

                    currentModel = new LDrawModel
                    {
                        File = content.Substring(5),
                        Name = content.Substring(5),
                    };

                    // Set the mainModel when the first model is instantiated
                    if (mainModel == null)
                    {
                        mainModel = currentModel;
                    }

                    currentStep = new LDrawStep();
                    currentModel.Steps.Add(currentStep);

                    if (!reader.TryReadLine(out var nextLine))
                    {
                        throw new InvalidOperationException("Unexpected end of file.");
                    }
                    lineIndex++;

                    var s = SplitLine(nextLine);
                    if (s.type == "0")
                    {
                        currentModel.Descrption = s.content;
                    }
                }
                else if (content.StartsWith("Name: "))
                {
                    currentModel.Name = content.Substring(6).Trim();
                }
                else if (content.StartsWith("Author: "))
                {
                    currentModel.Author = content.Substring(8);
                }
                else if (content.Equals("STEP"))
                {
                    currentStep = new LDrawStep();
                    currentModel.Steps.Add(currentStep);
                }
                else if (content.StartsWith("STUDIOSTEPDESC"))
                {
                    currentStep.Description = content.Substring(15);
                }
            }

            // ---------------------------------------------------------------
            void ParseSubFileLine(string type, string content)
            {
                var line = content.Split(' ', 17, StringSplitOptions.RemoveEmptyEntries);

                customPartsDic.TryGetValue(line[16], out var customPart);

                var position = new Vector3(
                    x: ParseToFloat(reader, line[4]), 
                    y: ParseToFloat(reader, line[5]), 
                    z: ParseToFloat(reader, line[6]));

                var rotation = new Matrix3x3(
                    m11: ParseToFloat(reader, line[7]),
                    m12: ParseToFloat(reader, line[8]),
                    m13: ParseToFloat(reader, line[9]),
                    m21: ParseToFloat(reader, line[10]),
                    m22: ParseToFloat(reader, line[11]),
                    m23: ParseToFloat(reader, line[12]),
                    m31: ParseToFloat(reader, line[13]),
                    m32: ParseToFloat(reader, line[14]),
                    m33: ParseToFloat(reader, line[15])); 

                var part = new LDrawPart()
                {
                    LineInFile = lineIndex,
                    LDrawColorId = ParseToInt(reader, line[0]),
                    PartName = line[16], 
                    Position = position,
                    Rotation = rotation,
                    CustomPart = customPart,
                };

                currentStep.Parts.Add(part);
            }
        }


        private static void FixModelsLastStep(LDrawModel model)
        {
            if (model.Steps.Count > 1 &&
                !model.Steps.Last().Parts.Any())
            {
                model.Steps.RemoveAt(model.Steps.Count - 1);
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
            if (float.TryParse(v, CultureInfo.InvariantCulture, out float result))
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