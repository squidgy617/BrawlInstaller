using BrawlInstaller.Classes;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
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
        List<Cosmetic> GetFighterCosmetics(FighterIds fighterIds);
    }
    [Export(typeof(ICosmeticService))]
    internal class CosmeticService : ICosmeticService
    {
        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }

        [ImportingConstructor]
        public CosmeticService(ISettingsService settingsService, IFileService fileService) 
        {
            _settingsService = settingsService;
            _fileService = fileService;
        }

        // Methods
        public string FormatCosmeticId(CosmeticDefinition definition, int cosmeticId)
        {
            var id = (cosmeticId * definition.Multiplier).ToString("D" + definition.SuffixDigits);
            return id;
        }

        public List<string> GetCosmeticPaths(CosmeticDefinition definition, int id)
        {
            var buildPath = _settingsService.BuildPath;
            var paths = new List<string>();
            if (definition.InstallLocation.FilePath.EndsWith("\\"))
            {
                var formattedId = FormatCosmeticId(definition, id);
                var directoryInfo = new DirectoryInfo(buildPath + definition.InstallLocation.FilePath);
                var files = directoryInfo.GetFiles("*." + definition.InstallLocation.FileExtension, SearchOption.TopDirectoryOnly);
                if (definition.SeparateFiles)
                    paths = files.Where(f => f.Name.StartsWith(definition.Prefix) && CheckIdRange(definition.Multiplier, id, f.Name.Replace(f.Extension, ""), definition.Prefix)).Select(f => f.FullName).ToList();
                else
                    paths = files.Where(f => f.Name == definition.Prefix + FormatCosmeticId(definition, id) + "." + definition.InstallLocation.FileExtension).Select(f => f.FullName).ToList();
            }
            else
                paths.Add(buildPath + definition.InstallLocation.FilePath);
            return paths;
        }

        public bool CheckIdRange(int multiplier, int id, string name, string prefix)
        {
            var suffix = name.Replace(prefix, "").Replace(".", "");
            if (suffix != "")
            {
                var index = Convert.ToInt32(suffix);
                return CheckIdRange(multiplier, id, index);
            }
            return false;
        }

        public bool CheckIdRange(int multiplier, int id, int index)
        {
            var minRange = id * multiplier;
            var maxRange = multiplier > 1 ? minRange + multiplier : id;
            if (index <= maxRange && index > minRange)
                return true;
            return false;
        }

        public int GetCostumeIndex(TEX0Node node, CosmeticDefinition definition, int id)
        {
            string suffix;
            if (definition.SeparateFiles)
                suffix = node.RootNode.FileName.Replace(definition.Prefix, "").Replace("." + definition.InstallLocation.FileExtension, "");
            else
                suffix = node.Name.Replace(definition.Prefix, "").Replace(".", "");
            var isNumeric = int.TryParse(suffix, out int index);
            if (isNumeric)
            {
                index = index - (id * definition.Multiplier);
                return index;
            }
            return 0;
        }

        public int GetCostumeIndex(int index, CosmeticDefinition definition, int id)
        {
            index = index - (id * definition.Multiplier);
            return index;
        }

        public List<CosmeticTexture> GetTextures(CosmeticDefinition definition, ResourceNode node, int id, bool restrictRange)
        {
            var nodes = new List<CosmeticTexture>();
            if (definition.PatSettings != null)
            {
                var pat = node.FindChild(definition.PatSettings.Path);
                if (pat != null)
                {
                    var patEntries = pat.Children.Where(x => !restrictRange || CheckIdRange(definition.Multiplier, id, Convert.ToInt32(((PAT0TextureEntryNode)x).FrameIndex)));
                    foreach (PAT0TextureEntryNode patEntry in patEntries)
                    {
                        patEntry.GetImage(0);
                        nodes.Add(new CosmeticTexture { Texture = patEntry._textureNode, CostumeIndex = GetCostumeIndex(Convert.ToInt32(patEntry.FrameIndex), definition, id) });
                    }
                    return nodes;
                }
            }
            var start = definition.InstallLocation.NodePath != "" ? node.FindChild(definition.InstallLocation.NodePath) : node;
            if (start.GetType() == typeof(ARCNode))
                start = start.Children.First(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == id);
            var folder = start.FindChild("Textures(NW4R)");
            if (folder != null)
            {
                foreach (var child in folder.Children)
                {
                    if (child.GetType() == typeof(TEX0Node) && (!restrictRange || (child.Name.StartsWith(definition.Prefix) && CheckIdRange(definition.Multiplier, id, child.Name, definition.Prefix))))
                        nodes.Add(new CosmeticTexture { Texture = (TEX0Node)child, CostumeIndex = GetCostumeIndex((TEX0Node)child, definition, id) });
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
                    Image = texture.Texture.GetImage(0),
                    Texture = texture.Texture,
                    Palette = texture.Texture.GetPaletteNode(),
                    SharesData = texture.Texture.SharesData,
                    InternalIndex = cosmetics.Count(),
                    CostumeIndex = texture.CostumeIndex
                });
            }
            return cosmetics;
        }

        public List<Cosmetic> GetFighterCosmetics(FighterIds fighterIds)
        {
            var cosmetics = new List<Cosmetic>();
            var settings = _settingsService.BuildSettings;
            foreach (var cosmetic in settings.CosmeticSettings.GroupBy(c => new { c.CosmeticType, c.Style }).Select(g => g.First()).ToList())
            {
                var id = fighterIds.Ids.First(x => x.Type == cosmetic.IdType).Id;
                foreach (var path in GetCosmeticPaths(cosmetic, id))
                {
                    var rootNode = _fileService.OpenFile(path);
                    cosmetics.AddRange(GetCosmetics(cosmetic, rootNode, id, !cosmetic.InstallLocation.FilePath.EndsWith("\\")));
                    rootNode.Dispose();
                }
            }
            return cosmetics;
        }
    }
}
