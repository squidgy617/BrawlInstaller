﻿using BrawlInstaller.Classes;
using BrawlLib.SSBB.ResourceNodes.ProjectPlus;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface ITracklistService
    {
        /// <inheritdoc cref="TracklistService.GetTracklists()"/>
        List<string> GetTracklists();

        /// <inheritdoc cref="TracklistService.GetTracklistSong(uint, string)"/>
        TracklistSong GetTracklistSong(uint? songId, string tracklist);
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
            var buildPath = _settingsService.AppSettings.BuildPath;
            var tracklistPath = _settingsService.BuildSettings.FilePathSettings.TracklistPath;
            var path = Path.Combine(buildPath, tracklistPath, $"{tracklist}.tlst");
            var rootNode = _fileService.OpenFile(path);
            if (rootNode != null)
            {
                var songNode = rootNode.Children.FirstOrDefault(x => ((TLSTEntryNode)x).SongID == songId);
                if (songNode != null)
                {
                    song.SongPath = ((TLSTEntryNode)songNode).SongFileName;
                    var brstmPath = _settingsService.BuildSettings.FilePathSettings.BrstmPath;
                    var songPath = $"{((TLSTEntryNode)songNode).SongFileName}.brstm";
                    var songFile = Path.Combine(buildPath, brstmPath, songPath);
                    if (File.Exists(songFile))
                    {
                        song.SongFile = songFile;
                    }
                }
                _fileService.CloseFile(rootNode);
            }
            return song;
        }
    }
}
