using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BrawlInstaller.Helpers
{
    public class CursorWait : IDisposable
    {
        public CursorWait(bool appStarting = false)
        {
            // Wait
            Mouse.OverrideCursor = appStarting ? Cursors.AppStarting : Cursors.Wait;
        }

        public void Dispose()
        {
            // Reset
            Mouse.OverrideCursor = null;
        }
    }
}
