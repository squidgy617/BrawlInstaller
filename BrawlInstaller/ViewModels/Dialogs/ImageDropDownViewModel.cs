using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BrawlInstaller.ViewModels
{
    public interface IImageDropDownViewModel : IDropDownViewModel
    {
        new BitmapImage Image { get; }
    }


    [Export(typeof(IImageDropDownViewModel))]
    internal class ImageDropDownViewModel : DropDownViewModel, IImageDropDownViewModel
    {
        // Importing constructor
        [ImportingConstructor]
        public ImageDropDownViewModel()
        {

        }

        // Properties
        [DependsUpon(nameof(SelectedItem))]
        new public BitmapImage Image { get => (BitmapImage)SelectedItem; }
    }
}
