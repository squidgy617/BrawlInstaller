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
using BrawlLib.Wii.Textures;
using System.Drawing;
using BrawlLib.Internal.Windows.Forms;
using System.Windows;
using System.Windows.Forms;
using static BrawlLib.BrawlManagerLib.TextureContainer;
using System.Windows.Media.Imaging;
using System.Collections.Concurrent;

namespace BrawlInstaller.Services
{
    public interface ICosmeticService
    {
        /// <inheritdoc cref="CosmeticService.GetFighterCosmetics(BrawlIds)"/>
        List<Cosmetic> GetFighterCosmetics(BrawlIds fighterIds);

        /// <inheritdoc cref="CosmeticService.GetStageCosmetics(BrawlIds)"/>
        List<Cosmetic> GetStageCosmetics(BrawlIds stageIds);

        /// <inheritdoc cref="CosmeticService.GetFranchiseIcons()"/>
        CosmeticList GetFranchiseIcons();

        /// <inheritdoc cref="CosmeticService.ImportCosmetics(List{CosmeticDefinition}, CosmeticList, BrawlIds, string)"/>
        void ImportCosmetics(List<CosmeticDefinition> definitions, CosmeticList cosmeticList, BrawlIds ids, string name=null);

        /// <inheritdoc cref="CosmeticService.GetSharesDataGroups(List{Cosmetic})"/>
        List<List<Cosmetic>> GetSharesDataGroups(List<Cosmetic> cosmetics);
    }
    [Export(typeof(ICosmeticService))]
    internal class CosmeticService : ICosmeticService
    {
        // Properties
        private Dictionary<string, string> HDImages { get; set; } = new Dictionary<string, string>();
        private List<ResourceNode> FileCache { get; set; } = new List<ResourceNode>();

        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }
        IColorSmashService _colorSmashService { get; }

