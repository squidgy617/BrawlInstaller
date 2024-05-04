using BrawlInstaller.Classes;
using BrawlInstaller.Enums;
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
using BrawlInstaller.Common;
using static BrawlLib.BrawlManagerLib.TextureContainer;
using BrawlLib.Wii.Textures;
using System.Drawing;
using BrawlLib.Internal.Windows.Forms;
using System.Windows;

namespace BrawlInstaller.Services
{
    public interface ICosmeticService
    {
        List<Cosmetic> GetFighterCosmetics(FighterIds fighterIds);
        List<FranchiseCosmetic> GetFranchiseIcons();
        void ImportCosmetics(CosmeticDefinition definition, List<Cosmetic> cosmetics, int id);
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
        public TEX0Node ImportTexture(BRRESNode destinationNode, string imageSource, WiiPixelFormat format, System.Drawing.Size size)
        {
            var dialog = new TextureConverterDialog();
            dialog.ImageSource = imageSource;
            dialog.InitialFormat = format;
            dialog.Automatic = true;
            dialog.InitialSize = size;
            dialog.ShowDialog(null, destinationNode);
            var node = dialog.TEX0TextureNode;
            dialog.Dispose();
            return node;
        }

        public TEX0Node ImportTexture(BRRESNode destinationNode, TEX0Node texture)
        {
            destinationNode.GetOrCreateFolder<TEX0Node>()?.AddChild(texture);
            return texture;
        }

        public PLT0Node ImportPalette(BRRESNode destinationNode, PLT0Node palette)
        {
            destinationNode.GetOrCreateFolder<PLT0Node>()?.AddChild(palette);
            return palette;
        }

        public void RemoveTextures(BRRESNode parentNode, CosmeticDefinition definition, int id)
        {
            var folder = parentNode.GetFolder<TEX0Node>();
            if (folder != null)
            {
                folder.Children.RemoveAll(x => CheckIdRange(definition, id, x.Name, definition.Prefix));
            }
            RemovePalettes(parentNode, definition, id);
        }

        public void RemovePalettes(BRRESNode parentNode, CosmeticDefinition definition, int id)
        {
            var folder = parentNode.GetFolder<PLT0Node>();
            if (folder != null)
            {
                folder.Children.RemoveAll(x => CheckIdRange(definition, id, x.Name, definition.Prefix));
            }
        }

        public Cosmetic ImportCosmetic(CosmeticDefinition definition, Cosmetic cosmetic, int id, BRRESNode parentNode)
        {
            if (cosmetic.ImagePath != "")
            {
                var texture = ImportTexture(parentNode, cosmetic.ImagePath, definition.Format, definition.Size ?? new System.Drawing.Size(64, 64));
                texture.Name = $"{definition.Prefix}.{FormatCosmeticId(definition, id, cosmetic.CostumeIndex)}";
                cosmetic.Texture = (TEX0Node)_fileService.CopyNode(texture);
                cosmetic.Palette = texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(texture.GetPaletteNode()) : null;
                cosmetic.SharesData = texture.SharesData;
                cosmetic.ImagePath = "";
                return cosmetic;
            }
            else if (cosmetic.Texture != null)
            {
                var texture = ImportTexture(parentNode, cosmetic.Texture);
                if (cosmetic.Palette != null)
                    ImportPalette(parentNode, cosmetic.Palette);
                return cosmetic;
            }
            return null;
        }

        public void ImportCosmetics(CosmeticDefinition definition, List<Cosmetic> cosmetics, int id)
        {
            var paths = GetCosmeticPaths(definition, id);
            foreach (var path in paths)
            {
                var rootNode = _fileService.OpenFile(path);
                if (rootNode != null)
                {
                    var parentNode = definition.InstallLocation.NodePath != "" ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
                    if (parentNode != null)
                    {
                        RemoveTextures((BRRESNode)parentNode, definition, id);
                        foreach (var cosmetic in cosmetics.OrderBy(x => x.InternalIndex))
                        {
                            ImportCosmetic(definition, cosmetic, id, (BRRESNode)parentNode);
                        }
                        _fileService.SaveFile(rootNode);
                    }
                    _fileService.CloseFile(rootNode);
                }
            }
        }

