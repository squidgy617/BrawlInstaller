using BrawlInstaller.Classes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BrawlInstaller.Validation
{
    public class FranchiseIconsWrapper : DependencyObject
    {
        public static readonly DependencyProperty FranchiseIconsProperty = 
            DependencyProperty.Register(nameof(FranchiseIcons), typeof(TrackedList<Cosmetic>), typeof(FranchiseIconsWrapper), new FrameworkPropertyMetadata(new TrackedList<Cosmetic>()));

        public TrackedList<Cosmetic> FranchiseIcons
        {
            get { return (TrackedList<Cosmetic>)GetValue(FranchiseIconsProperty); }
            set { SetValue(FranchiseIconsProperty, value); }
        }
    }
    public class FranchiseIdRule : ValidationRule
    {
        public FranchiseIconsWrapper Wrapper { get; set; }
        public FranchiseIdRule()
        {

        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var valid = int.TryParse((string)value, out int id);

            if (!valid || this.Wrapper.FranchiseIcons.Items.Any(x => x.Id == id))
            {
                return new ValidationResult(false, "Franchise icon ID already in use!");
            }
            return ValidationResult.ValidResult;
        }
    }
}
