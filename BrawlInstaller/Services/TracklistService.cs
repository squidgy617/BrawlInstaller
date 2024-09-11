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
    }

    [Export(typeof(ITracklistService))]
    internal class TracklistService : ITracklistService
    {
        // Services
        ISettingsService _settingsService { get; }

        [ImportingConstructor]
        public TracklistService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
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
    }
}
