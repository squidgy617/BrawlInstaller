using BrawlInstaller.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.ViewModels
{
    public interface IMultiMessageViewModel : IDialogViewModelBase
    {
        List<DialogMessage> Messages { get; set; }
    }

    [Export(typeof(IMultiMessageViewModel))]
    internal class MultiMessageViewModel : DialogViewModelBase, IMultiMessageViewModel
    {
        // Private properties
        private List<DialogMessage> _messages;

        [ImportingConstructor]
        public MultiMessageViewModel()
        {

        }

        // Properties
        public List<DialogMessage> Messages { get => _messages; set { _messages = value; OnPropertyChanged(nameof(Messages)); } }
    }
}
