﻿using BrawlInstaller.Classes;
using BrawlInstaller.Exceptions;
using BrawlInstaller.StaticClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        string ReplaceTable(string fileText, string label, List<AsmTableEntry> table, DataSize dataSize, int width = 1, int padding = 2);

        /// <inheritdoc cref="CodeService.ReplaceHook(AsmHook, string)"/>
        string ReplaceHook(AsmHook hook, string fileText);

        /// <inheritdoc cref="CodeService.ReplaceHooks(List{AsmHook}, string)"/>
        string ReplaceHooks(List<AsmHook> hooks, string fileText);

        /// <inheritdoc cref="CodeService.CompileCodes()"/>
        void CompileCodes();

        /// <inheritdoc cref="CodeService.GetMacro(string, string, string, int, string)"/>
        AsmMacro GetMacro(string fileText, string address, string paramValue, int paramIndex, string macroName);

        /// <inheritdoc cref="CodeService.InsertUpdateMacro(string, string, AsmMacro, int, int)"/>
        string InsertUpdateMacro(string fileText, string address, AsmMacro asmMacro, int paramIndex = 0, int index = 0);

        /// <inheritdoc cref="CodeService.RemoveMacro(string, string, string, string, int)"/>
        string RemoveMacro(string fileText, string address, string paramValue, string macroName, int paramIndex = 0);
    }

    [Export(typeof(ICodeService))]
    internal class CodeService : ICodeService
    {
        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }

        [ImportingConstructor]
        public CodeService(ISettingsService settingsService, IFileService fileService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
        }

        // Methods

        /// <summary>
        /// Read code from an ASM file
        /// </summary>
        /// <param name="filePath">File to read code from</param>
        /// <returns>String containing code</returns>
        public string ReadCode(string filePath)
        {
            var lines = _fileService.ReadTextLines(filePath);
            // Do this to maintain consistent line endings of \n
            var text = string.Join("\r\n", lines);
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
            var newLine = "\r\n";
            var hookString = string.Empty;
            // Single line code
            if (hook.Instructions.Count == 1 && !hook.IsHook)
            {
                hookString = $"{newLine}{hook.Instructions.FirstOrDefault()?.Text.Trim()} @ ${hook.Address}";
                // Add comment if there is one
                if (!string.IsNullOrEmpty(hook.Comment))
                {
                    hookString += $" # {hook.Comment}";
                }
                if (!string.IsNullOrEmpty(hook.Instructions.FirstOrDefault()?.Comment))
                {
                    hookString += $" # {hook.Instructions.FirstOrDefault()?.Comment}";
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
                hookString += string.Join(newLine, hook.Instructions.Select(s => $"\t{s.Text.Trim()}" + (!string.IsNullOrEmpty(s.Comment) ? $" # {s.Comment}" : string.Empty)));
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
            var newLine = "\r\n";
            var index = fileText.IndexOf($"${address}");
            if (index > -1)
            {
                // Find the start of the line
                hookStart = fileText.LastIndexOf(newLine, index);
                var header = fileText.Substring(hookStart, index - hookStart);
                var atIndex = header.IndexOf('@');
                var instruction = header.Substring(0, atIndex).Trim();
                var hookEnd = fileText.IndexOf(newLine, index);
                hookEnd = fileText.Length <= hookEnd + newLine.Length || hookEnd == -1 ? fileText.Length : hookEnd + newLine.Length;
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
                            fileText = fileText.Remove(startingBrace, endingBrace + newLine.Length - startingBrace);
                        }
                    }
                }
            }
            return (fileText, hookStart);
        }

        /// <summary>
        /// Get an instruction object from a string
        /// </summary>
        /// <param name="instructionText">Raw text of instruction</param>
        /// <returns>Instruction object</returns>
        private Instruction GetInstruction(string instructionText)
        {
            var instruction = new Instruction();
            var text = Regex.Replace(instructionText, "([|]|[#]|[//]).*", "");
            instruction.Text = text;
            if (instructionText.Contains("#"))
            {
                var commentStart = instructionText.IndexOf("#");
                var comment = instructionText.Substring(commentStart + 1).Trim();
                instruction.Comment = comment;
            }
            return instruction;
        }

        /// <summary>
        /// Read a piece of ASM code
        /// </summary>
        /// <param name="fileText">Text to read from</param>
        /// <param name="hookLocation">Address hook is bound to</param>
        /// <returns>ASM hook with instructions</returns>
        private AsmHook ReadHook(string fileText, string address)
        {
            var newLine = "\r\n";
            AsmHook asmHook = null;
            // Find hook position
            var index = fileText.IndexOf($"${address}");
            if (index > -1)
            {
                asmHook = new AsmHook();
                asmHook.Address = address;
                // Find the start of the line
                var hookStart = fileText.LastIndexOf(newLine, index);
                var header = fileText.Substring(hookStart, index - hookStart);
                var atIndex = header.IndexOf('@');
                var instructionText = header.Substring(0, atIndex).Trim();
                var lineEnd = fileText.IndexOf(newLine, hookStart + 1);
                var fullLine = fileText.Substring(hookStart, lineEnd - hookStart);
                // Single instruction hook
                if (instructionText != "CODE" && instructionText != "HOOK")
                {
                    var instruction = GetInstruction(instructionText);
                    if (fullLine.Contains("#"))
                    {
                        var commentStart = fullLine.IndexOf("#");
                        instruction.Comment = fullLine.Substring(commentStart + 1);
                    }
                    asmHook.IsHook = false;
                    asmHook.Instructions.Add(instruction);
                }
                // Multi instruction hook
                else
                {
                    // Find braces
                    var startingBrace = fileText.IndexOf('{', index);
                    if (startingBrace > -1)
                    {
                        var endingBrace = fileText.IndexOf('}', startingBrace);
                        // Get instructions between braces
                        if (endingBrace > -1)
                        {
                            var instructionLines = fileText.Substring(startingBrace + 1, endingBrace - startingBrace - 1).Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            foreach (var line in instructionLines)
                            {
                                var instruction = GetInstruction(line);
                                asmHook.Instructions.Add(instruction);
                            }
                        }
                    }
                }
                // Indicates if it is a true hook or just code
                if (instructionText == "HOOK")
                {
                    asmHook.IsHook = true;
                }
            }
            return asmHook;
        }

        /// <summary>
        /// Get macro in hook by address
        /// </summary>
        /// <param name="fileText">Text containing code</param>
        /// <param name="address">Address of hook to search for</param>
        /// <param name="paramValue">Value to compare to</param>
        /// <param name="paramIndex">Index of parameter to compare to</param>
        /// <param name="macroName">Name of macro</param>
        /// <returns>Found ASM macro</returns>
        public AsmMacro GetMacro(string fileText, string address, string paramValue, int paramIndex, string macroName)
        {
            var asmHook = ReadHook(fileText, address);
            if (asmHook != null)
            {
                var macroMatch = FindMacroMatch(asmHook, paramValue, paramIndex, macroName);
                return macroMatch.Macro;
            }
            return null;
        }

        /// <summary>
        /// Find matching macro in ASM hook
        /// </summary>
        /// <param name="asmHook">ASM hook to search for match</param>
        /// <param name="asmMacro">Macro to use for comparison</param>
        /// <param name="paramIndex">Which parameter to compare</param>
        /// <returns>Index of matching macro</returns>
        private (int Index, AsmMacro Macro) FindMacroMatch(AsmHook asmHook, AsmMacro asmMacro, int paramIndex)
        {
            var index = -1;
            AsmMacro newMacro = null;
            if (asmMacro.Parameters.Count > paramIndex)
            {
                return FindMacroMatch(asmHook, asmMacro.Parameters[paramIndex], paramIndex, asmMacro.MacroName);
            }
            return (index, newMacro);
        }

        /// <summary>
        /// Find matching macro in ASM hook
        /// </summary>
        /// <param name="asmHook">ASM hook to search for match</param>
        /// <param name="paramValue">Value to compare parameter to</param>
        /// <param name="paramIndex">Which parameter to compare</param>
        /// <returns>Index of matching macro</returns>
        private (int Index, AsmMacro Macro) FindMacroMatch(AsmHook asmHook, string paramValue, int paramIndex, string macroName)
        {
            var index = -1;
            AsmMacro newMacro = null;
            foreach(var instruction in asmHook.Instructions)
            {
                var formattedInstruction = instruction.Text.Trim();
                // Check if macro starting character (%) is present
                if (formattedInstruction.StartsWith("%"))
                {
                    // Get macro name and parameters
                    var opening = formattedInstruction.IndexOf('(');
                    var closing = formattedInstruction.IndexOf(')');
                    var label = formattedInstruction.Substring(1, opening - 1).Trim();
                    var parameterString = formattedInstruction.Substring(opening + 1, closing - opening - 1);
                    var comment = string.Empty;
                    // Get comments
                    if (formattedInstruction.Contains("#"))
                    {
                        var commentStart = formattedInstruction.IndexOf("#");
                        comment = formattedInstruction.Substring(commentStart);
                    }
                    parameterString = Regex.Replace(parameterString, @"\s+", "");
                    var parameters = parameterString.Split(',').ToList();
                    // Generate the new macro
                    newMacro = new AsmMacro
                    {
                        MacroName = label,
                        Parameters = parameters,
                        Comment = comment
                    };
                    // Check parameters at specified index to determine if we found a match
                    if (newMacro.Parameters.Count > paramIndex && newMacro.MacroName == macroName && newMacro.Parameters[paramIndex] == paramValue)
                    {
                        // If so, return the index of the match
                        index = asmHook.Instructions.IndexOf(instruction);
                        return (index, newMacro);
                    }
                }
            }
            return (index, null);
        }

        /// <summary>
        /// Insert or update a macro in a hook specified by address
        /// </summary>
        /// <param name="fileText">Text of code</param>
        /// <param name="address">Address of hook to insert to</param>
        /// <param name="asmMacro">Macro to insert</param>
        /// <param name="paramIndex">Index of parameter for comparison</param>
        /// <param name="index">Index to insert to</param>
        /// <returns>Updated code</returns>
        public string InsertUpdateMacro(string fileText, string address, AsmMacro asmMacro, int paramIndex = 0, int index = 0)
        {
            var asmHook = ReadHook(fileText, address);
            if (asmHook != null)
            {
                asmHook = InsertUpdateMacro(asmHook, asmMacro, paramIndex, index);
                fileText = ReplaceHook(asmHook, fileText);
            }
            return fileText;
        }

        /// <summary>
        /// Insert or update macro in ASM hook
        /// </summary>
        /// <param name="asmHook">ASM hook to insert macro to</param>
        /// <param name="asmMacro">ASM macro to insert</param>
        /// <param name="paramIndex">Parameter index to check for matching macro</param>
        /// <param name="index">Index to insert macro if no match is found</param>
        /// <returns>ASM hook with inserted macro</returns>
        private AsmHook InsertUpdateMacro(AsmHook asmHook, AsmMacro asmMacro, int paramIndex = 0, int index = 0)
        {
            var foundMacro = FindMacroMatch(asmHook, asmMacro, paramIndex);
            if (foundMacro.Index > -1)
            {
                index = foundMacro.Index;
                asmHook = RemoveInstruction(asmHook, index);
            }
            if (asmMacro != null)
            {
                return InsertMacro(asmHook, asmMacro, index);
            }
            return null;
        }

        /// <summary>
        /// Insert a macro into an ASM hook
        /// </summary>
        /// <param name="asmHook">ASM hook to insert macro into</param>
        /// <param name="asmMacro">ASM macro to insert</param>
        /// <param name="index">Line index to insert to</param>
        /// <returns>ASM hook with added macro</returns>
        private AsmHook InsertMacro(AsmHook asmHook, AsmMacro asmMacro, int index = 0)
        {
            var macroString = $"%{asmMacro.MacroName}({string.Join(",", asmMacro.Parameters)})" + (!string.IsNullOrEmpty(asmMacro.Comment) ? $"\t#{asmMacro.Comment}" : string.Empty);
            var instruction = GetInstruction(macroString);
            asmHook.Instructions.Insert(index, instruction);
            return asmHook;
        }

        /// <summary>
        /// Remove macro in hook specified by address
        /// </summary>
        /// <param name="fileText">Code to remove macro from</param>
        /// <param name="address">Address of hook to remove macro from</param>
        /// <param name="paramValue">Value to use for comparison</param>
        /// <param name="macroName">Name of macro</param>
        /// <param name="paramIndex">Index of parameter to compare</param>
        /// <returns>Code with macro removed</returns>
        public string RemoveMacro(string fileText, string address, string paramValue, string macroName, int paramIndex = 0)
        {
            var asmHook = ReadHook(fileText, address);
            if (asmHook != null)
            {
                asmHook = RemoveMacro(asmHook, paramValue, macroName, paramIndex);
                fileText = ReplaceHook(asmHook, fileText);
            }
            return fileText;
        }

        /// <summary>
        /// Remove macro in ASM hook
        /// </summary>
        /// <param name="asmHook">ASM hook to remove macro from</param>
        /// <param name="paramValue">Parameter value to use for comparison</param>
        /// <param name="macroName">Name of macro</param>
        /// <param name="paramIndex">Index of parameter to compare</param>
        /// <returns>ASM hook with macro removed</returns>
        private AsmHook RemoveMacro(AsmHook asmHook, string paramValue, string macroName, int paramIndex = 0)
        {
            var macro = FindMacroMatch(asmHook, paramValue, paramIndex, macroName);
            if (macro.Index > -1)
            {
                asmHook = RemoveInstruction(asmHook, macro.Index);
            }
            return asmHook;
        }

        /// <summary>
        /// Remove ASM hook instruction at specified index
        /// </summary>
        /// <param name="asmHook">ASM hook to remove instructionf rom</param>
        /// <param name="index">Index of instruction to remove</param>
        /// <returns>ASM hook with instruction removed</returns>
        private AsmHook RemoveInstruction(AsmHook asmHook, int index)
        {
            asmHook.Instructions.RemoveAt(index);
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
        /// <param name="padding">Minimum width of each column</param>
        /// <returns>Text with table replaced</returns>
        public string ReplaceTable(string fileText, string label, List<AsmTableEntry> table, DataSize dataSize, int width=1, int padding=2)
        {
            // Get position of our label
            var labelPosition = fileText.IndexOf(label);
            if (labelPosition > -1)
            {
                fileText = RemoveTable(fileText, label);
                fileText = WriteTable(fileText, label, table, labelPosition, dataSize, width, padding);
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
        /// <param name="padding">Minimum width of each column</param>
        /// <returns>Text with table added</returns>
        private string WriteTable(string fileText, string label, List<AsmTableEntry> table, int index, DataSize dataSize, int width=1, int padding=2)
        {
            var newLine = "\r\n";
            // Write label
            fileText = fileText.Insert(index, $"{label}{newLine}");
            index += label.Length + newLine.Length;
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
                    formattedTable += item.Item.PadLeft(padding);
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
                        if (!((table.IndexOf(item) + 1) % width == 0) && item != table.LastOrDefault() && !string.IsNullOrEmpty(table[table.IndexOf(item) + 1].Comment))
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
            var newLine = "\r\n";
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
                newText = fileText.Remove(labelPosition, nextLabelStart - labelPosition);
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
            var cleanText = Regex.Replace(fileText, "([|]|[#]|[//]).*(\n|\r)", "");
            // Find table start
            var labelPosition = cleanText.IndexOf(label);
            var workingText = cleanText.Substring(labelPosition, cleanText.Length - labelPosition);
            // Get table length
            var result = Regex.Match(workingText, "(half|word|byte|float)[[]\\d+[]]");
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

        /// <summary>
        /// Compile all codes
        /// </summary>
        // TODO: Create backups of code files when we do this
        public void CompileCodes()
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            if (!string.IsNullOrEmpty(_settingsService.BuildSettings.ToolPathSettings.GctRealMateExe))
            {
                var args = "-g -l -q";
                var argList = _settingsService.BuildSettings.FilePathSettings.CodeFilePaths.Select(s => $" \"{Path.Combine(buildPath, s)}\"");
                foreach(var arg in argList)
                {
                    args += arg;
                }
                Process gctRm = Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine(buildPath, _settingsService.BuildSettings.ToolPathSettings.GctRealMateExe),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = args
                });
                gctRm?.WaitForExit(5000);
                if (gctRm?.HasExited == false)
                {
                    var error = new CompilerTimeoutException($"{_settingsService.BuildSettings.ToolPathSettings.GctRealMateExe} encountered an error. Verify your code files are valid and don't contain errors.");
                    gctRm?.Kill();
                    throw error;
                }
            }
        }
    }
}