        public string FormatCosmeticId(CosmeticDefinition definition, int cosmeticId)
        {
            var id = (cosmeticId * definition.Multiplier).ToString("D" + definition.SuffixDigits);
            return id;
        }

        public string FormatCosmeticId(CosmeticDefinition definition, int cosmeticId, int? costumeIndex)
        {
            var id = ((cosmeticId * definition.Multiplier) + costumeIndex ?? 0).ToString("D" + definition.SuffixDigits);
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
                    paths = files.Where(f => f.Name.StartsWith(definition.Prefix) && CheckIdRange(definition, id, f.Name.Replace(f.Extension, ""), definition.Prefix)).Select(f => f.FullName).ToList();
                else
                    paths = files.Where(f => f.Name == definition.Prefix + FormatCosmeticId(definition, id) + "." + definition.InstallLocation.FileExtension).Select(f => f.FullName).ToList();
            }
            else if (File.Exists(buildPath + definition.InstallLocation.FilePath))
                paths.Add(buildPath + definition.InstallLocation.FilePath);
            return paths;
        }

        public bool CheckIdRange(CosmeticDefinition definition, int id, string name, string prefix)
        {
            var suffix = name.Replace(prefix, "").Replace(".", "");
            if (suffix != "" && int.TryParse(suffix, out int index))
            {
                index = Convert.ToInt32(suffix);
                return CheckIdRange(definition.IdType, definition.Multiplier, id, index);
            }
            return false;
        }

        public bool CheckIdRange(PatSettings patSettings, int id, int index)
        {
            return CheckIdRange(idType: patSettings.IdType ?? IdType.Cosmetic, multiplier: patSettings.Multiplier ?? 1, id, index);
        }

