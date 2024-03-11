using BrawlInstaller.Classes;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BrawlInstaller.Services
{
    public interface ICosmeticService
    {
        List<string> GetCosmeticPaths(CosmeticDefinition definition, int id);
        List<Cosmetic> GetCosmetics(CosmeticDefinition definition, ResourceNode node, int id, bool restrictRange);
    }
    [Export(typeof(ICosmeticService))]
    internal class CosmeticService : ICosmeticService
    {
        // Services
        ISettingsService _settingsService { get; }

        [ImportingConstructor]
        public CosmeticService(ISettingsService settingsService) 
        {
            _settingsService = settingsService;
        }

        // Methods
        public string FormatCosmeticId(bool fiftyCC, int cosmeticId)
        {
            var id = fiftyCC ? (cosmeticId * 50).ToString("D4") : (cosmeticId * 10).ToString("D3");
            return id;
        }

        public List<string> GetCosmeticPaths(CosmeticDefinition definition, int id)
        {
            var buildPath = _settingsService.BuildPath;
            var paths = new List<string>();
            if (definition.InstallLocation.FilePath.EndsWith("\\"))
            {
                var formattedId = FormatCosmeticId(definition.FiftyCC, id);
                var directoryInfo = new DirectoryInfo(buildPath + definition.InstallLocation.FilePath);
                var files = directoryInfo.GetFiles("*." + definition.InstallLocation.FileExtension, SearchOption.TopDirectoryOnly);
                paths = files.Where(f => f.Name.StartsWith(definition.Prefix) && CheckIdRange(definition.FiftyCC, id, f.Name.Replace(f.Extension, ""), definition.Prefix)).Select(f => f.FullName).ToList();
            }
            else
                paths.Add(buildPath + definition.InstallLocation.FilePath);
            return paths;
        }

        public bool CheckIdRange(bool fiftyCC, int id, string name, string prefix)
        {
            var suffix = name.Replace(prefix, "").Replace(".", "");
            if (suffix != "")
            {
                var minRange = fiftyCC ? id * 50 : id * 10;
                var maxRange = fiftyCC ? minRange + 50 : minRange + 10;
                var numToCheck = Convert.ToInt32(suffix);
                if (numToCheck <= maxRange && numToCheck > minRange)
                    return true;
            }
            return false;
        }

        public int GetCostumeIndex(TEX0Node node, CosmeticDefinition definition, int id)
        {
            string suffix;
            if (definition.MultiFile)
                suffix = node.RootNode.FileName.Replace(definition.Prefix, "").Replace("." + definition.InstallLocation.FileExtension, "");
            else
                suffix = node.Name.Replace(definition.Prefix, "").Replace(".", "");
            var isNumeric = int.TryParse(suffix, out int index);
            if (isNumeric)
            {
                index = (definition.FiftyCC ? index - (id * 50) : index - (id * 10)) - 1;
                return index;
            }
            return 0;
        }

        public List<TEX0Node> GetTextures(CosmeticDefinition definition, ResourceNode node, int id, bool restrictRange)
        {
            var nodes = new List<TEX0Node>();
            var start = definition.InstallLocation.NodePath != "" ? node.FindChild(definition.InstallLocation.NodePath) : node;
            var folder = start.FindChild("Textures(NW4R)");
            if (folder != null)
            {
                foreach (var child in folder.Children)
                {
                    if (child.GetType() == typeof(TEX0Node) && (!restrictRange || (child.Name.StartsWith(definition.Prefix) && CheckIdRange(definition.FiftyCC, id, child.Name, definition.Prefix))))
                        nodes.Add((TEX0Node) child);
                }
            }
            return nodes;
        }

        public List<Cosmetic> GetCosmetics(CosmeticDefinition definition, ResourceNode node, int id, bool restrictRange)
        {
            var cosmetics = new List<Cosmetic>();
            var textures = GetTextures(definition, node, id, restrictRange);
            foreach(var texture in textures)
            {
                cosmetics.Add(new Cosmetic
                {
                    CosmeticType = definition.CosmeticType,
                    Style = definition.Style,
                    Texture = texture,
                    Palette = texture.GetPaletteNode(),
                    SharesData = texture.SharesData,
                    InternalIndex = cosmetics.Count(),
                    CostumeIndex = GetCostumeIndex(texture, definition, id)
                });
            }
            return cosmetics;
        }
    }
}
