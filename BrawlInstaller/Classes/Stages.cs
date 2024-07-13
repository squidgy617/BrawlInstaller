using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class StageList
    {
        public List<StagePage> Pages = new List<StagePage>();
    }

    public class StagePage
    {
        public int PageNumber { get; set; }
        public List<BrawlIds> StageIds { get; set; } = new List<BrawlIds>();
    }
}
