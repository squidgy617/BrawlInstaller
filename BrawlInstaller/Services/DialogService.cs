using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BrawlInstaller.Services
{
    public interface IDialogService
    {
        bool ShowMessage(string text, string caption);
    }
    [Export(typeof(IDialogService))]
    internal class DialogService : IDialogService
    {
        [ImportingConstructor]
        public DialogService() { }

        // Methods
        public bool ShowMessage(string text, string caption)
        {
            var result = MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.OK || result == MessageBoxResult.None)
                return true;
            else
                return false;
        }
    }
}
