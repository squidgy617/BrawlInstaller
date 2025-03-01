using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Velopack;

namespace BrawlInstaller
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            VelopackApp.Build().Run();
            App application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }
}
