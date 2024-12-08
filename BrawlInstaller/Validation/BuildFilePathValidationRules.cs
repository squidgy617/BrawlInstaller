﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BrawlInstaller.Validation
{
    public class BuildFilePathWrapper : DependencyObject
    {
        public static readonly DependencyProperty FilePathProperty =
            DependencyProperty.Register(nameof(FilePath), typeof(string), typeof(BuildFilePathWrapper), new FrameworkPropertyMetadata(string.Empty));

        public static readonly DependencyProperty BuildPathProperty =
            DependencyProperty.Register(nameof(BuildPath), typeof(string), typeof(BuildFilePathWrapper), new FrameworkPropertyMetadata(string.Empty));

        public string FilePath
        {
            get { return (string)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public string BuildPath
        {
            get { return (string)GetValue(BuildPathProperty); }
            set { SetValue(BuildPathProperty, value); }
        }
    }

    public class BuildFilePathRule : ValidationRule
    {
        public BuildFilePathWrapper Wrapper { get; set; }

        public BuildFilePathRule()
        {

        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var path = (string)value;

            if (!string.IsNullOrEmpty(Wrapper.BuildPath) && (path.Contains(Wrapper.BuildPath) || Wrapper.BuildPath.Contains(path)))
            {
                return new ValidationResult(false, "Path must be within build and cannot be root folder of build.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
