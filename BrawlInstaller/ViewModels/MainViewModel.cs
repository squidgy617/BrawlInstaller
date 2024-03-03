using BrawlInstaller.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.Windows.Input;
using BrawlLib.SSBB.ResourceNodes;
using System.Diagnostics;

namespace BrawlInstaller.ViewModels
{
    public interface IMainViewModel
    {
        ICommand ButtonCommand { get; }
        void Button();
        string Title { get; }
    }

    [Export(typeof(IMainViewModel))]
    internal class MainViewModel : ViewModelBase, IMainViewModel
    {
        public ICommand ButtonCommand {
            get
            {
                var buttonCommand = new RelayCommand(param => this.Button());
                return buttonCommand;
            }
        }

        public void Button()
        {
            var rootNode = NodeFactory.FromFile(null, "F:\\ryant\\Documents\\Ryan\\Brawl Mods\\SmashBuild\\Builds\\P+Ex\\pf\\menu2\\sc_selcharacter.pac");
            var testName = rootNode.Children.Last().Name;
            Debug.Print(testName);
        }

        // Importing constructor tells us that we want to get instance items provided in the constructor
        [ImportingConstructor]
        public MainViewModel()
        {
            Title = "Test title";
        }

        public string Title { get; }
    }
}
