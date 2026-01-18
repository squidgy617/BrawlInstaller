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
using Newtonsoft.Json;

namespace BrawlInstaller.Classes
{
    public class Tracklist
    {
        public string Name { get; set; }
        public string File { get; set; }
        public List<TracklistSong> TracklistSongs { get; set; } = new List<TracklistSong>();

        public Tracklist Copy()
        {
            return JsonConvert.DeserializeObject<Tracklist>(JsonConvert.SerializeObject(this));
        }

        public TLSTNode ConvertToNode()
        {
            var node = new TLSTNode { Name = Name };
            foreach(var song in TracklistSongs)
            {
                node.AddChild(song.ConvertToNode());
            }
            return node;
        }
    }
    public class TracklistSong
    {
        public string Name { get; set; } = string.Empty;
        public string SongPath { get; set; } = string.Empty;
        public string SongFile { get; set; } = null;
        public uint? SongId { get; set; } = null;
        public short SongDelay { get; set; } = 0;
        public byte Volume { get; set; } = 80;
        public byte Frequency { get; set; } = 40;
        public ushort SongSwitch { get; set; } = 0;
        public bool DisableStockPinch { get; set; } = false;
        public bool HiddenFromTracklist { get; set; } = false;
        public int Index { get; set; } = -1;
        public bool ReplaceExisting { get; set; } = false;

        public TLSTEntryNode ConvertToNode()
        {
            var newEntry = new TLSTEntryNode
            {
                Name = Name,
                SongID = SongId.Value,
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

        public TracklistSong Copy()
        {
            return JsonConvert.DeserializeObject<TracklistSong>(JsonConvert.SerializeObject(this));
        }

        public TracklistSong CopyNoFile()
        {
            var copy = Copy();
            copy.SongFile = null;
            return copy;
        }
    }
}
