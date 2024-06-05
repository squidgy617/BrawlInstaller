using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class TrackedList<T>
    {
        public List<T> ChangedItems { get; } = new List<T>();
        public List<T> Items { get; set; } = new List<T>();
        public void ItemChanged(T item)
        {
            if (!ChangedItems.Contains(item))
                ChangedItems.Add(item);
        }
        public void ClearChanges()
        {
            ChangedItems.Clear();
        }
        public bool HasChanged(T item)
        {
            if (ChangedItems.Contains(item))
                return true;
            return false;
        }
    }
}
