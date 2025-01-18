using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Enums
{
    [Flags]
    public enum GameCubeButtons : ushort
    {
        Left = 0x0001,
        Right = 0x0002,
        Down = 0x0004,
        Up = 0x0008,
        Z = 0x0010,
        R = 0x0020,
        L = 0x0040,
        Unused0x0080 = 0x0080,
        A = 0x0100,
        B = 0x0200,
        X = 0x0400,
        Y = 0x0800,
        Start = 0x1000,
        Unused0x2000 = 0x2000,
        Unused0x4000 = 0x4000,
        Unused0x8000 = 0x8000,
        EventAlt = 0xC000
    }
}
