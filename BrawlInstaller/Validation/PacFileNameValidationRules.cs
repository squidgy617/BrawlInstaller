using BrawlInstaller.StaticClasses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BrawlInstaller.Validation
{
    public class PacFileNameWrapper : DependencyObject
    {
        public static readonly DependencyProperty SuffixProperty
            = DependencyProperty.Register(nameof(Suffix), typeof(string), typeof(PacFileNameWrapper), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty AllowCostumeIdsProperty
            = DependencyProperty.Register(nameof(AllowCostumeIds), typeof(bool), typeof(PacFileNameWrapper), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty ExtraSuffixesProperty
            = DependencyProperty.Register(nameof(ExtraSuffixes), typeof(List<string>), typeof(PacFileNameWrapper), new FrameworkPropertyMetadata(new List<string>()));

        public string Suffix
        {
            get { return (string)GetValue(SuffixProperty);}
            set { SetValue(SuffixProperty, value); }
        }

        public bool AllowCostumeIds
        {
            get { return (bool)GetValue(AllowCostumeIdsProperty); }
            set { SetValue(AllowCostumeIdsProperty, value); }
        }

        public List<string> ExtraSuffixes
        {
            get { return (List<string>)GetValue(ExtraSuffixesProperty); }
            set { SetValue(ExtraSuffixesProperty, value); }
        }
    }
    public class PacFileNameValidationRule : ValidationRule
    {
        public PacFileNameWrapper Wrapper { get; set; }

        public PacFileNameValidationRule()
        {

        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var suffix = (string)value;
            var costumeSuffix = string.Empty;

            var suffixString = "^(";
            suffixString += string.Join("|", PacFiles.PacFileSuffixes.Select(x => $"({x.Replace("#", "\\d")})"));
            suffixString += string.Join("|", Wrapper.ExtraSuffixes);
            suffixString += ")+";
            if (Wrapper.AllowCostumeIds)
            {
                costumeSuffix += "(\\d\\d)?";
                suffixString += costumeSuffix;
            }
            suffixString += "$";

            if (!string.IsNullOrEmpty(suffix) && !(suffix.StartsWith("$") && suffix.Length > 1) && !Regex.IsMatch(suffix, suffixString, RegexOptions.IgnoreCase) && !(Wrapper.AllowCostumeIds && Regex.IsMatch(suffix, $"^{costumeSuffix}$", RegexOptions.IgnoreCase)))
            {
                return new ValidationResult(false, "File suffix is not valid.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
