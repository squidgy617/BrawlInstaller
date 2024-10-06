﻿using BrawlInstaller.Classes;
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
        /// <inheritdoc cref="TracklistService.GetTracklists()"/>
        List<string> GetTracklists();

        /// <inheritdoc cref="TracklistService.GetTracklistSong(uint?, string)"/>
        TracklistSong GetTracklistSong(uint? songId, string tracklist);

        /// <inheritdoc cref="TracklistService.DeleteTracklistSong(uint?, string)"/>
        void DeleteTracklistSong(uint? songId, string tracklist);

        /// <inheritdoc cref="TracklistService.ImportTracklistSong(TracklistSong, string, ResourceNode)"/>
        uint ImportTracklistSong(TracklistSong tracklistSong, string tracklist, ResourceNode brstmNode);
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
        /// <returns>List of tracklists</returns>
        public List<string> GetTracklists()
        {
            var tracklists = new List<string>();
            var buildPath = _settingsService.AppSettings.BuildPath;
            var tracklistPath = _settingsService.BuildSettings.FilePathSettings.TracklistPath;
            var path = Path.Combine(buildPath, tracklistPath);
            if (Directory.Exists(path))
            {
                tracklists = Directory.GetFiles(path, "*.tlst").ToList();
            }
            return tracklists;
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
                    if (File.Exists(songFile))
                    {
                        song.SongFile = songFile;
                    }
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
        /// Open tracklist by name
        /// </summary>
        /// <param name="tracklist">Tracklist to open</param>
        /// <returns>Tracklist root node</returns>
        private ResourceNode OpenTracklist(string tracklist)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var tracklistPath = _settingsService.BuildSettings.FilePathSettings.TracklistPath;
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
        public void DeleteTracklistSong(uint? songId, string tracklist)
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
                    if (File.Exists(songFile))
                    {
                        _fileService.DeleteFile(songFile);
                    }
                    rootNode.RemoveChild(songNode);
                    songNode.Dispose();
                }
                _fileService.SaveFile(rootNode);
                _fileService.CloseFile(rootNode);
            }
        }

        /// <summary>
        /// Add a song to a tracklist
        /// </summary>
        /// <param name="tracklistSong">Tracklist song object to add</param>
        /// <param name="tracklist">Tracklist to add to</param>
        /// <returns>Added song ID</returns>
        private uint AddTracklistSong(TracklistSong tracklistSong, string tracklist)
        {
            var rootNode = OpenTracklist(tracklist);
            if (rootNode != null)
            {
                // Generate tracklist node from object
                var newNode = tracklistSong.ConvertToNode();
                // If song ID is taken, generate a new one
                if (rootNode.Children.Select(x => ((TLSTEntryNode)x).SongID).ToList().Contains(tracklistSong.SongId))
                {
                    tracklistSong.SongId = 0x0000F000;
                    while (rootNode.Children.Select(x => ((TLSTEntryNode)x).SongID).ToList().Contains(tracklistSong.SongId))
                    {
                        tracklistSong.SongId++;
                    }
                    newNode.SongID = tracklistSong.SongId;
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
                _fileService.SaveFile(rootNode);
                _fileService.CloseFile(rootNode);
            }
            return tracklistSong.SongId;
        }

        /// <summary>
        /// Import tracklist song
        /// </summary>
        /// <param name="tracklistSong">Tracklist song to import</param>
        /// <param name="tracklist">Tracklist to import to</param>
        /// <param name="brstmNode">BRSTM node to import</param>
        /// <returns>Added song ID</returns>
        public uint ImportTracklistSong(TracklistSong tracklistSong, string tracklist, ResourceNode brstmNode)
        {
            var id = tracklistSong.SongId;
            // Only add the song if it's using a custom TLST ID, otherwise it's a vBrawl song and should not be added
            if (tracklistSong.SongId >= 0x0000F000)
            {
                id = AddTracklistSong(tracklistSong, tracklist);
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
            return id;
        }
    }
}
