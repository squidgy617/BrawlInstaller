using BrawlInstaller.Classes;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface IExtractService
    {
        FighterPackage ExtractFighter(FighterIds fighterIds);
    }
    [Export(typeof(IExtractService))]
    internal class ExtractService : IExtractService
    {
        IFileService _fileService { get; }
        ISettingsService _settingsService { get; }
        [ImportingConstructor]
        public ExtractService(IFileService fileService, ISettingsService settingsService) 
        {
            _fileService = fileService;
            _settingsService = settingsService;
        }

        public string FormatCosmeticId(bool fiftyCC, int cosmeticId)
        {
            var id = fiftyCC ? (cosmeticId * 50).ToString("D4") : (cosmeticId * 10).ToString("D3");
            return id;
        }

        public string GetCosmeticPath(CosmeticDefinition definition, int id)
        {
            var buildPath = _settingsService.BuildPath;
            string path = "";
            if (definition.InstallLocation.FilePath.EndsWith("\\"))
            {
                var formattedId = FormatCosmeticId(definition.FiftyCC, id);
                path = buildPath + definition.InstallLocation.FilePath + definition.Prefix + formattedId + "." + definition.InstallLocation.FilExtension;
            }
            else
                path = buildPath + definition.InstallLocation.FilePath;
            return path;
        }

        public bool CheckIdRange(bool fiftyCC, int id, string name)
        {
            var suffix = name.Substring(name.LastIndexOf('.') + 1);
            if (suffix != "")
            {
                var minRange = fiftyCC ? id * 50 : id * 10;
                var maxRange = fiftyCC ? minRange + 50 : minRange + 10;
                var numToCheck = Convert.ToInt32(suffix);
                if (numToCheck < maxRange && numToCheck >= minRange)
                    return true;
            }
            return false;
        }

        public List<ResourceNode> GetCosmeticNodes(CosmeticDefinition definition, ResourceNode node, int id, string folderName, bool restrictRange)
        {
            var nodes = new List<ResourceNode>();
            var start = definition.InstallLocation.NodePath != "" ? node.FindChild(definition.InstallLocation.NodePath) : node;
            var folder = start.FindChild(folderName);
            if (folder != null)
            {
                foreach(var child in folder.Children)
                {
                    if (child.Name.StartsWith(definition.Prefix) && (!restrictRange || CheckIdRange(definition.FiftyCC, id, child.Name)))
                        nodes.Add(child);
                }
            }
            return nodes;
        }

        public List<ResourceNode> GetTextures(CosmeticDefinition definition, ResourceNode node, int id, bool restrictRange)
        {
            return GetCosmeticNodes(definition, node, id, "Textures(NW4R)", restrictRange);
        }

        public FighterPackage ExtractFighter(FighterIds fighterIds)
        {
            var fighterPackage = new FighterPackage();
            var settings = _settingsService.BuildSettings;
            foreach (var CSP in settings.CosmeticSettings.CSPs)
            {
                var rootNode = _fileService.OpenFile(GetCosmeticPath(CSP, fighterIds.CosmeticId));
                var textures = GetTextures(CSP, rootNode, fighterIds.CosmeticId, false);
                foreach (var texture in textures)
                {
                    Debug.Print(texture.Name);
                }
            }
            return fighterPackage;
        }
    }
}
