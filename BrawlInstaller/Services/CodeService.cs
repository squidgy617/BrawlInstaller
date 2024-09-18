using BrawlInstaller.Classes;
using BrawlInstaller.StaticClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface ICodeService
    {
        /// <inheritdoc cref="CodeService.ReadCode(string)"/>
        string ReadCode(string filePath);

        /// <inheritdoc cref="CodeService.ReadTable(string, string)"/>
        List<string> ReadTable(string fileText, string label);

        /// <inheritdoc cref="CodeService.ReplaceTable(string, string, List{AsmTableEntry}, DataSize, int)"/>
        string ReplaceTable(string fileText, string label, List<AsmTableEntry> table, DataSize dataSize, int width = 1);

        /// <inheritdoc cref="CodeService.ReplaceHook(AsmHook, string)"/>
        string ReplaceHook(AsmHook hook, string fileText);

        /// <inheritdoc cref="CodeService.ReplaceHooks(List{AsmHook}, string)"/>
        string ReplaceHooks(List<AsmHook> hooks, string fileText);
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
        /// Read code from an ASM file
        /// </summary>
        /// <param name="filePath">File to read code from</param>
        /// <returns>String containing code</returns>
        public string ReadCode(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            var text = string.Join(Environment.NewLine, lines);
            return text;
        }

        /// <summary>
        /// Replace multiple pieces of ASM code
        /// </summary>
        /// <param name="hooks">List of hooks to replace</param>
        /// <param name="fileText">Text to replace hooks in</param>
        /// <returns>Text with hooks replaced</returns>
        public string ReplaceHooks(List<AsmHook> hooks, string fileText)
        {
            foreach(var hook in hooks) 
            { 
                fileText = ReplaceHook(hook, fileText);
            }
            return fileText;
        }

        /// <summary>
        /// Replace a piece of ASM code
        /// </summary>
        /// <param name="hook">Hook to write</param>
        /// <param name="fileText">Text to replace hook in</param>
        /// <returns>Text with hook replaced</returns>
        public string ReplaceHook(AsmHook hook, string fileText)
        {
            var result = RemoveHook(fileText, hook.Address);
            if (result.Index > -1)
            {
                fileText = WriteHook(hook, result.FileText, result.Index);
            }
            return fileText;
        }

        /// <summary>
        /// Write a piece of ASM code
        /// </summary>
        /// <param name="hook">ASM hook to write</param>
        /// <param name="fileText">Text to write hook to</param>
        /// <param name="index">Location to write hook to</param>
        /// <returns>Text with hook added</returns>
        private string WriteHook(AsmHook hook, string fileText, int index)
        {
            var newLine = Environment.NewLine;
            var hookString = string.Empty;
            // Single line code
            if (hook.Instructions.Count == 1)
            {
                hookString = $"{newLine}{hook.Instructions.FirstOrDefault().Trim()} @ ${hook.Address}";
                // Add comment if there is one
                if (!string.IsNullOrEmpty(hook.Comment))
                {
                    hookString += $" # {hook.Comment}";
                }
                hookString += newLine;
            }
            // Multi line
            else
            {
                hookString = newLine + (hook.IsHook ? "HOOK" : "CODE") + " @ $" + hook.Address;
                // Add comment if there is one
                if (!string.IsNullOrEmpty(hook.Comment))
                {
                    hookString += $" # {hook.Comment}";
                }
                // Add instructions
                hookString += newLine + "{" + newLine;
                hookString += string.Join(newLine, hook.Instructions.Select(s => $"\t{s}"));
                hookString += newLine + "}" + newLine;
            }
            fileText = fileText.Insert(index, hookString);
            return fileText;
        }

        /// <summary>
        /// Remove a piece of ASM code
        /// </summary>
        /// <param name="fileText">Text to remove from</param>
        /// <param name="address">Address to remove code from</param>
        /// <returns>Text with hook removed, index where hook was removed</returns>
        private (string FileText, int Index) RemoveHook(string fileText, string address)
        {
            var hookStart = -1;
            var newLine = Environment.NewLine;
            var index = fileText.IndexOf($"${address}");
            if (index > -1)
            {
                // Find the start of the line
                hookStart = fileText.LastIndexOf(newLine, index);
                var header = fileText.Substring(hookStart, index - hookStart);
                var atIndex = header.IndexOf('@');
                var instruction = header.Substring(0, atIndex).Trim();
                var hookEnd = fileText.IndexOf(newLine, index);
                hookEnd = fileText.Length <= hookEnd + 2 || hookEnd == -1 ? fileText.Length : hookEnd + 2;
                // Single instruction hook
                fileText = fileText.Remove(hookStart, hookEnd - hookStart);
                // Multi instruction hook
                if (instruction == "CODE" || instruction == "HOOK")
                {
                    // Find braces
                    var startingBrace = fileText.IndexOf('{', hookStart);
                    if (startingBrace > -1)
                    {
                        var endingBrace = fileText.IndexOf('}', startingBrace);
                        endingBrace = fileText.IndexOf(newLine, endingBrace);
                        // Remove instructions between braces
                        if (endingBrace > -1)
                        {
                            fileText = fileText.Remove(startingBrace, endingBrace + 2 - startingBrace);
                        }
                    }
                }
            }
            return (fileText, hookStart);
        }

        /// <summary>
        /// Read a piece of ASM code
        /// </summary>
        /// <param name="fileText">Text to read from</param>
        /// <param name="hookLocation">Address hook is bound to</param>
        /// <returns>ASM hook with instructions</returns>
        private AsmHook ReadHook(string fileText, string address)
        {
            var newLine = Environment.NewLine;
            var asmHook = new AsmHook();
            // Remove comments
            var cleanText = Regex.Replace(fileText, "([|]|[#]).*(\n|\r)", "");
            // Find hook position
            var index = cleanText.IndexOf($"${address}");
            if (index > -1)
            {
                asmHook.Address = address;
                // Find the start of the line
                var hookStart = cleanText.LastIndexOf(newLine, index);
                var header = cleanText.Substring(hookStart, index - hookStart);
                var atIndex = header.IndexOf('@');
                var instruction = header.Substring(0, atIndex).Trim();
                // Single instruction hook
                if (instruction != "CODE" && instruction != "HOOK")
                {
                    asmHook.IsHook = false;
                    asmHook.Instructions.Add(instruction);
                }
                // Multi instruction hook
                else
                {
                    // Find braces
                    var startingBrace = cleanText.IndexOf('{', index);
                    if (startingBrace > -1)
                    {
                        var endingBrace = cleanText.IndexOf('}', startingBrace);
                        // Get instructions between braces
                        if (endingBrace > -1)
                        {
                            asmHook.Instructions = cleanText.Substring(startingBrace + 1, endingBrace - startingBrace - 1).Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        }
                    }
                }
                // Indicates if it is a true hook or just code
                if (instruction == "HOOK")
                {
                    asmHook.IsHook = true;
                }
            }
            return asmHook;
        }

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
            var newLine = Environment.NewLine;
            // Write label
            fileText = fileText.Insert(index, $"{label}{newLine}");
            index += label.Length + 1;
            if (table.Count > 0)
            {
                // Write count
                var countLine = $"\t{dataSize}[{table.Count}] |{newLine}";
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
                        formattedTable += $"{lineComment}{newLine}";
                        lineComment = string.Empty;
                    }
                }
                fileText = fileText.Insert(index, formattedTable);
                index += formattedTable.Length;
            }
            // Add return
            fileText = fileText.Insert(index, newLine);
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
            var newLine = Environment.NewLine;
            var newText = fileText;
            // Get position of our label
            var labelPosition = fileText.IndexOf(label);
            if (labelPosition > -1)
            {
                // Find the end of the next label
                var nextLabelPosition = fileText.IndexOf(':', labelPosition + label.Length);
                // Find the start of the next label
                var nextLabelStart = fileText.LastIndexOf(newLine, nextLabelPosition);
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
