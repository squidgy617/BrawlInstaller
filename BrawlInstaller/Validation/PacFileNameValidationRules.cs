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

        public string Suffix
        {
            get { return (string)GetValue(SuffixProperty);}
            set { SetValue(SuffixProperty, value); }
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

            var suffixString = string.Join("|", PacFiles.PacFileSuffixes.Select(x => $"({x.Replace("#", "\\d")})"));

            if (!string.IsNullOrEmpty(suffixString) && !Regex.IsMatch(suffix, suffixString))
            {
                return new ValidationResult(false, "File suffix is not valid.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
