using BrawlCrate.UI;
using BrawlInstaller.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace BrawlInstaller.ViewModels
{
    public interface IMessageViewModel : IDialogViewModelBase
    {

    }

    [Export(typeof(IMessageViewModel))]
    internal class MessageViewModel : DialogViewModelBase, IMessageViewModel
    {
        // Importing constructor
        [ImportingConstructor]
        public MessageViewModel()
        {

        }
    }
}
