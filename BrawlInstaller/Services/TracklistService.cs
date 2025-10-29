using BrawlInstaller.Classes;
using BrawlInstaller.Enums;
using BrawlLib.BrawlManagerLib.Songs;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.ResourceNodes.ProjectPlus;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface ITracklistService
    {
        /// <inheritdoc cref="TracklistService.GetTracklists(TracklistType)"/>
        List<string> GetTracklists(TracklistType tracklistType = TracklistType.Standard);

        /// <inheritdoc cref="TracklistService.GetTracklistSong(uint?, string)"/>
        TracklistSong GetTracklistSong(uint? songId, string tracklist);

        /// <inheritdoc cref="TracklistService.LoadTracklist(string, TracklistType)"/>
        Tracklist LoadTracklist(string tracklist, TracklistType tracklistType = TracklistType.Standard);

        /// <inheritdoc cref="TracklistService.GetAllTracklistSongs(string)"/>
        List<TracklistSong> GetAllTracklistSongs(string tracklist);

        /// <inheritdoc cref="TracklistService.DeleteTracklistSong(uint?, string, bool, bool, TracklistType)"/>
        void DeleteTracklistSong(uint? songId, string tracklist, bool deleteSongFile = true, bool deleteTracklistEntry = true, TracklistType tracklistType = TracklistType.Standard);

        /// <inheritdoc cref="TracklistService.ImportTracklistSong(TracklistSong, string, ResourceNode, TracklistType)"/>
        uint ImportTracklistSong(TracklistSong tracklistSong, string tracklist, ResourceNode brstmNode, TracklistType tracklistType = TracklistType.Standard);

        /// <inheritdoc cref="TracklistService.GetTracklistDeleteOptions(Tracklist)"/>
        List<string> GetTracklistDeleteOptions(Tracklist tracklist);

        /// <inheritdoc cref="TracklistService.SaveTracklist(Tracklist, List{string}, TracklistType)"/>
        Tracklist SaveTracklist(Tracklist tracklist, List<string> deleteOptions, TracklistType tracklistType = TracklistType.Standard);
    }

    [Export(typeof(ITracklistService))]
    internal class TracklistService : ITracklistService
    {
        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }

        [ImportingConstructor]
        public TracklistService(ISettingsService settingsService, IFileService fileService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
        }

        // Methods

        /// <summary>
        /// Get all tracklists in build
        /// </summary>
        /// <param name="tracklistType">Tracklist type</param>
        /// <returns>List of tracklists</returns>
        public List<string> GetTracklists(TracklistType tracklistType = TracklistType.Standard)
        {
            var tracklists = new List<string>();
            var buildPath = _settingsService.AppSettings.BuildPath;
            var tracklistPath = GetTracklistPath(tracklistType);
            var path = Path.Combine(buildPath, tracklistPath);
            tracklists = _fileService.GetFiles(path, "*.tlst").OrderBy(x => Path.GetFileName(x)).ToList();
            return tracklists;
        }

        /// <summary>
        /// Get tracklist path by type
        /// </summary>
        /// <param name="tracklistType">Tracklist type</param>
        /// <returns>Tracklist path</returns>
        private string GetTracklistPath(TracklistType tracklistType = TracklistType.Standard)
        {
            if (tracklistType == TracklistType.Netplay)
            {
                return _settingsService.BuildSettings.FilePathSettings.NetplaylistPath;
            }
            else
            {
                return _settingsService.BuildSettings.FilePathSettings.TracklistPath;
            }
        }

        /// <summary>
        /// Get synced tracklist paths by type
        /// </summary>
        /// <param name="tracklistType">Type of tracklist</param>
        /// <returns>List of tracklist paths</returns>
        private List<string> GetTracklistPaths(TracklistType tracklistType = TracklistType.Standard)
        {
            var tracklistPaths = new List<string>
            {
                GetTracklistPath(tracklistType)
            };
            if (_settingsService.BuildSettings.MiscSettings.SyncTracklists && tracklistType == TracklistType.Standard && !string.IsNullOrEmpty(_settingsService.BuildSettings.FilePathSettings.NetplaylistPath))
            {
                tracklistPaths.Add(_settingsService.BuildSettings.FilePathSettings.NetplaylistPath);
            }
            else if (_settingsService.BuildSettings.MiscSettings.SyncTracklists && tracklistType == TracklistType.Netplay && !string.IsNullOrEmpty(_settingsService.BuildSettings.FilePathSettings.TracklistPath))
            {
                tracklistPaths.Add(_settingsService.BuildSettings.FilePathSettings.TracklistPath);
            }
            return tracklistPaths;
        }

        /// <summary>
        /// Get song object from tracklist
        /// </summary>
        /// <param name="songId">ID of song to retrieve</param>
        /// <param name="tracklist">Tracklist to retrieve song from</param>
        /// <returns>Tracklist song object</returns>
        public TracklistSong GetTracklistSong(uint? songId, string tracklist)
        {
            var song = new TracklistSong();
            var rootNode = OpenTracklist(tracklist);
            if (rootNode != null)
            {
                var songNode = GetSongNode(rootNode, songId);
                if (songNode != null)
                {
                    song = GetTracklistSong(songNode);
                }
                _fileService.CloseFile(rootNode);
            }
            if (songId != null)
            {
                song.SongId = (uint)songId;
            }
            return song;
        }

        /// <summary>
        /// Load tracklist from file
        /// </summary>
        /// <param name="tracklist">Tracklist file to load</param>
        /// <param name="tracklistType">Tracklist type</param>
        /// <returns>Tracklist object</returns>
        public Tracklist LoadTracklist(string tracklist, TracklistType tracklistType = TracklistType.Standard)
        {
            var openedList = new Tracklist();
            var rootNode = OpenTracklist(tracklist, tracklistType);
            if (rootNode != null)
            {
                openedList.Name = rootNode.Name;
                openedList.File = rootNode.FilePath;
                openedList.TracklistSongs = GetAllTracklistSongs(rootNode);
                _fileService.CloseFile(rootNode);
            }
            return openedList;
        }
        
        /// <summary>
        /// Get all song objects from tracklist
        /// </summary>
        /// <param name="tracklist">Tracklist to open</param>
        /// <returns>TracklistSongs</returns>
        public List<TracklistSong> GetAllTracklistSongs(string tracklist)
        {
            var songs = new List<TracklistSong>();
            var rootNode = OpenTracklist(tracklist);
            if (rootNode != null)
            {
                songs = GetAllTracklistSongs(rootNode);
                _fileService.CloseFile(rootNode);
            }
            return songs;
        }

        /// <summary>
        /// Get all song objects from tracklist
        /// </summary>
        /// <param name="rootNode">Root node to get songs from</param>
        /// <returns>TracklistSongs</returns>
        private List<TracklistSong> GetAllTracklistSongs(ResourceNode rootNode)
        {
            var songs = new List<TracklistSong>();
            foreach (TLSTEntryNode songNode in rootNode.Children)
            {
                songs.Add(GetTracklistSong(songNode));
            }
            return songs;
        }


        /// <summary>
        /// Get tracklist song from node
        /// </summary>
        /// <param name="songNode">TLSTEntryNode</param>
        /// <returns>TracklistSong</returns>
        private TracklistSong GetTracklistSong(TLSTEntryNode songNode)
        {
            var song = new TracklistSong();
            song.Name = songNode.Name;
            song.SongId = songNode.SongID;
            song.SongPath = songNode.SongFileName;
            song.SongDelay = songNode.SongDelay;
            song.Volume = songNode.Volume;
            song.Frequency = songNode.Frequency;
            song.SongSwitch = songNode.SongSwitch;
            song.DisableStockPinch = songNode.DisableStockPinch;
            song.HiddenFromTracklist = songNode.HiddenFromTracklist;
            song.Index = songNode.Index;
            var brstmPath = _settingsService.BuildSettings.FilePathSettings.BrstmPath;
            var songPath = $"{songNode.SongFileName}.brstm";
            var songFile = Path.Combine(_settingsService.AppSettings.BuildPath, brstmPath, songPath);
            if (_fileService.FileExists(songFile))
            {
                song.SongFile = songFile;
            }
            return song;
        }

        /// <summary>
        /// Open tracklist by name
        /// </summary>
        /// <param name="tracklist">Tracklist to open</param>
        /// <param name="tracklistType">Type of tracklist</param>
        /// <returns>Tracklist root node</returns>
        private ResourceNode OpenTracklist(string tracklist, TracklistType tracklistType = TracklistType.Standard)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var tracklistPath = GetTracklistPath(tracklistType);
            var path = Path.Combine(buildPath, tracklistPath, $"{tracklist}.tlst");
            var rootNode = _fileService.OpenFile(path);
            if (rootNode != null)
            {
                return rootNode;
            }
            return null;
        }

        /// <summary>
        /// Get song node in tracklist
        /// </summary>
        /// <param name="rootNode">Tracklist node to search</param>
        /// <param name="songId">ID of song node</param>
        /// <returns>Song node</returns>
        private TLSTEntryNode GetSongNode(ResourceNode rootNode, uint? songId)
        {
            if (rootNode != null)
            {
                var songNode = rootNode.Children.FirstOrDefault(x => ((TLSTEntryNode)x).SongID == songId);
                if (songNode != null)
                {
                    return (TLSTEntryNode)songNode;
                }
            }
            return null;
        }

        /// <summary>
        /// Delete song on tracklist
        /// </summary>
        /// <param name="songId">ID of song to delete</param>
        /// <param name="tracklist">Tracklist song is in</param>
        /// <param name="tracklistType">Tracklist type</param>
        public void DeleteTracklistSong(uint? songId, string tracklist, bool deleteSongFile = true, bool deleteTracklistEntry = true, TracklistType tracklistType = TracklistType.Standard)
        {
            var rootNode = OpenTracklist(tracklist);
            if (rootNode != null)
            {
                var songNode = GetSongNode(rootNode, songId);
                if (songNode != null)
                {
                    var brstmPath = _settingsService.BuildSettings.FilePathSettings.BrstmPath;
                    var songPath = $"{songNode.SongFileName}.brstm";
                    var songFile = Path.Combine(_settingsService.AppSettings.BuildPath, brstmPath, songPath);
                    if (_fileService.FileExists(songFile) && deleteSongFile)
                    {
                        _fileService.DeleteFile(songFile);
                    }
                    if (deleteTracklistEntry)
                    {
                        rootNode.RemoveChild(songNode);
                        songNode.Dispose();
                    }
                }
                foreach (var tracklistPath in GetTracklistPaths(tracklistType))
                {
                    _fileService.SaveFileAs(rootNode, $"{Path.Combine(_settingsService.GetBuildFilePath(tracklistPath), rootNode.Name)}.tlst");
                }
                _fileService.CloseFile(rootNode);
            }
        }

        /// <summary>
        /// Add a song to a tracklist
        /// </summary>
        /// <param name="tracklistSong">Tracklist song object to add</param>
        /// <param name="tracklist">Tracklist to add to</param>
        /// <param name="tracklistType">Tracklist type</param>
        /// <returns>Added song ID</returns>
        private uint AddTracklistSong(TracklistSong tracklistSong, string tracklist, TracklistType tracklistType = TracklistType.Standard)
        {
            var rootNode = OpenTracklist(tracklist);
            if (rootNode != null)
            {
                // Generate tracklist node from object
                var newNode = tracklistSong.ConvertToNode();
                // If song ID is taken, generate a new one
                if (rootNode.Children.Select(x => ((TLSTEntryNode)x).SongID).ToList().Contains(tracklistSong.SongId.Value))
                {
                    tracklistSong.SongId = 0x0000F000;
                    while (rootNode.Children.Select(x => ((TLSTEntryNode)x).SongID).ToList().Contains(tracklistSong.SongId.Value))
                    {
                        tracklistSong.SongId++;
                    }
                    newNode.SongID = tracklistSong.SongId.Value;
                }
                if (tracklistSong.Index <= -1)
                {
                    rootNode.AddChild(newNode);
                    tracklistSong.Index = newNode.Index;
                }
                else
                {
                    rootNode.InsertChild(newNode, tracklistSong.Index);
                }
                foreach(var tracklistPath in GetTracklistPaths(tracklistType))
                {
                    _fileService.SaveFileAs(rootNode, $"{Path.Combine(_settingsService.GetBuildFilePath(tracklistPath), rootNode.Name)}.tlst");
                }
                _fileService.CloseFile(rootNode);
            }
            return tracklistSong.SongId.Value;
        }

        /// <summary>
        /// Import tracklist song
        /// </summary>
        /// <param name="tracklistSong">Tracklist song to import</param>
        /// <param name="tracklist">Tracklist to import to</param>
        /// <param name="brstmNode">BRSTM node to import</param>
        /// <param name="tracklistType">Type of tracklist</param>
        /// <returns>Added song ID</returns>
        public uint ImportTracklistSong(TracklistSong tracklistSong, string tracklist, ResourceNode brstmNode, TracklistType tracklistType = TracklistType.Standard)
        {
            var id = tracklistSong.SongId;
            if (id == null || string.IsNullOrEmpty(tracklistSong.SongPath))
            {
                return tracklistSong.SongId ?? 0x0000;
            }
            // Only add the song if it's using a custom TLST ID, otherwise it's a vBrawl song and should not be added
            if (tracklistSong.SongId >= 0x0000F000)
            {
                id = AddTracklistSong(tracklistSong, tracklist, tracklistType);
                if (brstmNode != null)
                {
                    var buildPath = _settingsService.AppSettings.BuildPath;
                    var strmPath = _settingsService.BuildSettings.FilePathSettings.BrstmPath;
                    var path = $"{Path.Combine(buildPath, strmPath, tracklistSong.SongPath)}.brstm";
                    brstmNode._origPath = path;
                    _fileService.SaveFile(brstmNode);
                    _fileService.CloseFile(brstmNode);
                    tracklistSong.SongFile = path;
                }
            }
            return id.Value;
        }

        /// <summary>
        /// Get potential options for deletion before saving a tracklist
        /// </summary>
        /// <param name="tracklist">Tracklist to be saved</param>
        /// <returns>Delete options</returns>
        public List<string> GetTracklistDeleteOptions(Tracklist tracklist)
        {
            var deleteOptions = new List<string>();
            var buildPath = _settingsService.AppSettings.BuildPath;
            var strmPath = _settingsService.BuildSettings.FilePathSettings.BrstmPath;
            var tracklistNode = _fileService.OpenFile(tracklist.File);
            if (tracklistNode != null)
            {
                foreach(TLSTEntryNode entry in tracklistNode.Children.Where(x => !string.IsNullOrEmpty(((TLSTEntryNode)x).SongFileName)))
                {
                    var brstmPath = $"{Path.Combine(buildPath, strmPath, entry?.SongFileName)}.brstm";
                    // If a BRSTM file exists and is not in the tracklist we're saving, prompt user to delete
                    if (_fileService.FileExists(brstmPath) && !tracklist.TracklistSongs.Any(x => x.SongPath == entry.SongFileName && !string.IsNullOrEmpty(x.SongFile)))
                    {
                        deleteOptions.Add(brstmPath);
                    }
                }
                _fileService.CloseFile(tracklistNode);
            }
            return deleteOptions;
        }

        /// <summary>
        /// Save a tracklist
        /// </summary>
        /// <param name="tracklist">Tracklist to save</param>
        /// <param name="deleteOptions">Selected options to delete</param>
        /// <param name="tracklistType">Type of tracklist</param>
        /// <returns>Updated tracklist</returns>
        public Tracklist SaveTracklist(Tracklist tracklist, List<string> deleteOptions, TracklistType tracklistType = TracklistType.Standard)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var strmPath = _settingsService.BuildSettings.FilePathSettings.BrstmPath;
            var tracklistPaths = GetTracklistPaths(tracklistType);
            // Open BRSTMs in case they get deleted
            var brstms = new List<ResourceNode>();
            foreach (var song in tracklist.TracklistSongs.Where(x => !string.IsNullOrEmpty(x.SongFile)))
            {
                var node = _fileService.OpenFile(song.SongFile);
                if (node != null)
                {
                    node._origPath = $"{Path.Combine(buildPath, strmPath, song.SongPath)}.brstm";
                    song.SongFile = node._origPath;
                    brstms.Add(node);
                }
            }
            // Delete selected BRSTMs
            foreach(var brstm in deleteOptions)
            {
                _fileService.DeleteFile(brstm);
            }
            // Delete old tracklist
            foreach(var tracklistPath in tracklistPaths)
            {
                if (!string.IsNullOrEmpty(tracklist?.File))
                {
                    var path = Path.Combine(_settingsService.GetBuildFilePath(tracklistPath), Path.GetFileName(tracklist.File));
                    if (_fileService.FileExists(path))
                    {
                        _fileService.DeleteFile(path);
                    }
                }
            }
            // Save BRSTMs
            foreach(var node in brstms)
            {
                _fileService.SaveFile(node);
                _fileService.CloseFile(node);
            }
            // Save tracklist
            if (!string.IsNullOrEmpty(tracklist.Name))
            {
                var tracklistNode = tracklist.ConvertToNode();
                foreach(var tracklistPath in tracklistPaths)
                {
                    var tracklistSavePath = $"{Path.Combine(buildPath, tracklistPath, tracklist.Name)}.tlst";
                    _fileService.SaveFileAs(tracklistNode, tracklistSavePath);
                    tracklist.File = tracklistSavePath;
                }
            }
            return tracklist;
        }
    }
}
