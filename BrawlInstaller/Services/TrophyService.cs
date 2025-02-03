using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface ITrophyService
    {

    }

    [Export(typeof(ITrophyService))]
    internal class TrophyService : ITrophyService
    {
        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }
        ICosmeticService _cosmeticService { get; }

        [ImportingConstructor]
        public TrophyService(ISettingsService settingsService, IFileService fileService, ICosmeticService cosmeticService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _cosmeticService = cosmeticService;
        }

        // Methods

    }
}
