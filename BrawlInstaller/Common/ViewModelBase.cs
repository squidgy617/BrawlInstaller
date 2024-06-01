using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Common
{
    /// <summary>
    /// Base for view models to provide property changed notifications
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
        protected class DependsUponAttribute : Attribute
        {
            public string DependencyName { get; private set; }

            public DependsUponAttribute(string propertyName)
            {
                DependencyName = propertyName;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handlers = PropertyChanged;
            if (handlers != null)
            {
                foreach (var property in AllNotifiedProperties(propertyName))
                    handlers(this, new PropertyChangedEventArgs(property));
            }
        }

        private IEnumerable<string> DependentProperties(string inputName)
        {
            return from property in GetType().GetProperties()
                   where property.GetCustomAttributes(typeof(DependsUponAttribute), true).Cast<DependsUponAttribute>()
                         .Any(attribute => attribute.DependencyName == inputName)
                   select property.Name;
        }

        private IEnumerable<string> NotifiedProperties(IEnumerable<string> inputs)
        {
            var dependencies = from input in inputs
                               from dependency in DependentProperties(input)
                               select dependency;

            return inputs.Union(dependencies).Distinct();
        }

        private IEnumerable<string> AllNotifiedProperties(string inputName)
        {
            IEnumerable<string> results = new[] { inputName };

            while (NotifiedProperties(results).Count() > results.Count())
                results = NotifiedProperties(results);

            return results;
        }
    }
}
