using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrawlInstaller.Common
{
    /// <summary>
    /// Implementation of ICommand, to provide actions/predicates from view model that should be called on execution from a view
    /// </summary>
    public class RelayCommand : ICommand
    {
        private Predicate<object> _canExecute;
        private Action<object> _execute;

        public RelayCommand(Action execute)
        {
            _canExecute = x => true;
            _execute = x => { execute.Invoke(); };
        }

        public RelayCommand(Action<object> execute) : this(x => true, execute)
        {
        }

        public RelayCommand(Predicate<object> canExecute, Action<object> execute)
        {
            _canExecute = canExecute;
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();

        public bool CanExecute(object parameter) => _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);
    }
}
