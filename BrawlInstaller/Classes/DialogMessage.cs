using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class DialogMessage
    {
        public string Title { get; set; }
        public string Message { get; set; }

        public DialogMessage(string title, string message) 
        { 
            Title = title;
            Message = message;
        }
    }
}
