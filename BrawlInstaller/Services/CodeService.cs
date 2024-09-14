using BrawlInstaller.Classes;
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

        /// <inheritdoc cref="CodeService.ReplaceTable(string, string, List{AsmTableEntry}, DataSize, int)"/>
        string ReplaceTable(string fileText, string label, List<AsmTableEntry> table, DataSize dataSize, int width = 1);
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
        /// <param name="table">ASM table to write</param>
        /// <param name="dataSize">Size of data for table entries</param>
        /// <param name="width">Columns of table</param>
        /// <returns>Text with table replaced</returns>
        public string ReplaceTable(string fileText, string label, List<AsmTableEntry> table, DataSize dataSize, int width=1)
        {
            // Get position of our label
            var labelPosition = fileText.IndexOf(label);
            if (labelPosition > -1)
            {
                fileText = RemoveTable(fileText, label);
                fileText = WriteTable(fileText, label, table, labelPosition, dataSize, width);
            }
            return fileText;
        }

        /// <summary>
        /// Write an ASM table to text
        /// </summary>
        /// <param name="fileText">Text to write table to</param>
        /// <param name="label">Label to use for table</param>
        /// <param name="table">ASM table to write</param>
        /// <param name="index">Position to write table to</param>
        /// <param name="dataSize">Size of data for table entries</param>
        /// <param name="width">Columns of table</param>
        /// <returns>Text with table added</returns>
        private string WriteTable(string fileText, string label, List<AsmTableEntry> table, int index, DataSize dataSize, int width=1)
        {
            // Write label
            fileText = fileText.Insert(index, $"{label}\n");
            index += label.Length + 1;
            if (table.Count > 0)
            {
                // Write count
                var countLine = $"\t{dataSize}[{table.Count}] |\n";
                fileText = fileText.Insert(index, countLine);
                index += countLine.Length;
                // Write table
                var formattedTable = string.Empty;
                var lineComment = string.Empty;
                foreach (var item in table)
                {
                    formattedTable += item.Item;
                    if (table.LastOrDefault() != item)
                    {
                        formattedTable += ", ";
                    }
                    // Generate comments
                    if (item.Comment != string.Empty)
                    {
                        // If start of comment, put comment symbols
                        if (lineComment == string.Empty)
                        {
                            lineComment += " | # ";
                        }
                        // Add comment
                        lineComment += item.Comment;
                        // If not end of line, add a comma 
                        if (!((table.IndexOf(item) + 1) % width == 0) && item != table.LastOrDefault())
                        {
                            lineComment += ", ";
                        }
                    }
                    // If end of line, add comment and return
                    if ((table.IndexOf(item) + 1) % width == 0 || item == table.LastOrDefault())
                    {
                        formattedTable += $"{lineComment}\n";
                        lineComment = string.Empty;
                    }
                }
                fileText = fileText.Insert(index, formattedTable);
                index += formattedTable.Length;
            }
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
            var newText = fileText;
            // Get position of our label
            var labelPosition = fileText.IndexOf(label);
            if (labelPosition > -1)
            {
                // Find the end of the next label
                var nextLabelPosition = fileText.IndexOf(':', labelPosition + label.Length);
                // Find the start of the next label
                var nextLabelStart = fileText.LastIndexOf('\n', nextLabelPosition);
                // Replace all table text with whitespace
                var tableText = fileText.Substring(labelPosition, nextLabelStart - labelPosition);
                newText = fileText.Replace(tableText, string.Empty);
            }
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
            var cleanText = Regex.Replace(fileText, "([|]|[#]).*(\n|\r)", "");
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