        public bool CheckIdRange(IdType idType, int multiplier, int id, int index)
        {
            // TODO: Do we really only check this for cosmetic IDs?
            if (idType != IdType.Cosmetic)
                return index == id;
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
                return GetCostumeIndex(index, definition.Multiplier, id);
            }
            return 0;
        }

        public int GetCostumeIndex(int index, int multiplier, int id)
        {
            if (multiplier <= 1)
                return 0;
            index = index - (id * multiplier);
            return index;
        }

        public int? GetCosmeticId(string name, CosmeticDefinition definition)
        {
            if (name != null && name.StartsWith(definition.Prefix))
            {
                var success = int.TryParse(name.Replace(definition.Prefix, "").Replace(".", "").Replace("_TopN",""), System.Globalization.NumberStyles.Integer, null, out int id);
                if (success)
                    return id;
            }
            return null;
        }

        public List<CosmeticTexture> GetTextures(CosmeticDefinition definition, ResourceNode node, FighterIds fighterIds, bool restrictRange)
        {
            // Try to get all textures for cosmetic definition
            var nodes = new List<CosmeticTexture>();
            var id = fighterIds.GetIdOfType(definition.IdType) + definition.Offset;
            // If the definition contains PatSettings, check the PAT0 first
            if (definition.PatSettings != null)
            {
                var pat = node.FindChild(definition.PatSettings.Paths.First());
                if (pat != null)
                {
                    var patEntries = new List<ResourceNode>();
                    // If PatSettings have their own IdType and Offset, use those instead of the base definition values
                    if (definition.PatSettings.IdType != null)
                    {
                        id = fighterIds.GetIdOfType(definition.PatSettings.IdType ?? definition.IdType) + (definition.PatSettings.Offset ?? definition.Offset);
                        patEntries = pat.Children.Where(x => !restrictRange || CheckIdRange(definition.PatSettings, id, Convert.ToInt32(((PAT0TextureEntryNode)x).FrameIndex))).ToList();
                    }
                    else
                        patEntries = pat.Children.Where(x => !restrictRange || CheckIdRange(definition.IdType, definition.Multiplier, id, Convert.ToInt32(((PAT0TextureEntryNode)x).FrameIndex))).ToList();
                    foreach (PAT0TextureEntryNode patEntry in patEntries)
                    {
                        // Get the texture from the pat entry
                        patEntry.GetImage(0);
                        nodes.Add(new CosmeticTexture { Texture = patEntry._textureNode, CostumeIndex = GetCostumeIndex(Convert.ToInt32(patEntry.FrameIndex), definition.PatSettings.Multiplier ?? definition.Multiplier, id), Id = (int)patEntry.FrameIndex });
                    }
                    return nodes;
                }
            }
            var start = definition.InstallLocation.NodePath != "" ? node.FindChild(definition.InstallLocation.NodePath) : node;
            // If the node path is an ARC node, search for a matching BRRES first
            if (start.GetType() == typeof(ARCNode))
                start = start.Children.First(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == id);
            var folder = start.FindChild("Textures(NW4R)");
            if (folder != null)
            {
                foreach (var child in folder.Children)
                {
                    // Get textures that match definition
                    if (child.GetType() == typeof(TEX0Node) && (definition.SeparateFiles || child.Name.StartsWith(definition.Prefix)) && (!restrictRange || CheckIdRange(definition, id, child.Name, definition.Prefix)))
                        nodes.Add(new CosmeticTexture { Texture = (TEX0Node)child, CostumeIndex = GetCostumeIndex((TEX0Node)child, definition, id), Id = GetCosmeticId(child.Name, definition) });
                }
            }
            return nodes;
        }

        public List<MDL0Node> GetModels(CosmeticDefinition definition, ResourceNode node, FighterIds fighterIds, bool restrictRange)
        {
            // Try to get all models for cosmetic definition
            var nodes = new List<MDL0Node>();
            var id = fighterIds.GetIdOfType(definition.IdType) + definition.Offset;
            var start = definition.ModelPath != null ? node.FindChild(definition.ModelPath) : node;
            // If the node path is an ARC node, search for a matching BRRES first
            if (start.GetType() == typeof(ARCNode))
                start = start.Children.First(x => x.ResourceFileType == ResourceType.BRES && (!restrictRange || ((BRRESNode)x).FileIndex == id));
            var folder = start.FindChild("3DModels(NW4R)");
            if (folder != null)
            {
                foreach (var child in folder.Children)
                {
                    // Get models that match definition
                    if (child.GetType() == typeof(MDL0Node) && child.Name.StartsWith(definition.Prefix) && (!restrictRange || CheckIdRange(definition, id, child.Name.Replace("_TopN", ""), definition.Prefix)))
                        nodes.Add((MDL0Node)child);
                }
            }
            return nodes;
        }

        public List<Cosmetic> GetDefinitionCosmetics(CosmeticDefinition definition, ResourceNode node, FighterIds fighterIds, bool restrictRange)
        {
            // Get textures for provided definition and IDs
            var cosmetics = new List<Cosmetic>();
            var textures = GetTextures(definition, node, fighterIds, restrictRange);
            foreach(var texture in textures)
            {
                cosmetics.Add(new Cosmetic
                {
                    CosmeticType = definition.CosmeticType,
                    Style = definition.Style,
                    Image = texture.Texture?.GetImage(0).ToBitmapImage(),
                    Texture = (TEX0Node)_fileService.CopyNode(texture.Texture),
                    Palette = texture.Texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(texture.Texture.GetPaletteNode()) : null,
                    SharesData = texture.Texture.SharesData,
                    InternalIndex = cosmetics.Count(),
                    CostumeIndex = texture.CostumeIndex,
                    Id = texture.Id
                });
            }
            if (definition.ModelPath != null)
            {
                var models = GetModels(definition, node, fighterIds, restrictRange);
                foreach (var model in models)
                {
                    cosmetics.Add(new Cosmetic
                    {
                        CosmeticType = definition.CosmeticType,
                        Style = definition.Style,
                        Model = (MDL0Node)_fileService.CopyNode(model),
                        Id = GetCosmeticId(model.Name, definition)
                    });
                }
            }
            return cosmetics;
        }

        // TODO: Importing cosmetics
        // Use the Cosmetic class
        // If the cosmetic has a TEX0Node, you import that. Otherwise, import the image
        // When editing a character, if you change a cosmetic, its TEX0Node is automatically nulled and SharesData is set to False
        // You can then color smash these from the UI, but this will just flip SharesData. Their images will get imported instead of a TEX0
        // This allows us to differentiate between edited cosmetics and ones that should just remain color smashed
        // Also, when we import an image for the first time, we set the TEX0 for the class, which allows us to reuse it? (Alternatively we just set up the TEX0s at the start)
        // From the screen, cosmetics can be un-color smashed and reordered (which just changes internal order)

        // TODO: When importing a character, franchise icons with a null ID or an ID greater than any existing franchise icon will be installed as new. Any others
        // will overwrite existing ones.

        // TODO: separate RSPs and CSPs so they can be toggled on install

        /// <summary>
        /// Get a list of all cosmetics for a fighter
        /// </summary>
        /// <param name="fighterIds">IDs of fighter to retrieve cosmetics from</param>
        /// <returns></returns>
        public List<Cosmetic> GetFighterCosmetics(FighterIds fighterIds)
        {
            var settings = _settingsService.BuildSettings;
            var definitions = settings.CosmeticSettings;
            return GetCosmetics(fighterIds, definitions, true);
        }

        /// <summary>
        /// Get a list of all franchise icons
        /// </summary>
        /// <returns></returns>
        public List<FranchiseCosmetic> GetFranchiseIcons()
        {
            var franchiseIcons = new List<FranchiseCosmetic>();
            var settings = _settingsService.BuildSettings;
            var definitions = settings.CosmeticSettings.Where(x => x.CosmeticType == CosmeticType.FranchiseIcon).ToList();
            // Get all franchise icons
            var allIcons = GetCosmetics(new FighterIds(), definitions, false);
            // Aggregate the models and transparent textures
            foreach(var icon in allIcons.Where(x => x.Style == "Icon").GroupBy(x => x.Id).Select(x => x.First()).ToList())
            {
                franchiseIcons.Add(new FranchiseCosmetic
                {
                    CosmeticType = icon.CosmeticType,
                    Style = icon.Style,
                    Image = icon.Image,
                    Texture = icon.Texture,
                    Palette = icon.Palette,
                    SharesData = icon.SharesData,
                    InternalIndex = icon.InternalIndex,
                    CostumeIndex = icon.CostumeIndex,
                    Id = icon.Id,
                    Model = allIcons.FirstOrDefault(x => x.Id == icon.Id && x.Style == "Model" && x.Model != null)?.Model,
                    TransparentImage = allIcons.FirstOrDefault(x => x.Id == icon.Id && x.Style == "Model" && x.Image != null)?.Image,
                    TransparentTexture = allIcons.FirstOrDefault(x => x.Id == icon.Id && x.Style == "Model" && x.Texture != null)?.Texture
                });
            }
            return franchiseIcons;
        }

        public List<Cosmetic> GetCosmetics(FighterIds fighterIds, List<CosmeticDefinition> definitions, bool restrictRange)
        {
            var cosmetics = new List<Cosmetic>();
            // Get all cosmetic definitions grouped by type and style
            foreach (var cosmeticGroup in definitions.GroupBy(c => new { c.CosmeticType, c.Style }).ToList())
            {
                // Check each definition in the group for cosmetics
                foreach (var cosmetic in cosmeticGroup)
                {
                    var id = fighterIds.GetIdOfType(cosmetic.IdType) + cosmetic.Offset;
                    // Check all paths for the cosmetic definition
                    foreach (var path in GetCosmeticPaths(cosmetic, id))
                    {
                        var rootNode = _fileService.OpenFile(path);
                        if (rootNode != null)
                        {
                            cosmetics.AddRange(GetDefinitionCosmetics(cosmetic, rootNode, fighterIds, restrictRange && !cosmetic.InstallLocation.FilePath.EndsWith("\\")));
                            _fileService.CloseFile(rootNode);
                        }
                    }
                    // If we found cosmetics, don't bother checking the other definitions in the group - proceed to the next group
                    if (cosmetics.Count > 0)
                        break;
                }
            }
            return cosmetics;
        }
    }
}
