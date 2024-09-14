﻿using BrawlInstaller.Classes;
using BrawlInstaller.StaticClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface ICodeService
    {
        /// <inheritdoc cref="CodeService.ReadTable(string, string)"/>
        List<string> ReadTable(string fileText, string label);

        /// <inheritdoc cref="CodeService.ReplaceTable(string, string, List{string}, DataSize)"/>
        string ReplaceTable(string fileText, string label, List<string> table, DataSize dataSize);
    }

    [Export(typeof(ICodeService))]
    internal class CodeService : ICodeService
    {
        // Services
        ISettingsService _settingsService { get; }

        [ImportingConstructor]
        public CodeService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        // Methods

        /// <summary>
        /// Replace an ASM table in text
        /// </summary>
        /// <param name="fileText">Text to replace table in</param>
        /// <param name="label">Label of table to replace</param>
        /// <param name="table">Table to write</param>
        /// <returns>Text with table replaced</returns>
        public string ReplaceTable(string fileText, string label, List<string> table, DataSize dataSize)
        {
            // Get position of our label
            var labelPosition = fileText.IndexOf(label);
            fileText = RemoveTable(fileText, label);
            fileText = WriteTable(fileText, label, table, labelPosition, dataSize);
            return fileText;
        }

        /// <summary>
        /// Write an ASM table to text
        /// </summary>
        /// <param name="fileText">Text to write table to</param>
        /// <param name="label">Label to use for table</param>
        /// <param name="table">Table to write</param>
        /// <param name="index">Position in text to start writing</param>
        /// <returns>Text with table added</returns>
        private string WriteTable(string fileText, string label, List<string> table, int index, DataSize dataSize)
        {
            // Write label
            fileText = fileText.Insert(index, $"{label}\n");
            index += label.Length + 1;
            // Write count
            var countLine = $"\t{dataSize}[{table.Count}] |\n";
            fileText = fileText.Insert(index, countLine);
            index += countLine.Length;
            // Write table
            var tableText = string.Join(",\n", table);
            fileText = fileText.Insert(index, tableText);
            index += tableText.Length;
            // Add return
            fileText = fileText.Insert(index, "\n");
            return fileText;
        }

        /// <summary>
        /// Remove an ASM table from text
        /// </summary>
        /// <param name="fileText">Text to remove table from</param>
        /// <param name="label">Label of table</param>
        /// <returns>Text with table removed</returns>
        private string RemoveTable(string fileText, string label)
        {
            // Get position of our label
            var labelPosition = fileText.IndexOf(label);
            // Find the end of the next label
            var nextLabelPosition = fileText.IndexOf(':', labelPosition + label.Length);
            // Find the start of the next label
            var nextLabelStart = fileText.LastIndexOf('\n', nextLabelPosition);
            // Replace all table text with whitespace
            var tableText = fileText.Substring(labelPosition, nextLabelStart - labelPosition);
            var newText = fileText.Replace(tableText, string.Empty);
            return newText;
        }

        /// <summary>
        /// Read an ASM table from text
        /// </summary>
        /// <param name="fileText">Text to read table from</param>
        /// <param name="label">Label of table</param>
        /// <returns>List of strings found in table</returns>
        public List<string> ReadTable(string fileText, string label)
        {
            var table = new List<string>();
            // Remove comments
            var cleanText = Regex.Replace(fileText, "([|]|[#]).+(\n|\r)", "");
            // Find table start
            var labelPosition = cleanText.IndexOf(label);
            var workingText = cleanText.Substring(labelPosition, cleanText.Length - labelPosition);
            // Get table length
            var result = Regex.Match(workingText, "(half|word|byte)[[]\\d+[]]");
            // If another label appears before the table counter, then it is an empty table
            var nextLabelPosition = workingText.IndexOf(':', label.Length);
            if (result.Success && result.Index < nextLabelPosition)
            {
                var match = result.Value;
                var start = match.IndexOf('[') + 1;
                var end = match.IndexOf(']');
                var countString = match.Substring(start, end - start);
                // Add table values to list
                if (int.TryParse(countString, out int count)) 
                {
                    // Table starts after count
                    var tableStart = workingText.IndexOf(countString) + countString.Length + 1;
                    workingText = workingText.Substring(tableStart, workingText.Length - tableStart);
                    // Values are comma-separated
                    foreach(var item in workingText.Split(','))
                    {
                        // Remove everything after tabs or newlines
                        var value = item.Trim().Split('\t', '\n', '\r')[0];
                        // Only gather up to table size
                        if (table.Count >= count)
                        {
                            break;
                        }
                        table.Add(value);
                    }
                }
            }
            return table;
        }
    }
}
