using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public class CostumeSwap
    {
        public int SwapFighterId { get; set; }
        public int CostumeId { get; set; }
        public int NewCostumeId { get; set; }

        public CostumeSwap()
        {

        }

        public CostumeSwap(int swapFighterId, int costumeId, int newCostumeId)
        {
            SwapFighterId = swapFighterId;
            CostumeId = costumeId;
            NewCostumeId = newCostumeId;
        }
    }
}
