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
            var result = Regex.Match(workingText, "(half|word|byte)[[]\\d*[]]");
            if (result.Success)
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
