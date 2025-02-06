using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.StaticClasses
{
    public static class Trophies
    {
        public static Dictionary<string, int> Series = new Dictionary<string, int>
        {
            { "Super Smash Bros.", 0 },
            { "The Subspace Emissary", 1 },
            { "Super Mario Bros.", 2 },
            { "Donkey Kong", 3 },
            { "The Legend of Zelda", 4 },
            { "Metroid", 5 },
            { "Yoshi's Island", 6 },
            { "Kirby Super Star", 7 },
            { "Star Fox", 8 },
            { "Pokemon", 9 },
            { "F-Zero", 10 },
            { "Mother", 11 },
            { "Ice Climber", 12 },
            { "Fire Emblem", 13 },
            { "Kid Icarus", 14 },
            { "WarioWare", 15 },
            { "Pikmin", 16 },
            { "Animal Crossing", 17 },
            { "Game & Watch", 18 },
            { "Others", 19 },
            { "Metal Gear Solid", 20 },
            { "Sonic the Hedgehog", 21 }
        };

        public static Dictionary<string, int> Categories = new Dictionary<string, int>
        {
            { "Fighter", 23 },
            { "Fighter Related", 24 },
            { "Final Smash", 25 },
            { "Item", 26 },
            { "Assist Trophy", 27 },
            { "Poke Ball", 28 },
            { "The Subspace Emissary", 29 },
            { "Enemy", 30 },
            { "Stage", 31 },
            { "Others", 32 }
        };
    }
}
