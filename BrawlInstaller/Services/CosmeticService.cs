using BrawlInstaller.Classes;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface ICosmeticService
    {
        string GetCosmeticPath(CosmeticDefinition definition, int id);
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

        public List<TEX0Node> GetTextures(CosmeticDefinition definition, ResourceNode node, int id, bool restrictRange)
        {
            var nodes = new List<TEX0Node>();
            var start = definition.InstallLocation.NodePath != "" ? node.FindChild(definition.InstallLocation.NodePath) : node;
            var folder = start.FindChild("Textures(NW4R)");
            if (folder != null)
            {
                foreach (var child in folder.Children)
                {
                    if (child.GetType() == typeof(TEX0Node) && child.Name.StartsWith(definition.Prefix) && (!restrictRange || CheckIdRange(definition.FiftyCC, id, child.Name)))
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
                    InternalIndex = cosmetics.Count()
                });
            }
            return cosmetics;
        }
    }
}
