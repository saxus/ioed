using IoEditor.Models.Configuration;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoEditor.Models.Studio
{
    internal class ColorLibrary
    {
        private static readonly string[] headers = new string[]
        {
            "Studio Color Code", "BL Color Code", "LDraw Color Code", "LDD color code", "Studio Color Name",
            "BL Color Name", "LDraw Color Name", "LDD Color Name", "RGB value", "Alpha", "CategoryName",
            "Color Group Index", "note", "Ins_RGB", "Ins_CMYK"
        };

        private Dictionary<int, Color> colors = new Dictionary<int, Color>();
        private Dictionary<int, Color> blColorCodeIndex = new Dictionary<int, Color>();
        private Dictionary<int, Color> lDrawColorCodeIndex = new Dictionary<int, Color>();

        private readonly StudioOptions _options;

        public ColorLibrary(StudioOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            var file = Path.Combine(_options.StudioFolder, "data", "StudioColorDefinition.txt");
            LoadColors(file);
        }

        private void LoadColors(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} does not exist.");
            }

            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Loading colors started.");

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var streamReader = new StreamReader(fileStream))
                {
                    // Read and validate the header line
                    string headerLine = streamReader.ReadLine();
                    if (headerLine == null || !IsValidHeader(headerLine))
                    {
                        throw new InvalidDataException("The Studio Color Definition file does not contain a valid header.");
                    }

                    // Read the rest of the file
                    string line;
                    int lineNumber = 1; // Start counting lines from 1
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        lineNumber++;
                        try
                        {
                            var color = FromTsv(line);

                            if (!color.BLColorCode.HasValue)
                            {
                                Console.WriteLine($"Skipped color: {color.StudioColorName} because of missing BlColorCode. ");
                                continue;
                            }

                            colors[color.StudioColorCode] = color;
                            blColorCodeIndex[color.BLColorCode.Value] = color;
                            lDrawColorCodeIndex[color.LDrawColorCode] = color;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error parsing line {lineNumber}: {line}. Exception: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading colors: {ex.Message}");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"Loading colors completed in {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        public static Color FromTsv(string tsvLine)
        {
            var values = tsvLine.Split('\t');
            try
            {
                int studioColorCode = Parsers.IntParser(values, headers, 0);
                int? blColorCode = Parsers.NullableIntParser(values, headers, 1);
                int lDrawColorCode = Parsers.IntParser(values, headers, 2);
                int LDDColorCode = Parsers.IntParser(values, headers, 3);
                string studioColorName = values[4];
                string blColorName = values[5];
                string lDrawColorName = values[6];
                string LDDColorName = values[7];
                string rgbValue = values[8];
                float alpha = Parsers.FloatParser(values, headers, 9);
                string categoryName = values[10];
                int colorGroupIndex = Parsers.IntParser(values, headers, 11);
                string note = values[12];
                string insRgb = values[13];
                string insCmyk = values[14];

                return new Color(
                    studioColorCode, 
                    blColorCode, 
                    lDrawColorCode, 
                    LDDColorCode,
                    studioColorName, 
                    blColorName, 
                    lDrawColorName, 
                    LDDColorName,
                    rgbValue, 
                    alpha, 
                    categoryName, 
                    colorGroupIndex, 
                    note, 
                    insRgb, 
                    insCmyk);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error parsing TSV line: {tsvLine}. Exception: {ex.Message}");
                throw;
            }
        }

        private bool IsValidHeader(string headerLine)
        {
            var headersInFile = headerLine.Split('\t');
            if (headersInFile.Length != headers.Length)
            {
                return false;
            }

            for (int i = 0; i < headers.Length; i++)
            {
                if (headersInFile[i] != headers[i])
                {
                    return false;
                }
            }

            return true;
        }

        public Color GetColorByStudioColorCode(int studioColorCode)
        {
            return colors.TryGetValue(studioColorCode, out var color) ? color : null;
        }

        public Color GetColorByBLColorCode(int blColorCode)
        {
            return blColorCodeIndex.TryGetValue(blColorCode, out var color) ? color : null;
        }

        public Color GetColorByLDrawColorCode(int lDrawColorCode)
        {
            return lDrawColorCodeIndex.TryGetValue(lDrawColorCode, out var color) ? color : null;
        }
    }
}
