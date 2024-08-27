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
    internal class PartLibrary
    {
        private static readonly string[] headers = new string[]
        {
            "Studio ItemNo", "BaseStudioItemNo", "BL ItemNo", "BL ItemKey", "LDraw ItemNo", "LDD ItemNo",
            "Description", "isPerfectForCulling", "BLCatalogIndex", "BLCatalogSubIndex", "EasyModeIndex",
            "IsAssembly?", "flexible type", "IsDecorated", "XPCatalogIndex", "XPCatalogSubIndex"
        };


        private Dictionary<string, Part> parts = new Dictionary<string, Part>();
        private Dictionary<string, Part> blItemNoIndex = new Dictionary<string, Part>();
        private Dictionary<string, Part> lDrawItemNoIndex = new Dictionary<string, Part>();

        private readonly StudioOptions _options;

        public PartLibrary(StudioOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));

            var file = Path.Combine(_options.StudioFolder, "data", "StudioPartDefinition2.txt");
            LoadParts(file);
        }

        private void LoadParts(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} does not exist.");
            }

            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine("Loading parts started.");

            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var streamReader = new StreamReader(fileStream))
                {
                    // Read and validate the header line
                    string headerLine = streamReader.ReadLine();
                    string errorMessage = null;
                    if (headerLine == null || !IsValidHeader(headerLine, out errorMessage))
                    {
                        throw new InvalidDataException($"The Studio Part Definition file does not contain a valid header. {errorMessage}");
                    }

                    // Read the rest of the file
                    string line;
                    int lineNumber = 1; // Start counting lines from 1
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        lineNumber++;
                        try
                        {
                            var part = FromTsv(line);
                            parts[part.BLItemNo] = part;
                            blItemNoIndex[part.BLItemNo] = part;
                            lDrawItemNoIndex[part.LDrawItemNo] = part;
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
                Console.WriteLine($"An error occurred while loading parts: {ex.Message}");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine($"Loading parts completed. Number of parts: {parts.Count}. Time taken: {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        public static Part FromTsv(string tsvLine)
        {
            var values = tsvLine.Split('\t');
            try
            {
                int studioItemNo = Parsers.IntParser(values, headers, 0);
                int baseStudioItemNo = Parsers.IntParser(values, headers, 1);
                string blItemNo = values[2];
                string blItemKey = values[3];
                string lDrawItemNo = values[4];
                string lddItemNo = values[5];
                string description = values[6];
                string options = values[7];
                int blCatalogIndex = Parsers.IntParser(values, headers, 8);
                int blCatalogSubIndex = Parsers.IntParser(values, headers, 9);
                int easyModeIndex = Parsers.IntParser(values, headers, 10);
                bool isAssembly = Parsers.BoolParser(values, headers, 11);
                string flexibleType = values[12];
                bool isDecorated = Parsers.BoolParser(values, headers, 13);
                int? xpCatalogIndex = Parsers.NullableIntParser(values, headers, 14);
                int? xpCatalogSubIndex = Parsers.NullableIntParser(values, headers, 15);

                return new Part(studioItemNo, baseStudioItemNo, blItemNo, blItemKey, lDrawItemNo, lddItemNo, description, options, blCatalogIndex, blCatalogSubIndex, easyModeIndex, isAssembly, flexibleType, isDecorated, xpCatalogIndex, xpCatalogSubIndex);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error parsing TSV line: {tsvLine}. Exception: {ex.Message}");
                throw;
            }
        }

        private bool IsValidHeader(string headerLine, out string errorMessage)
        {
            // Define the expected header columns
           var headerColumns = headerLine.Split('\t');
            var missingColumns = headers.Except(headerColumns).ToList();
            var extraColumns = headerColumns.Except(headers).ToList();

            if (missingColumns.Any() || extraColumns.Any())
            {
                errorMessage = $"Missing columns: {string.Join(", ", missingColumns)}. Extra columns: {string.Join(", ", extraColumns)}.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public Part GetPartByBLItemNo(string blItemNo)
        {
            return blItemNoIndex.TryGetValue(blItemNo, out var part) ? part : null;
        }

        public Part GetPartByLDrawItemNo(string lDrawItemNo)
        {
            return lDrawItemNoIndex.TryGetValue(lDrawItemNo, out var part) ? part : null;
        }
    }
}
