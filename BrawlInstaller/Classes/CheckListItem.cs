using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BrawlInstaller.Classes
{
    public class CheckListItem
    {
        public object Item { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public BitmapImage Image { get; set; } = null;
        public bool IsChecked { get; set; } = false;

        public CheckListItem(object item, string name, string description, bool isChecked = false, BitmapImage image = null)
        {
            Item = item;
            Name = name;
            Description = description;
            Image = image;
            IsChecked = isChecked;
        }
    }

    public class RadioButtonItem : CheckListItem
    {
        public string GroupName { get; set; } = string.Empty;

        public RadioButtonItem(object item, string name, string description, string groupName, bool isChecked = false, BitmapImage image = null) : base(item, name, description, isChecked, image)
        {
            Item = item;
            Name = name;
            Description = description;
            Image = image;
            IsChecked = isChecked;
            GroupName = groupName;
        }
    }

    public class RadioButtonGroup
    {
        public string DisplayName { get; set; }
        public string GroupName { get; set; }
        public List<RadioButtonItem> Items { get; set; } = new List<RadioButtonItem>();
    }
}
