using BrawlInstaller.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            DependencyProperty.Register(nameof(FranchiseIcons), typeof(ObservableCollection<Cosmetic>), typeof(FranchiseIconsWrapper), new FrameworkPropertyMetadata(new ObservableCollection<Cosmetic>()));

        public static readonly DependencyProperty OldIdProperty =
            DependencyProperty.Register(nameof(OldId), typeof(int?), typeof(FranchiseIconsWrapper), new FrameworkPropertyMetadata(0));

        public ObservableCollection<Cosmetic> FranchiseIcons
        {
            get { return (ObservableCollection<Cosmetic>)GetValue(FranchiseIconsProperty); }
            set { SetValue(FranchiseIconsProperty, value); }
        }

        public int? OldId
        {
            get { return (int?)GetValue(OldIdProperty); }
            set { SetValue(OldIdProperty, value); }
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

            if (!valid || (Wrapper.FranchiseIcons.Any(x => x.Id == id) && Wrapper.OldId != id))
            {
                return new ValidationResult(false, "Franchise icon ID already in use!");
            }
            return ValidationResult.ValidResult;
        }
    }
}
