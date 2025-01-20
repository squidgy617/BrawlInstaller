using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.StaticClasses
{
    // TODO: Possibly make this configurable eventually? Maybe we don't have to because they can just add entries to the fighter list for that?
    public static class ReservedIds
    {
        public static List<int> ReservedStageCosmeticIds = new List<int> { 80, 81, 82, 83, 84, 85, 100, 101, 102 }; // Things like stage builder, home run contest, etc
        public static List<int> ReservedStageIds = new List<int> { 0x26, 0x28, 0x3D, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39 }; // Things like stage builder, home run contest, etc
        public static List<int> ReservedFighterConfigIds = new List<int> { 0x48, 0x49, 0x4A }; // Used for SSE characters
        public static List<int> ReservedFighterCosmeticIds = new List<int>
        { 
            20, // Reserved because Ganondorf uses both frame 19 and 20 in SSE for some reason
            27, // Used by Pokemon Trainer, which is still in SSE even in builds that don't have him
            39, // Empty slot
            50, // Random
            60, // Sandbag
            61, // Target Test
            62, // Red Alloy
            63, // Blue Alloy
            64, // Green Alloy
            65, // Yellow Alloy
            98, // Smash Logo
            99, // Petey Piranha
            100, // Rayquaza
            101, // Porky Statue
            102, // Porky
            103, // Headrobo
            104, // Ridley
            105, // Duon
            106, // Meta Ridley
            107, // Tabuu
            108, // Master Hand
            109, // Crazy Hand
            127 // Causes issues with stock icons?
        };
        public static List<string> ReservedInternalNames = new List<string> { "zakoball", "zakoboy", "zakochild", "zakogirl" };
    }
}
