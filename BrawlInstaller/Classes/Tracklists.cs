using BrawlLib.SSBB.ResourceNodes.ProjectPlus;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static BrawlLib.SSBB.ResourceNodes.ProjectPlus.STEXNode;
using BrawlLib.BrawlManagerLib.Songs;

namespace BrawlInstaller.Classes
{
    public class TracklistSong
    {
        public string Name { get; set; } = string.Empty;
        public string SongPath { get; set; } = string.Empty;
        public string SongFile { get; set; } = null;
        public uint SongId { get; set; } = 0x0000FF00;
        public short SongDelay { get; set; } = 0;
        public byte Volume { get; set; } = 80;
        public byte Frequency { get; set; } = 40;
        public ushort SongSwitch { get; set; } = 0;
        public bool DisableStockPinch { get; set; } = false;
        public bool HiddenFromTracklist { get; set; } = false;
        public int Index { get; set; } = -1;

        public TLSTEntryNode ConvertToNode()
        {
            var newEntry = new TLSTEntryNode
            {
                Name = Name,
                SongID = SongId,
                SongFileName = SongPath,
                SongDelay = SongDelay,
                Volume = Volume,
                Frequency = Frequency,
                SongSwitch = SongSwitch,
                DisableStockPinch = DisableStockPinch,
                HiddenFromTracklist = HiddenFromTracklist
        };
            return newEntry;
        }
    }
}
