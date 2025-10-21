using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.StaticClasses
{
    public static class Trophies
    {
        public static Dictionary<int, string> Series = new Dictionary<int, string>
        {
            { 0, "Super Smash Bros." },
            { 1, "The Subspace Emissary" },
            { 2, "Super Mario Bros." },
            { 3, "Donkey Kong" },
            { 4, "The Legend of Zelda" },
            { 5, "Metroid" },
            { 6, "Yoshi's Island" },
            { 7, "Kirby Super Star" },
            { 8, "Star Fox" },
            { 9, "Pokemon" },
            { 10, "F-Zero" },
            { 11, "Mother" },
            { 12, "Ice Climber" },
            { 13, "Fire Emblem" },
            { 14, "Kid Icarus" },
            { 15, "WarioWare" },
            { 16, "Pikmin" },
            { 17, "Animal Crossing" },
            { 18, "Game & Watch" },
            { 19, "Others" },
            { 20, "Metal Gear Solid" },
            { 21, "Sonic the Hedgehog" }
        };

        public static Dictionary<int, string> Categories = new Dictionary<int, string>
        {
            { 23, "Fighter" },
            { 24, "Fighter Related" },
            { 25, "Final Smash" },
            { 26, "Item" },
            { 27, "Assist Trophy" },
            { 28, "Poke Ball" },
            { 29, "The Subspace Emissary" },
            { 30, "Enemy" },
            { 31, "Stage" },
            { 32, "Others" }
        };
    }
}