        [ImportingConstructor]
        public CosmeticService(ISettingsService settingsService, IFileService fileService, IColorSmashService colorSmashService) 
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _colorSmashService = colorSmashService;
        }

        // Methods

        /// <summary>
        /// Import a texture from an image path
        /// </summary>
        /// <param name="destinationNode">Destination for imported texture node</param>
        /// <param name="imageSource">Path containing image source</param>
        /// <param name="format">Encoding format to use</param>
        /// <param name="size">Dimensions to scale image to</param>
        /// <returns>TEX0Node</returns>
        private TEX0Node ImportTexture(BRRESNode destinationNode, string imageSource, WiiPixelFormat format, ImageSize size)
        {
            var dialog = new TextureConverterDialog();
            dialog.ImageSource = imageSource;
            dialog.InitialFormat = format;
            dialog.Automatic = true;
            dialog.InitialSize = new System.Drawing.Size(size.Width, size.Height);
            dialog.ShowDialog(null, destinationNode);
            var node = dialog.TEX0TextureNode;
            dialog.Dispose();
            return node;
        }

        /// <summary>
        /// Reimport a texture that is already imported
        /// </summary>
        /// <param name="destinationNode">Destination for imported texture node</param>
        /// <param name="cosmetic">Cosmetic to reimport</param>
        /// <param name="format">Encoding format to use</param>
        /// <param name="size">Dimensions to scale image to</param>
        /// <returns>TEX0Node</returns>
        private TEX0Node ReimportTexture(BRRESNode destinationNode, Cosmetic cosmetic, WiiPixelFormat format, ImageSize size)
        {
            var texture = GetTexture(destinationNode, cosmetic.Texture.Name);
            var palette = texture.GetPaletteNode();
            var index = texture.Index;
            var folder = destinationNode.GetFolder<TEX0Node>();
            var name = texture.Name;
            _fileService.SaveImage(cosmetic.Image, $"{_settingsService.AppSettings.TempPath}\\tempNode.png");
            texture.Remove(true);
            palette?.Remove();
            var node = ImportTexture(destinationNode, $"{_settingsService.AppSettings.TempPath}\\tempNode.png", format, size);
            _fileService.DeleteFile($"{_settingsService.AppSettings.TempPath}\\tempNode.png");
            if (node.GetPaletteNode() != null)
                node.GetPaletteNode().Name = name;
            node.Name = name;
            folder.RemoveChild(node);
            folder.InsertChild(node, index);
            return node;
        }

        /// <summary>
        /// Import a model
        /// </summary>
        /// <param name="destinationNode">Destination for model node</param>
        /// <param name="modelSource">Path to model</param>
        /// <returns>MDL0Node</returns>
        private MDL0Node ImportModel(BRRESNode destinationNode, string modelSource)
        {
            var node = new MDL0Node();
            node.Replace(modelSource);
            var folder = destinationNode.GetOrCreateFolder<MDL0Node>();
            folder.AddChild(node);
            return node;
        }

        /// <summary>
        /// Import a texture node
        /// </summary>
        /// <param name="destinationNode">Destination to import texture node</param>
        /// <param name="texture">Texture node to import</param>
        /// <returns>TEX0Node</returns>
        private TEX0Node ImportTexture(BRRESNode destinationNode, TEX0Node texture)
        {
            var newTexture = _fileService.CopyNode(texture) as TEX0Node;
            destinationNode.GetOrCreateFolder<TEX0Node>()?.AddChild(newTexture);
            return newTexture;
        }

        /// <summary>
        /// Get texture node matching what's stored in cosmetic to guarantee accuracy
        /// </summary>
        /// <param name="rootNode">Root node of file to get texture from</param>
        /// <param name="definition">Cosmetic definition for texture</param>
        /// <param name="name">Name of texture</param>
        /// <returns>TEX0Node</returns>
        private TEX0Node GetTexture(ResourceNode rootNode, CosmeticDefinition definition, string name, int id)
        {
            var parentNode = !string.IsNullOrEmpty(definition.InstallLocation.NodePath) ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
            if (parentNode.GetType() == typeof(ARCNode))
            {
                parentNode = parentNode.Children.FirstOrDefault(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == id);
            }
            if (parentNode != null)
            {
                return GetTexture((BRRESNode)parentNode, name);
            }
            return null;
        }

        /// <summary>
        /// Get texture node matching what's stored in cosmetic to guarantee accuracy
        /// </summary>
        /// <param name="parentNode">Parent node of texture to get</param>
        /// <param name="name">Name of texture</param>
        /// <returns>TEX0Node</returns>
        private TEX0Node GetTexture(BRRESNode parentNode, string name)
        {
            var folder = parentNode.GetFolder<TEX0Node>();
            if (folder != null)
            {
                var node = folder.FindChild(name);
                if (node != null)
                    return (TEX0Node)node;
            }
            return null;
        }

        /// <summary>
        /// Import a palette node
        /// </summary>
        /// <param name="destinationNode">Destination to import node to</param>
        /// <param name="palette">Palette node to import</param>
        /// <returns>PLT0Node</returns>
        private PLT0Node ImportPalette(BRRESNode destinationNode, PLT0Node palette)
        {
            var newPalette = _fileService.CopyNode(palette) as PLT0Node;
            destinationNode.GetOrCreateFolder<PLT0Node>()?.AddChild(newPalette);
            return newPalette;
        }

        /// <summary>
        /// Import a model node
        /// </summary>
        /// <param name="destinationNode">Destination to import node to</param>
        /// <param name="model">Model node</param>
        /// <returns>MDL0Node</returns>
        private MDL0Node ImportModel(BRRESNode destinationNode, MDL0Node model)
        {
            var newModel = _fileService.CopyNode(model) as MDL0Node;
            destinationNode.GetOrCreateFolder<MDL0Node>()?.AddChild(newModel);
            return newModel;
        }

        /// <summary>
        /// Import a color sequence node
        /// </summary>
        /// <param name="destinationNode">Destination to import color sequence to</param>
        /// <param name="colorSequence">Color sequence to import</param>
        /// <returns>CLR0Node</returns>
        private CLR0Node ImportColorSequence(BRRESNode destinationNode, CLR0Node colorSequence)
        {
            var newColorSequence = _fileService.CopyNode(colorSequence) as CLR0Node;
            destinationNode.GetOrCreateFolder<CLR0Node>()?.AddChild(newColorSequence);
            return newColorSequence;
        }

        /// <summary>
        /// Create a new PAT0 entry node
        /// </summary>
        /// <param name="destinationNode">Destination to create PAT0 entry</param>
        /// <param name="frameIndex">Frame index to assign PAT0 entry</param>
        /// <param name="texture">Texture to assign PAT0 entry</param>
        /// <param name="palette">Palette to assign PAT0 entry</param>
        /// <returns>PAT0TextureEntryNode</returns>
        private PAT0TextureEntryNode CreatePatEntry(ResourceNode destinationNode, int frameIndex, string texture="", string palette="")
        {
            if (destinationNode != null)
            {
                var node = new PAT0TextureEntryNode();
                destinationNode.AddChild(node);
                node.FrameIndex = frameIndex;
                node.Texture = texture;
                node.Palette = palette;
                return node;
            }
            return null;
        }

        /// <summary>
        /// Remove texture pattern entries based on definition rules
        /// </summary>
        /// <param name="rootNode">Root node of file to remove textures from</param>
        /// <param name="definition">Cosmetic definition for textures</param>
        /// <param name="id">ID associated with cosmetics</param>
        private void RemovePatEntries(ResourceNode rootNode, CosmeticDefinition definition, int id)
        {
            foreach(var patSetting in definition.PatSettings)
            {
                var patTexture = GetPatTextureNode(rootNode, patSetting);
                var patEntries = patTexture.Children.Where(x => CheckIdRange(patSetting, definition, id, Convert.ToInt32(((PAT0TextureEntryNode)x).FrameIndex))).ToList();
                // Remove texture and palette associated with pat entries
                foreach (var patEntry in patEntries)
                {
                    ((PAT0TextureEntryNode)patEntry).GetImage(0);
                    ((PAT0TextureEntryNode)patEntry)._textureNode?.Remove(true);
                }
                // Remove pat entries
                patTexture.Children.RemoveAll(x => patEntries.Contains(x));
                // Update the FrameCount
                var pat0 = patTexture.Parent.Parent;
                if (pat0 != null)
                {
                    pat0.IsDirty = true;
                    ((PAT0Node)pat0).FrameCount = (int)patTexture.Children.Max(x => ((PAT0TextureEntryNode)x).FrameIndex) + patSetting.FramesPerImage;
                }
            }
        }

        /// <summary>
        /// Get texture pattern nodes based on definition rules
        /// </summary>
        /// <param name="rootnode">Root node of file to get texture pattern nodes from</param>
        /// <param name="definition">Cosmetic definition for texture pattern nodes</param>
        /// <returns>List of ResourceNode</returns>
        private ResourceNode GetPatTextureNode(ResourceNode rootnode, PatSettings patSetting)
        {
            if (patSetting != null)
            {
                var pat0 = rootnode.FindChild(patSetting.Path);
                if (pat0 != null)
                {
                    return pat0;
                }
            }
            return null;
        }

        /// <summary>
        /// Remove textures based on definition rules
        /// </summary>
        /// <param name="parentNode">Parent node to remove textures from</param>
        /// <param name="definition">Cosmetic definition of textures</param>
        /// <param name="id">ID associated with textures</param>
        /// <param name="restrictRange">Whether to restrict range of textures based on ID</param>
        private void RemoveTextures(BRRESNode parentNode, CosmeticDefinition definition, int id, bool restrictRange)
        {
            var folder = parentNode.GetFolder<TEX0Node>();
            if (folder != null)
            {
                // Remove all HD textures
                foreach(var node in folder.Children.Where(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix)))
                {
                    var texNode = (TEX0Node)node;
                    var name = texNode.DolphinTextureName;
                    if (_settingsService.BuildSettings.HDTextures)
                    {
                        var deleteFile = HDImages.FirstOrDefault(x => x.Key == name);
                        _fileService.DeleteFile(deleteFile.Value);
                        HDImages.Remove(name);
                    }
                };
                folder.Children.RemoveAll(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix));
            }
            RemovePalettes(parentNode, definition, id, restrictRange);
        }

        /// <summary>
        /// Remove palettes based on definition rules
        /// </summary>
        /// <param name="parentNode">Parent node to remove palettes from</param>
        /// <param name="definition">Cosmetic definition associated with palettes</param>
        /// <param name="id">ID associated with palettes</param>
        /// <param name="restrictRange">Whether to restrict range of palettes based on ID</param>
        private void RemovePalettes(BRRESNode parentNode, CosmeticDefinition definition, int id, bool restrictRange)
        {
            var folder = parentNode.GetFolder<PLT0Node>();
            if (folder != null)
            {
                folder.Children.RemoveAll(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix));
            }
        }

        /// <summary>
        /// Remove models based on definition rules
        /// </summary>
        /// <param name="parentNode">Parent node to remove models from</param>
        /// <param name="definition">Cosmetic definition for models</param>
        /// <param name="id">ID associated with models</param>
        /// <param name="restrictRange">Whether to restrict range of models based on ID</param>
        private void RemoveModels(BRRESNode parentNode, CosmeticDefinition definition, int id, bool restrictRange)
        {
            var folder = parentNode.GetFolder<MDL0Node>();
            if (folder != null)
            {
                folder.Children.RemoveAll(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix));
            }
            RemoveColorSequences(parentNode, definition, id, restrictRange);
        }

        /// <summary>
        /// Remove color sequences based on definition rules
        /// </summary>
        /// <param name="parentNode">Parent node to remove color sequences from</param>
        /// <param name="definition">Cosmetic definition for color sequences</param>
        /// <param name="id">ID associated with color sequences</param>
        /// <param name="restrictRange">Whether to restrict range of color sequences based on ID</param>
        private void RemoveColorSequences(BRRESNode parentNode, CosmeticDefinition definition, int id, bool restrictRange)
        {
            var folder = parentNode.GetFolder<CLR0Node>();
            if (folder != null)
            {
                folder.Children.RemoveAll(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix));
            }
        }

        /// <summary>
        /// Import a single cosmetic based on definition rules
        /// </summary>
        /// <param name="definition">Cosmetic definition for cosmetic</param>
        /// <param name="cosmetic">Cosmetic to import</param>
        /// <param name="id">ID associated with cosmetic</param>
        /// <param name="rootNode">Root node of file to import cosmetic to</param>
        /// <returns></returns>
        private Cosmetic ImportCosmetic(CosmeticDefinition definition, Cosmetic cosmetic, int id, ResourceNode rootNode)
        {
            var node = definition.InstallLocation.NodePath != "" ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
            if (node.GetType() == typeof(ARCNode))
            {
                node = node.Children.FirstOrDefault(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == id);
            }
            var parentNode = (BRRESNode)node;
            id = definition.Offset + id;
            // If we have a texture node of the same properties, import that
            if (cosmetic.Texture != null && (cosmetic.Texture.SharesData ||
                (cosmetic.Texture.Width == definition.Size.Width && cosmetic.Texture.Height == definition.Size.Height
                && cosmetic.Texture.Format == definition.Format
                && !(cosmetic.Texture.SharesData && (definition.FirstOnly || definition.SeparateFiles)))))
            {
                cosmetic.Texture.Name = $"{definition.Prefix}.{FormatCosmeticId(definition, id, cosmetic)}";
                var texture = ImportTexture(parentNode, cosmetic.Texture);
                if (cosmetic.Palette != null)
                {
                    cosmetic.Palette.Name = texture.Name;
                    var palette = ImportPalette(parentNode, cosmetic.Palette);
                }
            }
            // If we have an image from filesystem, import that
            else if (cosmetic.ImagePath != "")
            {
                var texture = ImportTexture(parentNode, cosmetic.ImagePath, definition.Format, definition.Size ?? new ImageSize(64, 64));
                texture.Name = $"{definition.Prefix}.{FormatCosmeticId(definition, id, cosmetic)}";
                cosmetic.Texture = (TEX0Node)_fileService.CopyNode(texture);
                cosmetic.Palette = texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(texture.GetPaletteNode()) : null;
            }
            // TODO: Verify this actually works
            // If we should only import one cosmetic and it's a color smashed texture, reimport
            else if (cosmetic.Texture?.SharesData == true && (definition.FirstOnly || definition.SeparateFiles))
            {
                var texture = ReimportTexture(parentNode, cosmetic, definition.Format, definition.Size ?? new ImageSize(64, 64));
                texture.Name = $"{definition.Prefix}.{FormatCosmeticId(definition, id, cosmetic)}";
                cosmetic.Texture = (TEX0Node)_fileService.CopyNode(texture);
                cosmetic.Palette = texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(texture.GetPaletteNode()) : null;
            }
            // Create pat entry
            if (definition.PatSettings != null)
            {
                foreach(var patSetting in definition.PatSettings)
                {
                    var path = patSetting.Path;
                    node = rootNode.FindChild(path);
                    if (node != null)
                    {
                        CreatePatEntry(node, GetCosmeticId(definition, id, cosmetic), cosmetic?.Texture?.Name, cosmetic?.Palette?.Name);
                    }
                }
            }
            // If we have a model node, import that
            if (cosmetic.Model != null && definition.ModelPath != null)
            {
                node = definition.ModelPath != "" ? rootNode.FindChild(definition.ModelPath) : rootNode;
                parentNode = (BRRESNode)node;
                var model = ImportModel(parentNode, cosmetic.Model);
                model.Name = $"{definition.Prefix}{FormatCosmeticId(definition, id, cosmetic)}_TopN";
                var bone = model.FindBoneByIndex(0);
                if (bone != null)
                    bone.Name = model.Name;
                if (cosmetic.ColorSequence != null)
                {
                    var colorSequence = ImportColorSequence(parentNode, cosmetic.ColorSequence);
                    colorSequence.Name = model.Name;
                }
            }
            // Otherwise, import model from filesystem
            else if (cosmetic.ModelPath != "" && definition.ModelPath != null)
            {
                var model = ImportModel(parentNode, cosmetic.ModelPath);
                model.Name = $"{definition.Prefix}{FormatCosmeticId(definition, id, cosmetic)}_TopN";
                var bone = model.FindBoneByIndex(0);
                if (bone != null)
                    bone.Name = model.Name;
                cosmetic.Model = model;
                cosmetic.ModelPath = "";
                // Generate color sequence
                var folder = parentNode.GetFolder<CLR0Node>();
                if (folder != null)
                {
                    var clr0 = folder.Children.FirstOrDefault(x => x.Name.StartsWith(definition.Prefix));
                    if (clr0 != null)
                    {
                        cosmetic.ColorSequence = (CLR0Node)_fileService.CopyNode(clr0);
                        cosmetic.ColorSequence.Name = model.Name;
                        folder.AddChild(cosmetic.ColorSequence);
                    }
                }
            }
            return cosmetic;
        }

        /// <summary>
        /// Remove cosmetics based on definition rules
        /// </summary>
        /// <param name="rootNode">Root node of file to remove cosmetics from</param>
        /// <param name="definition">Cosmetic definition for cosmetics to remove</param>
        /// <param name="id">ID associated with cosmetics</param>
        private void RemoveCosmetics(ResourceNode rootNode, CosmeticDefinition definition, int id)
        {
            var restrictRange = rootNode.GetType() != typeof(ARCNode);
            if (definition.PatSettings != null)
            {
                RemovePatEntries(rootNode, definition, id);
            }
            // If the node path is an ARC node, search for a matching BRRES first and don't restrict range for textures
            var parentNode = definition.InstallLocation.NodePath != "" ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
            if (rootNode.GetType() == typeof(ARCNode))
            {
                parentNode = parentNode.Children.FirstOrDefault(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == id);
            }
            if (parentNode != null)
            {
                RemoveTextures((BRRESNode)parentNode, definition, id, !definition.InstallLocation.FilePath.EndsWith("\\") && restrictRange);
                RemoveModels((BRRESNode)parentNode, definition, id, !definition.InstallLocation.FilePath.EndsWith("\\") && restrictRange);
            }
        }

        /// <summary>
        /// Remove cosmetics that are in separate files
        /// </summary>
        /// <param name="definition">Cosmetic definition associated with cosmetics to remove</param>
        /// <param name="id">ID associated with cosmetics</param>
        private void RemoveCosmetics(CosmeticDefinition definition, int id)
        {
            var files = GetCosmeticPaths(definition, id);
            // This is just to remove HD textures
            foreach(var file in files)
            {
                var rootNode = _fileService.OpenFile(file);
                if (rootNode != null)
                {
                    RemoveTextures((BRRESNode)rootNode, definition, id, false);
                }
                _fileService.CloseFile(rootNode);
            }
            files.ForEach(x => _fileService.DeleteFile(x));
        }

        /// <summary>
        /// Get color smash groups from a list of cosmetics
        /// </summary>
        /// <param name="cosmetics">Cosmetics to get color smash groups from</param>
        /// <returns>List of color smash groups</returns>
        private List<List<Cosmetic>> GetColorSmashGroups(List<Cosmetic> cosmetics)
        {
            var colorSmashGroups = new List<List<Cosmetic>>();
            var currentGroup = new List<Cosmetic>();
            foreach (var cosmetic in cosmetics)
            {
                currentGroup.Add(cosmetic);
                if (cosmetic.SharesData == false && currentGroup.Count > 1)
                {
                    colorSmashGroups.Add(currentGroup);
                    currentGroup = new List<Cosmetic>();
                }
                else if (cosmetic.SharesData == false && currentGroup.Count <= 1)
                    currentGroup = new List<Cosmetic>();
            }
            return colorSmashGroups;
        }

        /// <summary>
        /// Get groups that share data from a list of cosmetics
        /// </summary>
        /// <param name="cosmetics">Cosmetics to get groups from</param>
        /// <returns>List of shared data groups</returns>
        public List<List<Cosmetic>> GetSharesDataGroups(List<Cosmetic> cosmetics)
        {
            var sharesDataGroups = new List<List<Cosmetic>>();
            var currentGroup = new List<Cosmetic>();
            foreach (var cosmetic in cosmetics)
            {
                currentGroup.Add(cosmetic);
                if (cosmetic.SharesData == false)
                {
                    sharesDataGroups.Add(currentGroup);
                    currentGroup = new List<Cosmetic>();
                }
            }
            return sharesDataGroups;
        }

        /// <summary>
        /// Color smash a list of cosmetics in a BRRES
        /// </summary>
        /// <param name="cosmetics">Cosmetics to color smash</param>
        /// <param name="rootNode">Root node of file to color smash cosmetics in</param>
        /// <param name="definition">Cosmetic definition for cosmetics</param>
        private void ColorSmashCosmetics(List<Cosmetic> cosmetics, ResourceNode rootNode, CosmeticDefinition definition, int id)
        {
            var node = definition.InstallLocation.NodePath != "" ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
            if (node.GetType() == typeof(ARCNode))
            {
                node = node.Children.FirstOrDefault(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == id);
            }
            if (node != null)
            {
                var bres = (BRRESNode)node;
                // Flip SharesData to false for all that need updating
                var sharesDataList = cosmetics.Where(x => x.ColorSmashChanged && x.SharesData == false).ToList();
                foreach(var cosmetic in sharesDataList)
                {
                    cosmetic.Texture = ReimportTexture(bres, cosmetic, definition.Format, definition.Size ?? new ImageSize(64, 64));
                }
                // Get color smash groups
                var colorSmashGroups = GetColorSmashGroups(cosmetics);
                var changeGroups = colorSmashGroups.Where(x => x.Any(y => y.ColorSmashChanged)).ToList();
                // Color smash groups
                foreach(var group in changeGroups)
                {
                    _colorSmashService.ColorSmashCosmetics(group, bres);
                }
                foreach(var cosmetic in sharesDataList)
                {
                    var texture = GetTexture(bres, cosmetic.Texture.Name);
                    if (texture != null)
                        cosmetic.Texture = (TEX0Node)_fileService.CopyNode(texture);
                    cosmetic.Palette = texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(texture.GetPaletteNode()) : null;
                }
            }
        }

        /// <summary>
        /// Import an HD texture from a cosmetic
        /// </summary>
        /// <param name="rootNode">Root node of file where texture is located</param>
        /// <param name="definition">Definition of cosmetic</param>
        /// <param name="cosmetic">Cosmetic to import texture from</param>
        /// <param name="name">Name associated with character cosmetic belongs to</param>
        private void ImportHDTexture(ResourceNode rootNode, CosmeticDefinition definition, Cosmetic cosmetic, int id, string name=null)
        {
            // Save HD cosmetic if it exists
            var texture = GetTexture(rootNode, definition, cosmetic.Texture?.Name, id);
            if (_settingsService.BuildSettings.HDTextures && !string.IsNullOrEmpty(cosmetic.HDImagePath) && !string.IsNullOrEmpty(texture?.DolphinTextureName))
            {
                var imagePath = $"{_settingsService.BuildSettings.FilePathSettings.HDTextures}\\{definition.HDImageLocation}";
                if (!string.IsNullOrEmpty(name) && definition.CreateHDTextureFolder == true)
                    imagePath += $"\\{name}";
                if (!Directory.Exists(imagePath))
                    Directory.CreateDirectory(imagePath);
                imagePath += $"\\{texture?.DolphinTextureName}.png";
                _fileService.SaveImage(cosmetic.HDImage, imagePath);
                cosmetic.HDImagePath = imagePath;
                // Cache HD image if it is not cached
                if (!HDImages.AsParallel().Any(x => x.Key == texture?.DolphinTextureName))
                {
                    HDImages.Add(texture?.DolphinTextureName, imagePath);
                }
            }
        }

        // TODO: Upon importing selectable cosmetics, filter out cosmetics with "SelectionOption" of true. Also only import the textures if they have a new ID, otherwise,
        // just update the texture name on the PAT0
        /// <summary>
        /// Import cosmetics with file caching based on definition rules
        /// </summary>
        /// <param name="definitions">Definitions for cosmetics</param>
        /// <param name="cosmeticList">List to import cosmetics from</param>
        /// <param name="ids">IDs associated with cosmetics</param>
        /// <param name="name">Name of character for HD textures</param>
        public void ImportCosmetics(List<CosmeticDefinition> definitions, CosmeticList cosmeticList, BrawlIds ids, string name = null)
        {
            foreach(var definition in definitions)
            {
                ImportCosmetics(definition, cosmeticList, ids.Ids.FirstOrDefault(x => x.Type == definition.IdType)?.Id ?? -1, name);
            }
            // Save and close all files
            foreach (var file in FileCache.ToList())
            {
                _fileService.SaveFile(file);
                FileCache.Remove(file);
                _fileService.CloseFile(file);
            }
        }

        // TODO: Handle compression
        // TODO: Handle selectable cosmetics
        /// <summary>
        /// Import cosmetics based on definition rules
        /// </summary>
        /// <param name="definition">Definition for cosmetics</param>
        /// <param name="cosmeticList">List to import cosmetics from</param>
        /// <param name="id">ID associated with cosmetics</param>
        /// <param name="name">Name of character for HD textures</param>
        private void ImportCosmetics(CosmeticDefinition definition, CosmeticList cosmeticList, int id, string name=null)
        {
            var cosmetics = cosmeticList.Items.Where(x => x.CosmeticType == definition.CosmeticType && x.Style == definition.Style && !x.SelectionOption).ToList();
            var changedCosmetics = cosmeticList.ChangedItems.Where(x => x.CosmeticType == definition.CosmeticType && x.Style == definition.Style && !x.SelectionOption).ToList();
            // If the definition doesn't use separate files, find the files and update them
            if (!definition.SeparateFiles)
            {
                var rootNode = GetCosmeticFile(definition, id);
                if (rootNode != null)
                {
                    // If cosmetics are supposed to use their own IDs, remove each one individually instead of removing all
                    if (!definition.UseIndividualIds)
                    {
                        RemoveCosmetics(rootNode, definition, id);
                    }
                    else
                    {
                        foreach(var cosmetic in changedCosmetics.OrderBy(x => x.InternalIndex))
                        {
                            RemoveCosmetics(rootNode, definition, cosmetic.Id ?? -1);
                        }
                    }
                    foreach (var cosmetic in cosmetics.OrderBy(x => x.InternalIndex))
                    {
                        ImportCosmetic(definition, cosmetic, id, rootNode);
                    }
                    if (!definition.FirstOnly && !definition.SeparateFiles)
                        ColorSmashCosmetics(cosmetics.OrderBy(x => x.InternalIndex).ToList(), rootNode, definition, id);
                    // Save HD cosmetics if they exist
                    if (_settingsService.BuildSettings.HDTextures)
                        foreach(var cosmetic in cosmetics.OrderBy(x => x.InternalIndex))
                        {
                            ImportHDTexture(rootNode, definition, cosmetic, id, name);
                        }
                }
            }
            // If the definition does use separate files, generate new files for each cosmetic
            else
            {
                RemoveCosmetics(definition, id);
                foreach(var cosmetic in cosmetics.OrderBy(x => x.InternalIndex))
                {
                    var rootNode = new BRRESNode();
                    ImportCosmetic(definition, cosmetic, id, rootNode);
                    rootNode._origPath = $"{_settingsService.AppSettings.BuildPath}{definition.InstallLocation.FilePath}" +
                        $"{definition.Prefix}{FormatCosmeticId(definition, id, cosmetic)}.{definition.InstallLocation.FileExtension}";
                    FileCache.Add(rootNode);
                    // Save HD cosmetic if it exists
                    var texture = GetTexture(rootNode, definition, cosmetic.Texture?.Name, id);
                    if (_settingsService.BuildSettings.HDTextures)
                    {
                        ImportHDTexture(rootNode, definition, cosmetic, id, name);
                    }
                }
            }
        }

        /// <summary>
        /// Format cosmetic ID to string based on definition
        /// </summary>
        /// <param name="definition">Definition for cosmetic</param>
        /// <param name="cosmeticId">ID associated with cosmetic</param>
        /// <returns></returns>
        private string FormatCosmeticId(CosmeticDefinition definition, int cosmeticId)
        {
            var id = (cosmeticId * definition.Multiplier).ToString("D" + definition.SuffixDigits);
            return id;
        }

        /// <summary>
        /// Format cosmetic ID to string based on definition
        /// </summary>
        /// <param name="definition">Definition for cosmetic</param>
        /// <param name="cosmeticId">ID associated with cosmetic</param>
        /// <param name="costumeIndex">Costume index associated with cosmetic</param>
        /// <returns></returns>
        private string FormatCosmeticId(CosmeticDefinition definition, int cosmeticId, Cosmetic cosmetic)
        {
            if (definition.UseIndividualIds)
            {
                return cosmetic.Id?.ToString("D" + definition.SuffixDigits);
            }
            var id = ((cosmeticId * definition.Multiplier) + (cosmetic.CostumeIndex ?? 0)).ToString("D" + definition.SuffixDigits);
            return id;
        }

        /// <summary>
        /// Get all files containing cosmetics specified by the cosmetic definition
        /// </summary>
        /// <param name="definition">Cosmetic definition</param>
        /// <param name="id">ID associated with cosmetics</param>
        /// <returns>List of files containing cosmetics</returns>
        private List<string> GetCosmeticPaths(CosmeticDefinition definition, int id)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var paths = new List<string>();
            if (definition.InstallLocation.FilePath.EndsWith("\\"))
            {
                var formattedId = FormatCosmeticId(definition, id);
                var directoryInfo = new DirectoryInfo(buildPath + definition.InstallLocation.FilePath);
                var files = directoryInfo.GetFiles("*." + definition.InstallLocation.FileExtension, SearchOption.TopDirectoryOnly);
                if (definition.SeparateFiles)
                    paths = files.Where(f => f.Name.StartsWith(definition.Prefix) && CheckIdRange(definition, id, f.Name.Replace(f.Extension, ""), definition.Prefix)).Select(f => f.FullName).ToList();
                else
                    paths = files.Where(f => f.Name == definition.Prefix + formattedId + "." + definition.InstallLocation.FileExtension).Select(f => f.FullName).ToList();
            }
            else if (File.Exists(buildPath + definition.InstallLocation.FilePath))
                paths.Add(buildPath + definition.InstallLocation.FilePath);
            return paths;
        }

        /// <summary>
        /// Generate filepath for cosmetic based on definition
        /// </summary>
        /// <param name="definition">Cosmetic definition to use</param>
        /// <param name="id">ID for cosmetic</param>
        /// <returns>Filepath for cosmetic</returns>
        private string BuildCosmeticPath(CosmeticDefinition definition, int id)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            if (definition.InstallLocation.FilePath.EndsWith("\\"))
            {
                var formattedId = FormatCosmeticId(definition, id);
                var fileName = $"{definition.Prefix}{formattedId}.{definition.InstallLocation.FileExtension}";
                return $"{buildPath}\\{definition.InstallLocation.FilePath}{fileName}";
            }
            else
            {
                return $"{buildPath}\\{definition.InstallLocation.FilePath}";
            }
        }

        /// <summary>
        /// Get or create cosmetic file based on definition
        /// </summary>
        /// <param name="definition">Cosmetic definition</param>
        /// <param name="id">ID for cosmetic file</param>
        /// <returns>Root node of file</returns>
        private ResourceNode GetCosmeticFile(CosmeticDefinition definition, int id)
        {
            var cosmeticPath = BuildCosmeticPath(definition, id);
            var node = FileCache.FirstOrDefault(x => x.FilePath == cosmeticPath);
            if (node != null)
            {
                return node;
            }
            node = _fileService.OpenFile(cosmeticPath);
            if (node != null)
            {
                FileCache.Add(node);
                return node;
            }
            if(definition.InstallLocation.FileExtension == "pac")
            {
                var newNode = new ARCNode();
                var bresNode = new BRRESNode();
                bresNode.FileIndex = (short)id;
                newNode.AddChild(bresNode);
                newNode._origPath = cosmeticPath;
                FileCache.Add(newNode);
                return newNode;
            }
            else if (definition.InstallLocation.FileExtension == "brres")
            {
                var newNode = new BRRESNode();
                //_fileService.SaveFileAs(newNode, cosmeticPath);
                newNode._origPath = cosmeticPath;
                FileCache.Add(newNode);
                return newNode;
            }
            return null;
        }

        /// <summary>
        /// Check if ID associated with a cosmetic is within the range specified by the cosmetic definition
        /// </summary>
        /// <param name="definition">Cosmetic definition</param>
        /// <param name="id">ID associated with cosmetic</param>
        /// <param name="name">Name of texture to check ID range on</param>
        /// <param name="prefix">Prefix of cosmetic name</param>
        /// <returns>Whether cosmetic is within the ID range</returns>
        private bool CheckIdRange(CosmeticDefinition definition, int id, string name, string prefix)
        {
            if (!name.StartsWith(prefix))
                return false;
            var suffix = name.Replace(prefix, "").Replace(".", "").Replace("_TopN", "");
            if (suffix != "" && int.TryParse(suffix, out int index))
            {
                index = Convert.ToInt32(suffix);
                return CheckIdRange(definition.IdType, definition.Multiplier, id, index);
            }
            return false;
        }

        /// <summary>
        /// Check if ID associated with a cosmetic is within the range specified by the cosmetic definition
        /// </summary>
        /// <param name="patSettings">Settings for texture pattern nodes</param>
        /// <param name="definition">Cosmetic definition</param>
        /// <param name="id">ID associated with cosmetic</param>
        /// <param name="index">Frame index of PAT0</param>
        /// <returns>Whether cosmetic is within the ID range</returns>
        private bool CheckIdRange(PatSettings patSettings, CosmeticDefinition definition, int id, int index)
        {
            return CheckIdRange(idType: patSettings.IdType ?? definition.IdType, multiplier: patSettings.Multiplier ?? definition.Multiplier, id, index);
        }

        /// <summary>
        /// Check if ID associated with a cosmetic is within range based on multiplier and ID
        /// </summary>
        /// <param name="idType">Type of ID associated with cosmetic</param>
        /// <param name="multiplier">Multiplier used for IDs</param>
        /// <param name="id">ID associated with cosmetic</param>
        /// <param name="index">Number to check is within range</param>
        /// <returns>Whether cosmetic is within the ID range</returns>
        private bool CheckIdRange(IdType idType, int multiplier, int id, int index)
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

        /// <summary>
        /// Get costume index from node name
        /// </summary>
        /// <param name="node">Texture to get costume index from</param>
        /// <param name="definition">Cosmetic definition</param>
        /// <param name="id">ID associated with cosmetic</param>
        /// <returns>Costume index of cosmetic</returns>
        private int GetCostumeIndex(TEX0Node node, CosmeticDefinition definition, int id)
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

        /// <summary>
        /// Calculate costume index from parameters
        /// </summary>
        /// <param name="index">Full ID (with costume index) of texture</param>
        /// <param name="multiplier">Multiplier used for IDs</param>
        /// <param name="id">ID associated with texture</param>
        /// <returns></returns>
        private int GetCostumeIndex(int index, int multiplier, int id)
        {
            if (multiplier <= 1)
                return 0;
            index = index - (id * multiplier);
            return index;
        }

        /// <summary>
        /// Get ID from cosmetic name string
        /// </summary>
        /// <param name="name">Name of cosmetic node</param>
        /// <param name="definition">Cosmetic definition</param>
        /// <returns>ID of cosmetic</returns>
        private int? GetCosmeticId(string name, CosmeticDefinition definition)
        {
            if (name != null && name.StartsWith(definition.Prefix))
            {
                var success = int.TryParse(name.Replace(definition.Prefix, "").Replace(".", "").Replace("_TopN",""), System.Globalization.NumberStyles.Integer, null, out int id);
                if (success)
                    return id;
            }
            return null;
        }

        /// <summary>
        /// Get ID from cosmetic definition
        /// </summary>
        /// <param name="definition">Cosmetic definition</param>
        /// <param name="cosmeticId">ID associated with cosmetic</param>
        /// <param name="costumeIndex">Costume index for cosmetic</param>
        /// <returns>Full ID of cosmetic</returns>
        private int GetCosmeticId(CosmeticDefinition definition, int cosmeticId, Cosmetic cosmetic)
        {
            if (definition.UseIndividualIds)
            {
                cosmeticId = cosmetic.Id ?? 0;
            }
            var id = (cosmeticId * definition.Multiplier) + (cosmetic?.CostumeIndex ?? 0);
            return id;
        }

        /// <summary>
        /// Get textures associated with provided cosmetic definition and IDs
        /// </summary>
        /// <param name="definition">Cosmetic definition</param>
        /// <param name="node">Root node of file to retrieve textures from</param>
        /// <param name="brawlIds">IDs to use for getting cosmetics</param>
        /// <param name="restrictRange">Whether to restrict the range of cosmetics based on ID</param>
        /// <returns>List of textures</returns>
        private List<CosmeticTexture> GetTextures(CosmeticDefinition definition, ResourceNode node, BrawlIds brawlIds, bool restrictRange)
        {
            // Try to get all textures for cosmetic definition
            var nodes = new List<CosmeticTexture>();
            var id = brawlIds.GetIdOfType(definition.IdType) + definition.Offset;
            // If the definition contains PatSettings, check the PAT0 first
            if (definition.PatSettings != null && definition.PatSettings.Count > 0)
            {
                var patSettings = definition.PatSettings.FirstOrDefault();
                var pat = node.FindChild(patSettings.Path);
                if (pat != null)
                {
                    var patEntries = new List<ResourceNode>();
                    id = brawlIds.GetIdOfType(patSettings.IdType ?? definition.IdType) + (patSettings.Offset ?? definition.Offset);
                    patEntries = pat.Children.Where(x => !restrictRange || CheckIdRange(patSettings, definition, id, Convert.ToInt32(((PAT0TextureEntryNode)x).FrameIndex))).ToList();
                    foreach (PAT0TextureEntryNode patEntry in patEntries)
                    {
                        // Get the texture from the pat entry
                        patEntry.GetImage(0);
                        if (patEntry._textureNode != null)
                        {
                            nodes.Add(new CosmeticTexture { Texture = patEntry._textureNode, CostumeIndex = GetCostumeIndex(Convert.ToInt32(patEntry.FrameIndex), patSettings.Multiplier ?? definition.Multiplier, id), Id = (int)patEntry.FrameIndex });
                        }
                    }
                    return nodes;
                }
            }
            var start = definition.InstallLocation.NodePath != "" ? node.FindChild(definition.InstallLocation.NodePath) : node;
            // If the node path is an ARC node, search for a matching BRRES first and don't restrict range for textures
            if (start.GetType() == typeof(ARCNode))
            {
                start = start.Children.FirstOrDefault(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == id);
                restrictRange = false;
            }
            if (start != null)
            {
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
            }
            return nodes;
        }

        /// <summary>
        /// Get models associated with provided cosmetic definition and IDs
        /// </summary>
        /// <param name="definition">Cosmetic definition</param>
        /// <param name="node">Root node of file to retrieve cosmetics from</param>
        /// <param name="brawlIds">IDs to use for getting cosmetics</param>
        /// <param name="restrictRange">Whether to restrict range based on ID</param>
        /// <returns>List of models</returns>
        private List<MDL0Node> GetModels(CosmeticDefinition definition, ResourceNode node, BrawlIds brawlIds, bool restrictRange)
        {
            // Try to get all models for cosmetic definition
            var nodes = new List<MDL0Node>();
            var id = brawlIds.GetIdOfType(definition.IdType) + definition.Offset;
            var start = definition.ModelPath != null ? node.FindChild(definition.ModelPath) : node;
            // If the node path is an ARC node, search for a matching BRRES first and don't restrict range for models
            if (start.GetType() == typeof(ARCNode))
            {
                start = start.Children.First(x => x.ResourceFileType == ResourceType.BRES && (!restrictRange || ((BRRESNode)x).FileIndex == id));
                restrictRange = false;
            }
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

        /// <summary>
        /// Loads and caches a list of all HD textures so we can more easily search for texture matches
        /// </summary>
        /// <returns>List of HD textures</returns>
        private Dictionary<string, string> PreloadHDTextures()
        {
            var directories = Directory.GetDirectories(_settingsService.BuildSettings.FilePathSettings.HDTextures, "*", SearchOption.AllDirectories);
            directories = directories.Where(x => !x.Contains(".git")).ToArray();
            directories = directories.Append(_settingsService.BuildSettings.FilePathSettings.HDTextures).ToArray();
            var hdImages = new ConcurrentBag<string>();
            Parallel.ForEach(directories, directory =>
            {
                var images = Directory.GetFiles(directory, "*.png").ToList();
                Parallel.ForEach(images, image =>
                {
                    hdImages.Add(image);
                });
            });
            HDImages = hdImages.ToDictionary(x => Path.GetFileNameWithoutExtension(x), x => x);
            return HDImages;
        }

        /// <summary>
        /// Get HD texture by Dolphin texture name
        /// </summary>
        /// <param name="textureName">Name of HD texture</param>
        /// <returns>HD texture path</returns>
        private string GetHDTexture(string textureName)
        {
            return HDImages.AsParallel().FirstOrDefault(x => x.Key == textureName).Value;
        }

        /// <summary>
        /// Get BitmapImage of HD texture by Dolphin name
        /// </summary>
        /// <param name="fileName">Filename of HD texture</param>
        /// <returns>BitmapImage of texture</returns>
        private BitmapImage GetHDImage(string fileName)
        {
            var file = GetHDTexture(fileName);
            if (!string.IsNullOrEmpty(file))
            {
                var bitmap = new Bitmap(file);
                return bitmap.ToBitmapImage();
            }
            return null;
        }

        /// <summary>
        /// Get cosmetics associated with provided cosmetic definition and IDs
        /// </summary>
        /// <param name="definition">Cosmetic definition</param>
        /// <param name="node">Root node of file to retrieve cosmetics from</param>
        /// <param name="brawlIds">IDs to use for getting cosmetics</param>
        /// <param name="restrictRange">Whether to restrict range based on IDs</param>
        /// <returns>List of cosmetics</returns>
        private List<Cosmetic> GetDefinitionCosmetics(CosmeticDefinition definition, ResourceNode node, BrawlIds brawlIds, bool restrictRange)
        {
            // Get textures for provided definition and IDs
            var cosmetics = new List<Cosmetic>();
            var textures = GetTextures(definition, node, brawlIds, restrictRange);
            foreach(var texture in textures)
            {
                var hdTexture = _settingsService.BuildSettings.HDTextures ? GetHDTexture(texture.Texture?.DolphinTextureName) : "";
                cosmetics.Add(new Cosmetic
                {
                    CosmeticType = definition.CosmeticType,
                    Style = definition.Style,
                    Image = texture.Texture?.GetImage(0).ToBitmapImage(),
                    HDImagePath = hdTexture,
                    HDImage = !string.IsNullOrEmpty(hdTexture) ? GetHDImage(texture.Texture?.DolphinTextureName) : null,
                    Texture = (TEX0Node)_fileService.CopyNode(texture.Texture),
                    Palette = texture.Texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(texture.Texture.GetPaletteNode()) : null,
                    SharesData = texture.Texture.SharesData,
                    InternalIndex = cosmetics.Count(),
                    CostumeIndex = texture.CostumeIndex,
                    Id = texture.Id,
                    // TODO: SelectionOption should be set based on the NEAREST ID
                    SelectionOption = definition.Selectable
                });
            }
            if (definition.ModelPath != null)
            {
                var models = GetModels(definition, node, brawlIds, restrictRange);
                foreach (var model in models)
                {
                    cosmetics.Add(new Cosmetic
                    {
                        CosmeticType = definition.CosmeticType,
                        Style = definition.Style,
                        Model = (MDL0Node)_fileService.CopyNode(model),
                        ColorSequence = model.GetColorSequence(),
                        Id = GetCosmeticId(model.Name, definition),
                        SelectionOption = definition.Selectable
                    });
                }
            }
            // For selectable cosmetics, we only want to get each texture once
            if (definition.Selectable)
            {
                // Get ID of selected cosmetic
                var cosmeticId = brawlIds.Ids.Where(x => x.Type == definition.IdType).FirstOrDefault().Id;
                // Get list of only textures, favoring the one closest to our ID
                cosmetics = cosmetics.GroupBy(x => x.Texture.Name)
                    .Select(g => g.OrderBy(x => x.Id).LastOrDefault(x => x.Id <= cosmeticId) ?? g.First())
                    .ToList();
                // Set nearest option to selected
                cosmetics.OrderBy(x => x.Id).LastOrDefault(x => x.Id <= cosmeticId).SelectionOption = false;
                // Order cosmetics
                cosmetics = cosmetics.OrderByDescending(x => x.Texture.Name).ToList();
            }
            return cosmetics;
        }

        // TODO: When importing a character, franchise icons with a null ID or an ID greater than any existing franchise icon will be installed as new. Any others
        // will overwrite existing ones.

        // TODO: separate RSPs and CSPs so they can be toggled on install

        /// <summary>
        /// Get a list of fighter cosmetics
        /// </summary>
        /// <param name="fighterIds">Fighter IDs to retrieve cosmetics for</param>
        /// <returns>List of cosmetics</returns>
        public List<Cosmetic> GetFighterCosmetics(BrawlIds fighterIds)
        {
            var settings = _settingsService.BuildSettings;
            var definitions = settings.CosmeticSettings.Where(x => x.FighterCosmetic).ToList();
            // Load HD textures in advance
            if (_settingsService.BuildSettings.HDTextures)
                PreloadHDTextures();
            return GetCosmetics(fighterIds, definitions, true);
        }

        /// <summary>
        /// Get a list of stage cosmetics
        /// </summary>
        /// <param name="stageIds">Stage IDs to retrieve cosmetics for</param>
        /// <returns>List of cosmetics</returns>
        public List<Cosmetic> GetStageCosmetics(BrawlIds stageIds)
        {
            var settings = _settingsService.BuildSettings;
            var definitions = settings.CosmeticSettings.Where(x => x.StageCosmetic).ToList();
            // Load HD textures in advance
            if (_settingsService.BuildSettings.HDTextures)
                PreloadHDTextures();
            return GetCosmetics(stageIds, definitions, true);
        }

        /// <summary>
        /// Get a list of all fighter franchise icons
        /// </summary>
        /// <returns>List of franchise icons</returns>
        public CosmeticList GetFranchiseIcons()
        {
            var franchiseIcons = new List<Cosmetic>();
            var settings = _settingsService.BuildSettings;
            var definitions = settings.CosmeticSettings.Where(x => x.CosmeticType == CosmeticType.FranchiseIcon).ToList();
            // Get all franchise icons
            var allIcons = GetCosmetics(new BrawlIds(), definitions, false);
            // Aggregate the models and transparent textures
            foreach(var icon in allIcons.Where(x => x.Texture != null).GroupBy(x => x.Id).Select(x => x.First()).ToList())
            {
                franchiseIcons.Add(new Cosmetic
                {
                    CosmeticType = icon.CosmeticType,
                    Style = icon.Style,
                    Image = icon.Image,
                    HDImage = icon.HDImage,
                    HDImagePath = icon.HDImagePath,
                    Texture = icon.Texture,
                    Palette = icon.Palette,
                    SharesData = icon.SharesData,
                    InternalIndex = icon.InternalIndex,
                    CostumeIndex = icon.CostumeIndex,
                    Id = icon.Id,
                    Model = allIcons.FirstOrDefault(x => x.Id == icon.Id && x.Model != null)?.Model,
                    ColorSequence = allIcons.FirstOrDefault(x => x.Id == icon.Id && x.ColorSequence != null)?.ColorSequence
                });
            }
            var franchiseIconList = new CosmeticList
            {
                Items = franchiseIcons.OrderBy(x => x.InternalIndex).ToList()
            };
            return franchiseIconList;
        }

        /// <summary>
        /// Get a list of cosmetics associated with provided IDs
        /// </summary>
        /// <param name="brawlIds">IDs to retrieve cosmetics for</param>
        /// <param name="definitions">List of cosmetic definitions to retrieve cosmetics for</param>
        /// <param name="restrictRange">Whether to restrict range of cosmetics based on ID</param>
        /// <returns>List of cosmetics</returns>
        private List<Cosmetic> GetCosmetics(BrawlIds brawlIds, List<CosmeticDefinition> definitions, bool restrictRange)
        {
            var cosmetics = new ConcurrentBag<Cosmetic>();
            Parallel.ForEach(definitions.GroupBy(c => new { c.CosmeticType, c.Style }).ToList(), cosmeticGroup =>
            {
                // Check each definition in the group for cosmetics
                // Order them to ensure that cosmetics with multiple definitions favor definitions that have multiple cosmetics
                foreach (var cosmetic in cosmeticGroup.OrderByDescending(x => !x.SeparateFiles).OrderByDescending(x => !x.FirstOnly))
                {
                    var id = brawlIds.GetIdOfType(cosmetic.IdType) + cosmetic.Offset;
                    // Check all paths for the cosmetic definition
                    foreach (var path in GetCosmeticPaths(cosmetic, id))
                    {
                        var rootNode = _fileService.OpenFile(path);
                        if (rootNode != null)
                        {
                            foreach (var foundCosmetics in GetDefinitionCosmetics(cosmetic, rootNode, brawlIds, restrictRange && !cosmetic.InstallLocation.FilePath.EndsWith("\\") && !cosmetic.Selectable))
                                cosmetics.Add(foundCosmetics);
                            _fileService.CloseFile(rootNode);
                        }
                    }
                    // If we found cosmetics, don't bother checking the other definitions in the group - proceed to the next group
                    if (cosmetics.Count > 0)
                        break;
                }
            });
            return cosmetics.ToList();
        }
    }
}
