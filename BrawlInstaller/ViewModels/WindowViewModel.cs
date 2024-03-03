using BrawlInstaller.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.ViewModels
{
    public interface IWindowViewModel
    {
        ViewModelBase CurrentViewModel { get; }
    }
    [Export(typeof(IWindowViewModel))]
    internal class WindowViewModel : ViewModelBase, IWindowViewModel
    {
        public WindowViewModel() 
        {
            CurrentViewModel = new MainViewModel();
        }

        public ViewModelBase CurrentViewModel { get; set; }
    }
}
