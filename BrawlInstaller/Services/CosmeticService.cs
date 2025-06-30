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
using BrawlLib.SSBB.Types.Animations;
using BrawlLib.SSBB.Types;
using Newtonsoft.Json;
using BrawlInstaller.StaticClasses;
using BrawlLib.Internal;
using System.Runtime.InteropServices;

namespace BrawlInstaller.Services
{
    public interface ICosmeticService
    {
        /// <inheritdoc cref="CosmeticService.DeleteHDTexture(TEX0Node)"/>
        void DeleteHDTexture(TEX0Node texNode);

        /// <inheritdoc cref="CosmeticService.GetHDImage(string)"/>
        BitmapImage GetHDImage(string fileName);

        /// <inheritdoc cref="CosmeticService.GetFighterCosmetics(BitmapImage, BitmapImage, WiiPixelFormat, ImageSize)"/>
        byte[] ImportBinThumbnail(BitmapImage thumbnail, BitmapImage hdThumbnail, WiiPixelFormat format, ImageSize size);

        /// <inheritdoc cref="CosmeticService.GetFighterCosmetics(BrawlIds)"/>
        List<Cosmetic> GetFighterCosmetics(BrawlIds fighterIds);

        /// <inheritdoc cref="CosmeticService.GetFighterCosmetics(BrawlIds)"/>
        List<Cosmetic> GetTrophyCosmetics(BrawlIds trophyIds);

        /// <inheritdoc cref="CosmeticService.GetStageCosmetics(BrawlIds)"/>
        List<Cosmetic> GetStageCosmetics(BrawlIds stageIds);

        /// <inheritdoc cref="CosmeticService.GetFranchiseIcons(bool)"/>
        CosmeticList GetFranchiseIcons(bool loadHdTextures = false);

        /// <inheritdoc cref="CosmeticService.ImportCosmetics(List{CosmeticDefinition}, CosmeticList, BrawlIds, string)"/>
        void ImportCosmetics(List<CosmeticDefinition> definitions, CosmeticList cosmeticList, BrawlIds ids, string name=null);

        /// <inheritdoc cref="CosmeticService.GetSharesDataGroups(List{Cosmetic})"/>
        List<List<Cosmetic>> GetSharesDataGroups(List<Cosmetic> cosmetics);

        /// <inheritdoc cref="CosmeticService.ExportCosmetics(string, CosmeticList)"/>
        void ExportCosmetics(string path, CosmeticList cosmeticList);

        /// <inheritdoc cref="CosmeticService.LoadCosmetics(string)"/>
        CosmeticList LoadCosmetics(string path);
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
        // TODO: Get rid of this, only in here for debugging
        IDialogService _dialogService { get; }

        [ImportingConstructor]
        public CosmeticService(ISettingsService settingsService, IFileService fileService, IColorSmashService colorSmashService, IDialogService dialogService) 
        {
            _settingsService = settingsService;
            _fileService = fileService;
            _colorSmashService = colorSmashService;
            _dialogService = dialogService;
        }

        // Methods

        /// <summary>
        /// Import a texture from an image path
        /// </summary>
        /// <param name="destinationNode">Destination for imported texture node</param>
        /// <param name="imageSource">Path containing image source</param>
        /// <param name="format">Encoding format to use</param>
        /// <param name="size">Dimensions to scale image to</param>
        /// <param name="paletteCount">Maximum number of colors to use in palette</param>
        /// <returns>TEX0Node</returns>
        private TEX0Node ImportTexture(BRRESNode destinationNode, string imageSource, WiiPixelFormat format, ImageSize size, int paletteCount = 256)
        {
            var dialog = new TextureConverterDialog();
            dialog.ImageSource = imageSource;
            dialog.InitialFormat = format;
            dialog.Automatic = true;
            dialog.MaxPaletteCount = paletteCount;
            // Only set size if it is configured
            if (size.Width != null && size.Height != null)
                dialog.InitialSize = new System.Drawing.Size((int)size.Width, (int)size.Height);
            dialog.ShowDialog(null, destinationNode);
            var node = dialog.TEX0TextureNode;
            // TODO: Get rid of this, for testing only
            if (node.Height == 0 && node.Width == 0)
            {
                _dialogService.ShowMessage($"Texture imported with height 0 and width 0. Please share the contents of this message with Squidgy :)\nImage: {Path.GetFileName(imageSource)}\nFormat: {format}\nSize: {size.Width}x{size.Height}\nDestination: {destinationNode.Name}", "Test Message");
            }
            dialog.Dispose();
            return node;
        }

        /// <summary>
        /// Import a texture from image data
        /// </summary>
        /// <param name="destinationNode">Destination for imported texture node</param>
        /// <param name="cosmeticImage">Bitmap image data</param>
        /// <param name="format">Encoding format to use</param>
        /// <param name="size">Dimensions to scale image to</param>
        /// <param name="paletteCount">Maximum number of colors to use in palette</param>
        /// <returns>TEX0Node</returns>
        private TEX0Node ImportTexture(BRRESNode destinationNode, BitmapImage cosmeticImage, WiiPixelFormat format, ImageSize size, int paletteCount = 256)
        {
            var path = $"{_settingsService.AppSettings.TempPath}\\tempNode.png";
            _fileService.SaveImage(cosmeticImage, path);
            var node = ImportTexture(destinationNode, $"{_settingsService.AppSettings.TempPath}\\tempNode.png", format, size, paletteCount);
            _fileService.DeleteFile($"{_settingsService.AppSettings.TempPath}\\tempNode.png");
            return node;
        }

        /// <summary>
        /// Import a texture from image data
        /// </summary>
        /// <param name="cosmeticImage">Bitmap image data</param>
        /// <param name="format">Encoding format to use</param>
        /// <param name="size">Dimensions to scale image to</param>
        /// <returns>TEX0Node</returns>
        private TEX0Node ImportTexture(BitmapImage cosmeticImage, WiiPixelFormat format, ImageSize size)
        {
            var newBrres = new BRRESNode();
            var node = ImportTexture(newBrres, cosmeticImage, format, size);
            return node;
        }

        /// <summary>
        /// Import stage bin thumbnail
        /// </summary>
        /// <param name="thumbnail">Image to import</param>
        /// <param name="format">Encoding format to use</param>
        /// <param name="size">Dimensions to scale image to</param>
        /// <returns>Byte array</returns>
        public byte[] ImportBinThumbnail(BitmapImage thumbnail, BitmapImage hdThumbnail, WiiPixelFormat format, ImageSize size)
        {
            // Get texture
            var texture = ImportTexture(thumbnail, WiiPixelFormat.RGBA8, new ImageSize(160, 120));
            // Import HD thumbnail
            if (!string.IsNullOrEmpty(_settingsService.AppSettings.HDTextures) && _settingsService.AppSettings.ModifyHDTextures)
            {
                if (HDImages.Count == 0)
                {
                    PreloadHDTextures(); // Load HD textures if they aren't loaded yet
                }
                DeleteHDTexture(texture);
                if (hdThumbnail != null)
                {
                    ImportHDTexture(texture, hdThumbnail, Path.Combine(_settingsService.AppSettings.HDTextures, _settingsService.BuildSettings.FilePathSettings.BinFileHDTexturePath));
                }
            }
            var imageData = GetTextureBytes(texture, WiiPixelFormat.RGBA8, new ImageSize(160, 120));
            return imageData;
        }

        /// <summary>
        /// Get bytes of texture from image data
        /// </summary>
        /// <param name="cosmeticImage">Bitmap image data</param>
        /// <param name="format">Encoding format to use</param>
        /// <param name="size">Dimensions to scale image to</param>
        /// <returns>Byte array</returns>
        private byte[] GetTextureBytes(BitmapImage cosmeticImage, WiiPixelFormat format, ImageSize size)
        {
            var texNode = ImportTexture(cosmeticImage, format, size);
            return GetTextureBytes(texNode, format, size);
        }

        /// <summary>
        /// Get bytes of texture from TEX0
        /// </summary>
        /// <param name="texNode">TEX0 node</param>
        /// <param name="format">Encoding format to use</param>
        /// <param name="size">Dimensions to scale image to</param>
        /// <returns>Byte array</returns>
        private byte[] GetTextureBytes(TEX0Node texNode, WiiPixelFormat format, ImageSize size)
        {
            // Get data
            var length = texNode.WorkingUncompressed.Length.Align(4);
            byte[] data = new byte[length];
            Marshal.Copy(texNode.SourceNode.WorkingUncompressed.Address, data, 0, length);
            return data;
        }

        /// <summary>
        /// Reimport a texture that is already imported
        /// </summary>
        /// <param name="destinationNode">Destination for imported texture node</param>
        /// <param name="cosmetic">Cosmetic to reimport</param>
        /// <param name="format">Encoding format to use</param>
        /// <param name="size">Dimensions to scale image to</param>
        /// <param name="paletteCount">Maximum number of colors to use in palette</param>
        /// <returns>TEX0Node</returns>
        private TEX0Node ReimportTexture(BRRESNode destinationNode, Cosmetic cosmetic, WiiPixelFormat format, ImageSize size, int paletteCount = 256)
        {
            var texture = GetTexture(destinationNode, cosmetic.Texture.Name);
            var palette = GetPalette(destinationNode, texture);
            var index = texture?.Index ?? cosmetic.Texture.Index;
            var folder = destinationNode.GetFolder<TEX0Node>();
            var name = texture?.Name ?? cosmetic.Texture.Name;
            _fileService.SaveImage(cosmetic.Image, $"{_settingsService.AppSettings.TempPath}\\tempNode.png");
            texture?.Remove(true);
            palette?.Remove();
            var node = ImportTexture(destinationNode, $"{_settingsService.AppSettings.TempPath}\\tempNode.png", format, size, paletteCount);
            _fileService.DeleteFile($"{_settingsService.AppSettings.TempPath}\\tempNode.png");
            if (node.GetPaletteNode() != null)
                node.GetPaletteNode().Name = name;
            node.Name = name;
            folder.RemoveChild(node);
            folder.InsertChild(node, index);
            return node;
        }

        /// <summary>
        /// Get palette node that matches a texture node
        /// </summary>
        /// <param name="parentNode">BRRES containing nodes</param>
        /// <param name="textureNode">TEX0 node</param>
        /// <returns>PLT0 node</returns>
        private PLT0Node GetPalette(BRRESNode parentNode, TEX0Node textureNode)
        {
            if (textureNode != null)
            {
                return parentNode.FindChild("Palettes(NW4R)/" + textureNode.Name) as PLT0Node;
            }
            return null;
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
        private TEX0Node GetTexture(ResourceNode rootNode, CosmeticDefinition definition, string name, BrawlIds ids)
        {
            var parentNode = !string.IsNullOrEmpty(definition.InstallLocation.NodePath) ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
            if (parentNode.GetType() == typeof(ARCNode))
            {
                parentNode = parentNode.Children.FirstOrDefault(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == definition.GetDefinitionArchiveId(ids));
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
        private PAT0TextureEntryNode CreatePatEntry(ResourceNode destinationNode, PatSettings patSetting, CosmeticDefinition definition, int frameIndex, string texture="", string palette="")
        {
            if (destinationNode != null)
            {
                var pat0Node = destinationNode?.Parent?.Parent as PAT0Node;
                // Check if node exists before adding
                var node = destinationNode.Children.FirstOrDefault(x => ((PAT0TextureEntryNode)x).FrameIndex == frameIndex) as PAT0TextureEntryNode;
                if (node == null)
                {
                    node = new PAT0TextureEntryNode();
                    destinationNode.AddChild(node);
                }
                node.FrameIndex = frameIndex;
                node.Texture = texture;
                node.Palette = palette;
                // Add terminator frames for definitions that use them
                if (patSetting.AddTerminatorFrame)
                {
                    // Only add if the next frame isn't already used
                    var nodeMatch = destinationNode.Children.FirstOrDefault(x => ((PAT0TextureEntryNode)x).FrameIndex == frameIndex + 1);
                    if (nodeMatch == null)
                    {
                        var terminator = new PAT0TextureEntryNode();
                        destinationNode.AddChild(terminator);
                        terminator.FrameIndex = frameIndex + 1;
                        terminator.Texture = GetTextureName(definition, 0);
                        terminator.Palette = terminator.Texture;
                    }
                }
                pat0Node.FrameCount = (int)pat0Node.Children.SelectMany(x => x.Children).SelectMany(x => x.Children).Max(x => ((PAT0TextureEntryNode)x).FrameIndex + patSetting.FramesPerImage);
                return node;
            }
            return null;
        }

        /// <summary>
        /// Remove texture pattern entries based on definition rules
        /// </summary>
        /// <param name="rootNode">Root node of file to remove textures from</param>
        /// <param name="definition">Cosmetic definition for textures</param>
        /// <param name="ids">IDs associated with cosmetics</param>
        /// <param name="ids">List of altered cosmetics that may contain IDs to remove</param>
        private void RemovePatEntries(ResourceNode rootNode, CosmeticDefinition definition, BrawlIds ids, List<Cosmetic> cosmetics)
        {
            foreach(var patSetting in definition.PatSettings)
            {
                // Remove by individual ID if it is configured as such
                if (definition.UseIndividualIds && !definition.Selectable)
                {
                    foreach (var cosmetic in cosmetics)
                    {
                        var id = cosmetic.Id ?? -1;
                        RemovePatEntries(rootNode, patSetting, definition, id);
                    }
                    
                }
                // Otherwise, we remove PAT0 entries based on the pat setting ID
                // For selectable cosmetics, we remove the PAT0 associated with the definition's ID, but we remove the SELECTED texture
                else
                {
                    var id = patSetting.GetFrameId(definition, ids);
                    RemovePatEntries(rootNode, patSetting, definition, id);
                }
            }
        }

        /// <summary>
        /// Remove texture pattern entries based on definition rules
        /// </summary>
        /// <param name="rootNode">Root node of file to remove textures from</param>
        /// <param name="patSetting">PatSetting definition for pat entries</param>
        /// <param name="definition">Cosmetic definition for textures</param>
        /// <param name="id">ID associated with cosmetics</param>
        private void RemovePatEntries(ResourceNode rootNode, PatSettings patSetting, CosmeticDefinition definition, int id)
        {
            var patTexture = GetPatTextureNode(rootNode, patSetting);
            if (patTexture != null)
            {
                var patEntries = patTexture.Children.Where(x => CheckIdRange(patSetting, definition, id, Convert.ToInt32(((PAT0TextureEntryNode)x).FrameIndex))).ToList();
                // Only remove textures if it's not selectable - we'll remove those by the selected ID instead of the definition ID
                if (!definition.Selectable)
                {
                    // Remove texture and palette associated with pat entries
                    foreach (var patEntry in patEntries)
                    {
                        ((PAT0TextureEntryNode)patEntry).GetImage(0);
                        if (((PAT0TextureEntryNode)patEntry)._textureNode != null)
                        {
                            DeleteHDTexture(((PAT0TextureEntryNode)patEntry)._textureNode);
                            ((PAT0TextureEntryNode)patEntry)._textureNode?.Remove(true);
                        }
                    }
                }
                // Only remove if we shouldn't use terminator frames, otherwise it'll end up using the previous frame's stuff
                if (!patSetting.AddTerminatorFrame)
                {
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
                // Otherwise blank out the texture and palette
                foreach (PAT0TextureEntryNode patEntry in patEntries)
                {
                    patEntry.Texture = "Placeholder";
                    patEntry.Palette = string.Empty;
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
        /// Delete HD texture associated with a TEX0
        /// </summary>
        /// <param name="texNode">TEX0 node to delete HD texture for</param>
        public void DeleteHDTexture(TEX0Node texNode)
        {
            var name = texNode.DolphinTextureName;
            if (_settingsService.AppSettings.ModifyHDTextures)
            {
                if (HDImages.Count == 0)
                {
                    PreloadHDTextures(); // Load HD textures if they aren't loaded yet
                }
                var deleteFiles = HDImages.Where(x => x.Key == name);
                foreach (var deleteFile in deleteFiles)
                {
                    _fileService.DeleteFile(deleteFile.Value);
                }
                HDImages.Remove(name);
            }
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
                var toRemove = new List<TEX0Node>();
                // Remove all HD textures
                foreach (var node in folder.Children.Where(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix)))
                {
                    var texNode = (TEX0Node)node;
                    DeleteHDTexture(texNode);
                    toRemove.Add(texNode);
                };
                // Remove regular textures
                foreach(var tex in toRemove)
                {
                    // Update names where texture is getting removed, in case they are named wrong
                    // This way when it gets reimported, they'll have the correct name
                    foreach(var patSetting in definition.PatSettings)
                    {
                        var patTexture = GetPatTextureNode(parentNode.RootNode, patSetting);
                        if (patTexture != null)
                        {
                            foreach (PAT0TextureEntryNode child in patTexture.Children)
                            {
                                child.GetImage(0);
                                if (child._textureNode == tex)
                                {
                                    child.Texture = GetTextureName(definition, id);
                                    child.Palette = child.Texture;
                                }
                            }
                        }
                    }
                    folder.RemoveChild(tex);
                }
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
                var toRemove = folder.Children.Where(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix)).ToList();
                foreach (var node in toRemove)
                {
                    folder.RemoveChild(node);
                }
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
                var toRemove = folder.Children.Where(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix)).ToList();
                foreach (var node in toRemove)
                {
                    folder.RemoveChild(node);
                }
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
                var toRemove = folder.Children.Where(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix)).ToList();
                foreach (var node in toRemove)
                {
                    folder.RemoveChild(node);
                }
            }
        }

        /// <summary>
        /// Get name for texture node from cosmetic
        /// </summary>
        /// <param name="definition">Definition to use</param>
        /// <param name="id">ID associated with cosmetic</param>
        /// <param name="cosmetic">Cosmetic</param>
        /// <returns>Texture node name</returns>
        private string GetTextureName(CosmeticDefinition definition, int id, Cosmetic cosmetic)
        {
            return $"{definition.Prefix}.{FormatCosmeticId(definition, id, cosmetic)}";
        }

        /// <summary>
        /// Get name for texture node from ID
        /// </summary>
        /// <param name="definition">Definition to use</param>
        /// <param name="id">ID associated with cosmetic</param>
        /// <returns>Texture node name</returns>
        private string GetTextureName(CosmeticDefinition definition, int id)
        {
            return $"{definition.Prefix}.{FormatCosmeticId(definition, id)}";
        }

        /// <summary>
        /// Get name for model node from cosmetic
        /// </summary>
        /// <param name="definition">Definition to use</param>
        /// <param name="id">ID associated with cosmetic</param>
        /// <param name="cosmetic">Cosmetic</param>
        /// <returns>Model node name</returns>
        private string GetModelName(CosmeticDefinition definition, int id, Cosmetic cosmetic)
        {
            return $"{definition.Prefix}{FormatCosmeticId(definition, id, cosmetic)}_TopN";
        }

        /// <summary>
        /// Get file name for cosmetics with separate files
        /// </summary>
        /// <param name="definition">Definition to use</param>
        /// <param name="ids">IDs associated with cosmetic</param>
        /// <param name="cosmetic">Cosmetic</param>
        /// <returns>Name of file for cosmetic</returns>
        private string GetFileName(CosmeticDefinition definition, BrawlIds ids, Cosmetic cosmetic)
        {
            var archiveId = definition.GetBaseArchiveId(ids);
            var prefix = !string.IsNullOrEmpty(definition.FilePrefix) ? definition.FilePrefix : definition.Prefix;
            return $"{prefix}{FormatCosmeticId(definition, archiveId, cosmetic)}.{definition.InstallLocation.FileExtension}";
        }

        /// <summary>
        /// Get file name for cosmetics with separate files
        /// </summary>
        /// <param name="definition">Definition to use</param>
        /// <param name="id">ID associated with cosmetic</param>
        /// <returns>Name of file for cosmetic</returns>
        private string GetFileName(CosmeticDefinition definition, int? id)
        {
            var prefix = !string.IsNullOrEmpty(definition.FilePrefix) ? definition.FilePrefix : definition.Prefix;
            return $"{prefix}{FormatCosmeticId(definition, id)}.{definition.InstallLocation.FileExtension}";
        }

        // TODO: Use for models too?
        /// <summary>
        /// Return the first available cosmetic ID
        /// </summary>
        /// <param name="definition">Definition of cosmetic</param>
        /// <param name="id">ID to compare</param>
        /// <param name="node">Node to check for cosmetics</param>
        /// <returns></returns>
        private int GetUnusedCosmeticId(CosmeticDefinition definition, int id, ResourceNode node, int? costumeId = 0)
        {
            if (costumeId == null)
            {
                costumeId = 0;
            }
            id = (id * definition.Multiplier) + definition.Offset + costumeId.Value;
            var usedIds = GetUsedCosmeticIds(definition, node);
            while (usedIds.Contains(id))
            {
                id = id + definition.Multiplier;
            }
            id = (id - definition.Offset - costumeId.Value) / definition.Multiplier;
            return id;
        }

        /// <summary>
        /// Get cosmetic IDs used by textures in cosmetic definition
        /// </summary>
        /// <param name="definition">Definition to use</param>
        /// <param name="node">Node containing cosmetics to check</param>
        /// <returns>List of used cosmetic IDs</returns>
        private List<int> GetUsedCosmeticIds(CosmeticDefinition definition, ResourceNode node)
        {
            var idList = new List<int>();
            if (node != null)
            {
                if (node.GetType() == typeof(ARCNode))
                {
                    node = node.FindChild(definition.InstallLocation.NodePath);
                }
                var textureFolder = ((BRRESNode)node).GetFolder<TEX0Node>();
                // Check each texture in definition for IDs
                var textures = textureFolder?.Children?.Where(x => x.Name.StartsWith(definition.Prefix));
                if (textures != null)
                {
                    foreach (var texture in textures)
                    {
                        // If the texture ends with a number, add it to the list
                        var result = int.TryParse(texture.Name.Replace(definition.Prefix + ".", ""), out int id);
                        if (result && !idList.Contains(id))
                        {
                            idList.Add(id);
                        }
                    }
                }
            }
            return idList.OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Remove formatting rules from a cosmetic ID
        /// </summary>
        /// <param name="definition">Cosmetic definition to use</param>
        /// <param name="id">ID to remove formatting from</param>
        /// <returns>ID with formatting removed and costume index</returns>
        private (int id, int costumeIndex) UnformatCosmeticId(CosmeticDefinition definition, int id)
        {
            id = id - definition.Offset;
            var costumeIndex = id % definition.Multiplier;
            id = id - costumeIndex;
            id = id / definition.Multiplier;
            return (id, costumeIndex);
        }

        /// <summary>
        /// Renames textures to line up with their PAT animation entries
        /// </summary>
        /// <param name="definition">Cosmetic definition to use</param>
        /// <param name="parentNode">Node containing PAT entries</param>
        private void NormalizeCosmeticIds(CosmeticDefinition definition, PAT0TextureNode parentNode)
        {
            // Only normalize for non-selectable cosmetics
            if (!definition.UseIndividualIds && !definition.Selectable)
            {
                var textureDict = new Dictionary<string, string>();
                // Give each cosmetic a temp name to avoid issues with palettes
                var i = 0;
                foreach (PAT0TextureEntryNode node in parentNode.Children)
                {
                    node.GetImage(0);
                    var texture = node._textureNode;
                    // If the texture doesn't exist or we've already renamed the texture, skip it
                    if (texture != null && !textureDict.ContainsKey(node.Texture))
                    {
                        // If the texture doesn't end with a number, it's a special case and should be skipped
                        if (!int.TryParse(texture.Name.Replace(definition.Prefix + ".", ""), out int result))
                        {
                            i++;
                            continue;
                        }
                        var palette = node._paletteNode;
                        texture.Name = $"TEMPNAME{i:D4}";
                        // Mark the texture as already renamed
                        textureDict.Add(node.Texture, texture.Name);
                        node.Texture = texture.Name;
                        node.Palette = texture.Name;
                        if (palette != null)
                        {
                            palette.Name = texture.Name;
                        }
                    }
                    i++;
                }
                var usedIdList = GetUsedCosmeticIds(definition, parentNode.RootNode);
                // Rename all textures to match the PAT0 frame indexes
                foreach (PAT0TextureEntryNode node in parentNode.Children)
                {
                    node.GetImage(0);
                    var texture = node._textureNode;
                    // If texture exists and uses our temp name, rename it
                    if (texture != null && texture.Name.StartsWith("TEMPNAME"))
                    {
                        var palette = node._paletteNode;
                        var id = definition.Offset;
                        // If the frame index is used by a texture that doesn't appear in the PAT0, use the first unused ID
                        if (usedIdList.Contains((int)node.FrameIndex))
                        {
                            while (usedIdList.Contains(id))
                            {
                                id += definition.Multiplier;
                            }
                        }
                        // Otherwise, use frame index
                        else
                        {
                            id = (int)node.FrameIndex;
                        }
                        // Rename texture based on frame index
                        var unformattedId = UnformatCosmeticId(definition, id);
                        texture.Name = $"{definition.Prefix}.{FormatCosmeticId(definition, unformattedId.id, unformattedId.costumeIndex)}";
                        // Mark the texture as already renamed
                        textureDict[textureDict.FirstOrDefault(x => x.Value == node.Texture).Key] = texture.Name;
                        node.Texture = texture.Name;
                        node.Palette = texture.Name;
                        if (palette != null)
                        {
                            palette.Name = texture.Name;
                        }
                    }
                    // Update dictionary with our new texture name
                    if (textureDict.ContainsKey(node.Texture) && textureDict[node.Texture].StartsWith("TEMPNAME"))
                    {
                        textureDict[textureDict.FirstOrDefault(x => x.Value == node.Texture).Key] = texture.Name;
                    }
                    // If dictionary value is already updated, update this entry to use the correct name
                    else if (textureDict.ContainsKey(node.Texture) && !textureDict[node.Texture].StartsWith("TEMPNAME"))
                    {
                        node.Texture = textureDict[node.Texture];
                        node.Palette = node.Texture;
                    }
                }
            }
        }

        /// <summary>
        /// Import a single cosmetic based on definition rules
        /// </summary>
        /// <param name="definition">Cosmetic definition for cosmetic</param>
        /// <param name="cosmetic">Cosmetic to import</param>
        /// <param name="ids">IDs associated with cosmetic</param>
        /// <param name="rootNode">Root node of file to import cosmetic to</param>
        /// <returns></returns>
        private Cosmetic ImportCosmetic(CosmeticDefinition definition, Cosmetic cosmetic, BrawlIds ids, ResourceNode rootNode)
        {
            var node = definition.InstallLocation.NodePath != "" ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
            var textureBaseId = definition.GetTextureBaseId(ids);
            // If location is an ARC node and the BRRES does not exist, generate new BRRES
            if (node.GetType() == typeof(ARCNode))
            {
                var foundNode = node.Children.FirstOrDefault(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == definition.GetDefinitionArchiveId(ids));
                if (foundNode == null)
                {
                    var bres = new BRRESNode();
                    bres.Compression = definition.CompressionType.ToString();
                    bres.FileType = definition.FileType;
                    bres.FileIndex = (short)definition.GetDefinitionArchiveId(ids);
                    node.InsertChild(bres, textureBaseId);
                    node._children = node._children.OrderBy(x => ((ARCEntryNode)x).FileIndex).ToList();
                    bres.UpdateName();
                    node = bres;
                }
                else
                {
                    node = foundNode;
                }
            }
            var parentNode = (BRRESNode)node;
            var textureId = GetUnusedCosmeticId(definition, textureBaseId, node, cosmetic.CostumeIndex);
            if (definition.ImportTextures)
            {
                // If we have a texture node of the same properties, import that
                if (cosmetic.Texture != null && ((cosmetic.Texture.SharesData && !(definition.FirstOnly || definition.SeparateFiles)) ||
                    (cosmetic.Texture.Width == definition.Size.Width && cosmetic.Texture.Height == definition.Size.Height
                    && cosmetic.Texture.Format == definition.Format
                    && !(cosmetic.Texture.SharesData && (definition.FirstOnly || definition.SeparateFiles)))))
                {
                    cosmetic.Texture.Name = GetTextureName(definition, textureId, cosmetic);
                    var texture = ImportTexture(parentNode, cosmetic.Texture);
                    if (cosmetic.Palette != null)
                    {
                        cosmetic.Palette.Name = texture.Name;
                        var palette = ImportPalette(parentNode, cosmetic.Palette);
                    }
                }
                // TODO: Could the below two if statements be consolidated into one? Do we even need the image path?
                // If we have an image from filesystem, import that
                else if (cosmetic.ImagePath != "")
                {
                    var texture = ImportTexture(parentNode, cosmetic.ImagePath, definition.Format, definition.Size ?? new ImageSize(null, null), definition.PaletteCount);
                    texture.Name = GetTextureName(definition, textureId, cosmetic);
                    cosmetic.Texture = (TEX0Node)_fileService.CopyNode(texture);
                    cosmetic.Palette = texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(texture.GetPaletteNode()) : null;
                }
                // If we should only import one cosmetic and it's a color smashed texture, reimport
                else if (cosmetic.Texture?.SharesData == true && (definition.FirstOnly || definition.SeparateFiles))
                {
                    var texture = ReimportTexture(parentNode, cosmetic, definition.Format, definition.Size ?? new ImageSize(null, null), definition.PaletteCount);
                    texture.Name = GetTextureName(definition, textureId, cosmetic);
                    cosmetic.Texture = (TEX0Node)_fileService.CopyNode(texture);
                    cosmetic.Palette = texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(texture.GetPaletteNode()) : null;
                }
                // If we have neither an image nor a texture node matching the definition, but we do *have* a texture node, reimport it
                // This suggests that we got a texture from the cosmetics that didn't match the definition,
                // or we updated the HD texture but didn't change the texture itself, which we should reimport, or it will be lost
                else if (cosmetic.Texture != null)
                {
                    var texture = ImportTexture(parentNode, cosmetic.Image, definition.Format, definition.Size, definition.PaletteCount);
                    texture.Name = GetTextureName(definition, textureId, cosmetic);
                    cosmetic.Texture = (TEX0Node)_fileService.CopyNode(texture);
                    cosmetic.Palette = texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(texture.GetPaletteNode()) : null;
                }
            }
            // Create pat entry
            if (definition.PatSettings.Count > 0)
            {
                foreach(var patSetting in definition.PatSettings)
                {
                    var path = patSetting.Path;
                    node = rootNode.FindChild(path);
                    var frameId = patSetting.GetFrameId(definition, ids);
                    if (node != null)
                    {
                        CreatePatEntry(node, patSetting, definition, GetCosmeticId(definition, frameId, cosmetic, patSetting.Offset ?? definition.Offset), cosmetic?.Texture?.Name ?? "PLACEHOLDER", cosmetic?.Palette?.Name ?? "PLACEHOLDER");
                    }
                }
            }
            // If we have a model node, import that
            if (cosmetic.Model != null && definition.ModelPath != null)
            {
                node = definition.ModelPath != "" ? rootNode.FindChild(definition.ModelPath) : rootNode;
                parentNode = (BRRESNode)node;
                var model = ImportModel(parentNode, cosmetic.Model);
                model.Name = GetModelName(definition, textureBaseId, cosmetic);
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
                model.Name = GetModelName(definition, textureBaseId, cosmetic);
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
        /// <param name="ids">IDs associated with cosmetics</param>
        /// <param name="cosmetics">Cosmetics that may contain IDs to remove</param>
        /// <param name="removeTextures">Whether or not textures should be removed</param>
        private void RemoveCosmetics(ResourceNode rootNode, CosmeticDefinition definition, BrawlIds ids, List<Cosmetic> cosmetics, bool removeTextures = true)
        {
            var restrictRange = true;
            // If the node path is an ARC node, search for a matching BRRES first and don't restrict range for textures
            var parentNode = definition.InstallLocation.NodePath != "" ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
            var originalNode = parentNode;
            var textureBaseId = definition.GetTextureBaseId(ids);
            if (parentNode.GetType() == typeof(ARCNode))
            {
                restrictRange = IsSharedGroupCosmetic(definition);
                parentNode = parentNode.Children.FirstOrDefault(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == definition.GetDefinitionArchiveId(ids));
            }
            if (parentNode != null)
            {
                // Normally, just remove based on definition ID
                if (!definition.UseIndividualIds)
                {
                    RemoveCosmetics(parentNode, definition, textureBaseId, restrictRange, removeTextures);
                }
                // Otherwise, remove by individual ID
                else
                {
                    foreach(var cosmetic in cosmetics)
                    {
                        // Use individual ID unless it's selectable, in which case use SELECTED ID
                        var id = !definition.Selectable ? cosmetic.Id ?? -1 : cosmetic.TextureId ?? -1;
                        RemoveCosmetics(parentNode, definition, id, restrictRange, removeTextures);
                    }
                }
                // If location is an ARC node, remove entire BRRES
                if (originalNode.GetType() == typeof(ARCNode) && !IsSharedGroupCosmetic(definition))
                {
                    originalNode.RemoveChild(parentNode);
                }
            }
        }

        /// <summary>
        /// Remove cosmetics by specific ID
        /// </summary>
        /// <param name="parentNode">Parent node to remove cosmetics from</param>
        /// <param name="definition">Cosmetic definition for cosmetics to remove</param>
        /// <param name="id">ID associated with cosmetics</param>
        /// <param name="restrictRange">Whether to restrict range on which textures are deleted by ID</param>
        /// <param name="removeTextures">Whether or not to remove textures</param>
        private void RemoveCosmetics(ResourceNode parentNode, CosmeticDefinition definition, int id, bool restrictRange, bool removeTextures = true)
        {
            if (removeTextures)
            {
                RemoveTextures((BRRESNode)parentNode, definition, id, (!definition.InstallLocation.FilePath.EndsWith("\\") || IsSharedGroupCosmetic(definition)) && restrictRange);
            }
            RemoveModels((BRRESNode)parentNode, definition, id, (!definition.InstallLocation.FilePath.EndsWith("\\") || IsSharedGroupCosmetic(definition)) && restrictRange);
        }

        /// <summary>
        /// Remove cosmetics that are in separate files
        /// </summary>
        /// <param name="definition">Cosmetic definition associated with cosmetics to remove</param>
        /// <param name="ids">IDs associated with cosmetics</param>
        private void RemoveCosmetics(CosmeticDefinition definition, BrawlIds ids)
        {
            var textureBaseId = definition.GetTextureBaseId(ids);
            var files = GetCosmeticPaths(definition, ids);
            // This is just to remove HD textures
            foreach(var file in files)
            {
                var rootNode = _fileService.OpenFile(file);
                if (rootNode != null)
                {
                    RemoveTextures((BRRESNode)rootNode, definition, textureBaseId, false);
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
        /// <param name="ids">IDs associated with cosmetics</param>
        private void ColorSmashCosmetics(List<Cosmetic> cosmetics, ResourceNode rootNode, CosmeticDefinition definition, BrawlIds ids)
        {
            var textureBaseId = definition.GetTextureBaseId(ids);
            var node = definition.InstallLocation.NodePath != "" ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
            if (node.GetType() == typeof(ARCNode))
            {
                node = node.Children.FirstOrDefault(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == definition.GetDefinitionArchiveId(ids));
            }
            if (node != null)
            {
                var bres = (BRRESNode)node;
                // Flip SharesData to false for all that need updating
                var sharesDataList = cosmetics.Where(x => x.ColorSmashChanged && x.SharesData == false).ToList();
                foreach(var cosmetic in sharesDataList)
                {
                    cosmetic.Texture = ReimportTexture(bres, cosmetic, definition.Format, definition.Size ?? new ImageSize(null, null), definition.PaletteCount);
                }
                // Get color smash groups
                var colorSmashGroups = GetColorSmashGroups(cosmetics);
                var changeGroups = colorSmashGroups.Where(x => x.Any(y => y.ColorSmashChanged)).ToList();
                // Color smash groups
                foreach(var group in changeGroups)
                {
                    _colorSmashService.ColorSmashCosmetics(group, bres, definition.PaletteCount);
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
        /// <param name="ids">IDs associated with texture</param>
        /// <param name="name">Name associated with character cosmetic belongs to</param>
        private void ImportHDTexture(ResourceNode rootNode, CosmeticDefinition definition, Cosmetic cosmetic, BrawlIds ids, string name=null)
        {
            var textureBaseId = definition.GetTextureBaseId(ids);
            // Save HD cosmetic if it exists
            var texture = GetTexture(rootNode, definition, cosmetic.Texture?.Name, ids);
            if (_settingsService.AppSettings.ModifyHDTextures && !string.IsNullOrEmpty(cosmetic.HDImagePath) && !string.IsNullOrEmpty(texture?.DolphinTextureName))
            {
                var imagePath = $"{_settingsService.AppSettings.HDTextures}\\{definition.HDImageLocation}";
                if (!string.IsNullOrEmpty(name) && definition.CreateHDTextureFolder == true)
                    imagePath += $"\\{name}";
                _fileService.CreateDirectory(imagePath);
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

        /// <summary>
        /// Import an HD texture
        /// </summary>
        /// <param name="texture">Texture node to get name from</param>
        /// <param name="hdImage">Image to import</param>
        /// <param name="imagePath">Path to import image to</param>
        private void ImportHDTexture(TEX0Node texture, BitmapImage hdImage, string imagePath)
        {
            if (_settingsService.AppSettings.ModifyHDTextures && !string.IsNullOrEmpty(texture?.DolphinTextureName))
            {
                _fileService.CreateDirectory(imagePath);
                imagePath += $"\\{texture?.DolphinTextureName}.png";
                _fileService.SaveImage(hdImage, imagePath);
                // Cache HD image if it is not cached
                if (!HDImages.AsParallel().Any(x => x.Key == texture?.DolphinTextureName))
                {
                    HDImages.Add(texture?.DolphinTextureName, imagePath);
                }
            }
        }

        /// <summary>
        /// Import cosmetics with file caching based on definition rules
        /// </summary>
        /// <param name="definitions">Definitions for cosmetics</param>
        /// <param name="cosmeticList">List to import cosmetics from</param>
        /// <param name="ids">IDs associated with cosmetics</param>
        /// <param name="name">Name of character for HD textures</param>
        public void ImportCosmetics(List<CosmeticDefinition> definitions, CosmeticList cosmeticList, BrawlIds ids, string name = null)
        {
            ProgressTracker.Start("Updating cosmetics...");
            foreach(var definition in definitions.Where(x => x.Enabled != false).OrderByDescending(x => x.CosmeticType).ThenByDescending(x => x.Style).ThenByDescending(x => x.Size.Width + x.Size.Height))
            {
                ProgressTracker.UpdateCaption($"Updating cosmetics...\n[Type] {definition.CosmeticType.GetDescription()}\n[Style] {definition.Style}");
                ImportCosmetics(definition, cosmeticList, ids, name);
            }
            // Save and close all files
            foreach(var file in FileCache.ToList())
            {
                _fileService.SaveFile(file);
                FileCache.Remove(file);
                _fileService.CloseFile(file);
            };
        }

        /// <summary>
        /// Import cosmetics based on definition rules
        /// </summary>
        /// <param name="definition">Definition for cosmetics</param>
        /// <param name="cosmeticList">List to import cosmetics from</param>
        /// <param name="id">ID associated with cosmetics</param>
        /// <param name="name">Name of character for HD textures</param>
        private void ImportCosmetics(CosmeticDefinition definition, CosmeticList cosmeticList, BrawlIds ids, string name=null)
        {
            var removeTextures = true; // Only remove textures if pat entries aren't found and removed
            var cosmetics = cosmeticList.Items.Where(x => x.CosmeticType == definition.CosmeticType && x.Style == definition.Style && !x.SelectionOption).ToList();
            cosmetics = definition.FirstOnly && cosmetics.FirstOrDefault() != null ? new List<Cosmetic> { cosmetics.OrderBy(x => x.CostumeIndex).FirstOrDefault() } : cosmetics; // If first only, then only grab the first one
            var changedCosmetics = cosmeticList.ChangedItems.Where(x => x.CosmeticType == definition.CosmeticType && x.Style == definition.Style && !x.SelectionOption).ToList();
            // If the definition doesn't use separate files, find the files and update them
            if (!definition.SeparateFiles)
            {
                var rootNode = GetCosmeticFile(definition, ids);
                if (rootNode != null)
                {
                    var cosmeticChanges = changedCosmetics.OrderBy(x => x.InternalIndex).ToList();
                    // Normalize IDs
                    foreach (var patSetting in definition.PatSettings)
                    {
                        if (patSetting.NormalizeTextureIds == true)
                        {
                            var patTexture = GetPatTextureNode(rootNode, patSetting);
                            NormalizeCosmeticIds(definition, (PAT0TextureNode)patTexture);
                        }
                    }
                    // Remove cosmetics that exist currently
                    if (definition.PatSettings.Count > 0)
                    {
                        RemovePatEntries(rootNode, definition, ids, cosmeticChanges);
                        // If we removed the pat entry and it's not selectable, texture will already have been removed
                        removeTextures = definition.PatSettings.Count > 0 && definition.UseIndividualIds && definition.Selectable;
                    }

                    RemoveCosmetics(rootNode, definition, ids, cosmeticChanges, removeTextures);

                    // Import and color smash new cosmetics
                    foreach (var cosmetic in cosmetics.OrderBy(x => x.InternalIndex))
                    {
                        ImportCosmetic(definition, cosmetic, ids, rootNode);
                    }
                    if (!definition.FirstOnly && !definition.SeparateFiles && definition.ImportTextures)
                    {
                        ColorSmashCosmetics(cosmetics.OrderBy(x => x.InternalIndex).ToList(), rootNode, definition, ids);
                    }
                    // Save HD cosmetics if they exist
                    if (_settingsService.AppSettings.ModifyHDTextures && definition.ImportTextures)
                    {
                        foreach (var cosmetic in cosmetics.OrderBy(x => x.InternalIndex))
                        {
                            ImportHDTexture(rootNode, definition, cosmetic, ids, name);
                        }
                    }
                }
            }
            // If the definition does use separate files, generate new files for each cosmetic
            else
            {
                // TODO: Redo this completely, instead of having an exception for this, use some sort of range variable to calculate when new BRRESes should be generated?
                var textureBaseId = definition.GetTextureBaseId(ids);
                RemoveCosmetics(definition, ids);
                foreach(var cosmetic in cosmetics.OrderBy(x => x.InternalIndex))
                {
                    var rootNode = new BRRESNode();
                    ImportCosmetic(definition, cosmetic, ids, rootNode);
                    rootNode._origPath = $"{_settingsService.AppSettings.BuildPath}{definition.InstallLocation.FilePath}" +
                        GetFileName(definition, ids, cosmetic);
                    FileCache.Add(rootNode);
                    // Save HD cosmetic if it exists
                    var texture = GetTexture(rootNode, definition, cosmetic.Texture?.Name, ids);
                    if (_settingsService.AppSettings.ModifyHDTextures && definition.ImportTextures)
                    {
                        ImportHDTexture(rootNode, definition, cosmetic, ids, name);
                    }
                }
            }
        }

        /// <summary>
        /// Format cosmetic ID to string based on definition
        /// </summary>
        /// <param name="definition">Definition for cosmetic</param>
        /// <param name="cosmeticId">ID associated with cosmetic</param>
        /// <returns>Formatted cosmetic ID</returns>
        private string FormatCosmeticId(CosmeticDefinition definition, int? cosmeticId)
        {
            var id = ((cosmeticId * definition.Multiplier) + definition.Offset)?.ToString("D" + definition.SuffixDigits);
            return id;
        }

        /// <summary>
        /// Format cosmetic ID to string based on definition
        /// </summary>
        /// <param name="definition">Definition for cosmetic</param>
        /// <param name="cosmeticId">ID associated with cosmetic</param>
        /// <param name="costumeIndex">Costume index of cosmetic</param>
        /// <returns>Formatted cosmetic ID</returns>
        private string FormatCosmeticId(CosmeticDefinition definition, int? cosmeticId, int costumeIndex)
        {
            var id = ((cosmeticId * definition.Multiplier) + definition.Offset + costumeIndex)?.ToString("D" + definition.SuffixDigits);
            return id;
        }

        /// <summary>
        /// Format cosmetic ID to string based on definition
        /// </summary>
        /// <param name="definition">Definition for cosmetic</param>
        /// <param name="cosmeticId">ID associated with cosmetic</param>
        /// <param name="costumeIndex">Costume index associated with cosmetic</param>
        /// <returns></returns>
        private string FormatCosmeticId(CosmeticDefinition definition, int? cosmeticId, Cosmetic cosmetic)
        {
            if (definition.UseIndividualIds && !definition.Selectable)
            {
                return cosmetic.Id?.ToString("D" + definition.SuffixDigits);
            }
            else if (definition.Selectable)
            {
                return cosmetic.TextureId?.ToString("D" + definition.SuffixDigits);
            }
            var id = ((cosmeticId * definition.Multiplier) + definition.Offset + (cosmetic.CostumeIndex ?? 0))?.ToString("D" + definition.SuffixDigits);
            return id;
        }

        /// <summary>
        /// Get all files containing cosmetics specified by the cosmetic definition
        /// </summary>
        /// <param name="definition">Cosmetic definition</param>
        /// <param name="ids">IDs associated with cosmetics</param>
        /// <returns>List of files containing cosmetics</returns>
        private List<string> GetCosmeticPaths(CosmeticDefinition definition, BrawlIds ids)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            var paths = new List<string>();
            if (definition.InstallLocation.FilePath.EndsWith("\\") && _fileService.DirectoryExists(_settingsService.GetBuildFilePath(definition.InstallLocation.FilePath)))
            {
                var directoryInfo = new DirectoryInfo(Path.Combine(buildPath, definition.InstallLocation.FilePath));
                var files = directoryInfo.GetFiles("*." + definition.InstallLocation.FileExtension, SearchOption.TopDirectoryOnly);
                paths = files.Where(f => definition.ArchiveIsInRange(f.FullName, ids)).Select(f => f.FullName).ToList();
            }
            else if (_fileService.FileExists(buildPath + definition.InstallLocation.FilePath))
                paths.Add(buildPath + definition.InstallLocation.FilePath);
            return paths;
        }

        /// <summary>
        /// Generate filepath for cosmetic based on definition
        /// </summary>
        /// <param name="definition">Cosmetic definition to use</param>
        /// <param name="id">ID for cosmetic</param>
        /// <returns>Filepath for cosmetic</returns>
        private string BuildCosmeticPath(CosmeticDefinition definition, BrawlIds ids)
        {
            var buildPath = _settingsService.AppSettings.BuildPath;
            if (definition.InstallLocation.FilePath.EndsWith("\\"))
            {
                var fileName = GetFileName(definition, definition.GetDefinitionArchiveId(ids));
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
        private ResourceNode GetCosmeticFile(CosmeticDefinition definition, BrawlIds ids)
        {
            var cosmeticPath = BuildCosmeticPath(definition, ids);
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
            if(definition.InstallLocation.FileExtension == "pac" && definition.AlwaysCreateArchive)
            {
                var newNode = new ARCNode();
                var bresNode = new BRRESNode();
                bresNode.Compression = definition.CompressionType.ToString();
                bresNode.FileType = definition.FileType;
                bresNode.FileIndex = (short)definition.GetDefinitionArchiveId(ids);
                bresNode.Parent = newNode;
                bresNode.UpdateName();
                newNode._origPath = cosmeticPath;
                FileCache.Add(newNode);
                return newNode;
            }
            else if (definition.InstallLocation.FileExtension == "brres" && definition.AlwaysCreateArchive)
            {
                var newNode = new BRRESNode();
                newNode.Compression = definition.CompressionType.ToString();
                newNode.FileType = ARCFileType.None;
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
        private bool CheckIdRange(CosmeticDefinition definition, int? id, string name, string prefix)
        {
            if (!name.StartsWith(prefix))
                return false;
            var suffix = name.Replace(prefix, "").Replace(".", "").Replace("_TopN", "");
            if (suffix != "" && int.TryParse(suffix, out int index))
            {
                index = Convert.ToInt32(suffix);
                return CheckIdRange(definition.Multiplier, id, index, definition.Offset, definition.CostumeCosmetic);
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
        private bool CheckIdRange(PatSettings patSettings, CosmeticDefinition definition, int? id, int index)
        {
            return CheckIdRange(patSettings.Multiplier ?? definition.Multiplier, id, index, patSettings.Offset ?? definition.Offset, patSettings.CostumeCosmetic ?? definition.CostumeCosmetic);
        }

        /// <summary>
        /// Check if ID associated with a cosmetic is within range based on multiplier and ID
        /// </summary>
        /// <param name="multiplier">Multiplier used for IDs</param>
        /// <param name="id">ID associated with cosmetic</param>
        /// <param name="index">Number to check is within range</param>
        /// <param name="costumeCosmetic">Whether the cosmetic is for a costume or not</param>
        /// <returns>Whether cosmetic is within the ID range</returns>
        private bool CheckIdRange(int multiplier, int? id, int index, int offset, bool costumeCosmetic = true)
        {
            if (!costumeCosmetic)
                return index == (id * multiplier) + offset;
            var minRange = (id * multiplier) + offset;
            var maxRange = multiplier > 1 ? minRange + multiplier + offset : id + offset;
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
        private int GetCostumeIndex(TEX0Node node, CosmeticDefinition definition, int? id)
        {
            string suffix;
            if (definition.SeparateFiles)
                suffix = node.RootNode.FileName.Replace(definition.Prefix, "").Replace("." + definition.InstallLocation.FileExtension, "");
            else
                suffix = node.Name.Replace(definition.Prefix, "").Replace(".", "");
            var isNumeric = int.TryParse(suffix, out int index);
            if (isNumeric)
            {
                return GetCostumeIndex(index, definition.Multiplier, id, definition.Offset);
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
        private int GetCostumeIndex(int index, int multiplier, int? id, int offset)
        {
            if (multiplier <= 1 || id == null)
                return 0;
            index = index - ((id.Value * multiplier) + offset);
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
        private int GetCosmeticId(CosmeticDefinition definition, int cosmeticId, Cosmetic cosmetic, int offset)
        {
            // If the cosmetic is selectable, always use the original ID, not the selected ID
            if (definition.UseIndividualIds && !definition.Selectable)
            {
                cosmeticId = cosmetic.Id ?? 0;
            }
            var id = (cosmeticId * definition.Multiplier) + offset + (cosmetic?.CostumeIndex ?? 0);
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
            var textureBaseId = definition.GetTextureBaseId(brawlIds);
            var nodes = new List<CosmeticTexture>();
            // If the definition contains PatSettings, check the PAT0 first
            if (definition.PatSettings.Count > 0)
            {
                var patSettings = definition.PatSettings.FirstOrDefault();
                var pat = node.FindChild(patSettings.Path);
                if (pat != null)
                {
                    var patEntries = new List<ResourceNode>();
                    var frameId = patSettings.GetFrameId(definition, brawlIds);
                    patEntries = pat.Children.Where(x => !restrictRange || CheckIdRange(patSettings, definition, frameId, Convert.ToInt32(((PAT0TextureEntryNode)x).FrameIndex))).ToList();
                    foreach (PAT0TextureEntryNode patEntry in patEntries)
                    {
                        // Get the texture from the pat entry
                        patEntry.GetImage(0);
                        if (patEntry._textureNode != null)
                        {
                            nodes.Add(new CosmeticTexture
                            {
                                Texture = patEntry._textureNode,
                                CostumeIndex = GetCostumeIndex(Convert.ToInt32(patEntry.FrameIndex), patSettings.Multiplier ?? definition.Multiplier, frameId, patSettings.Offset ?? definition.Offset),
                                Id = (int)(patEntry.FrameIndex - (patSettings.Offset ?? definition.Offset)) / (patSettings.Multiplier ?? definition.Multiplier),
                                TextureId = GetCosmeticId(patEntry._textureNode?.Name, definition)
                            });
                        }
                    }
                    // If it's a selectable cosmetic, don't return right away, we still need to retrieve the other textures
                    if (!definition.Selectable)
                    {
                        return nodes;
                    }
                } 
            }
            var start = definition.InstallLocation.NodePath != "" ? node.FindChild(definition.InstallLocation.NodePath) : node;
            // If the node path is an ARC node, search for a matching BRRES first and don't restrict range for textures
            if (start.GetType() == typeof(ARCNode))
            {
                start = start.Children.FirstOrDefault(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == definition.GetDefinitionArchiveId(brawlIds));
                restrictRange = IsSharedGroupCosmetic(definition);
            }
            if (start != null)
            {
                var folder = start.FindChild("Textures(NW4R)");
                if (folder != null)
                {
                    foreach (var child in folder.Children)
                    {
                        // Get textures that match definition
                        if (child.GetType() == typeof(TEX0Node) && (definition.SeparateFiles || child.Name.StartsWith(definition.Prefix)) && (!restrictRange || CheckIdRange(definition, textureBaseId, child.Name, definition.Prefix)))
                        {
                            // If it's selectable and not a new texture, don't add it. Also set ID to null for selectable textures
                            if (!definition.Selectable || !nodes.Select(x => x.TextureId).Contains(GetCosmeticId(child.Name, definition)))
                            {
                                var textureId = GetCosmeticId(child.Name, definition);
                                var nodeId = !definition.Selectable ? textureId : null;
                                nodes.Add(new CosmeticTexture { Texture = (TEX0Node)child, CostumeIndex = definition.ArchiveRange != 1 ? GetCostumeIndex((TEX0Node)child, definition, textureBaseId) : definition.GetArchiveFileId(node.FilePath), Id = nodeId, TextureId = textureId });
                            }
                        }
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
            var textureBaseId = definition.GetTextureBaseId(brawlIds);
            // Try to get all models for cosmetic definition
            var nodes = new List<MDL0Node>();
            var start = definition.ModelPath != null ? node.FindChild(definition.ModelPath) : node;
            // If the node path is an ARC node, search for a matching BRRES first and don't restrict range for models
            if (start.GetType() == typeof(ARCNode))
            {
                start = start.Children.First(x => x.ResourceFileType == ResourceType.BRES && (!restrictRange || ((BRRESNode)x).FileIndex == definition.GetDefinitionArchiveId(brawlIds)));
                restrictRange = IsSharedGroupCosmetic(definition);
            }
            var folder = start.FindChild("3DModels(NW4R)");
            if (folder != null)
            {
                foreach (var child in folder.Children)
                {
                    // Get models that match definition
                    if (child.GetType() == typeof(MDL0Node) && child.Name.StartsWith(definition.Prefix) && (!restrictRange || CheckIdRange(definition, textureBaseId, child.Name.Replace("_TopN", ""), definition.Prefix)))
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
            var directories = _fileService.GetDirectories(_settingsService.AppSettings.HDTextures, "*", SearchOption.AllDirectories);
            directories = directories.Where(x => !x.Contains(".git")).ToList();
            directories = directories.Append(_settingsService.AppSettings.HDTextures).ToList();
            var hdImages = new ConcurrentBag<string>();
            Parallel.ForEach(directories, directory =>
            {
                var images = _fileService.GetFiles(directory, "*.png").ToList();
                Parallel.ForEach(images, image =>
                {
                    hdImages.Add(image);
                });
            });
            HDImages = hdImages.GroupBy(i => Path.GetFileNameWithoutExtension(i)).ToDictionary(x => x.Key, x=> x.First());
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
        public BitmapImage GetHDImage(string fileName)
        {
            var file = GetHDTexture(fileName);
            if (!string.IsNullOrEmpty(file))
            {
                //var bitmap = new Bitmap(file);
                //return bitmap.ToBitmapImage();
                return _fileService.LoadImage(file);
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
                var hdTexture = _settingsService.AppSettings.ModifyHDTextures ? GetHDTexture(texture.Texture?.DolphinTextureName) : "";
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
                    TextureId = texture.TextureId,
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
                        ColorSequence = model.GetColorSequence() != null ? (CLR0Node)_fileService.CopyNode(model.GetColorSequence()) : null,
                        Id = GetCosmeticId(model.Name, definition),
                        SelectionOption = definition.Selectable
                    });
                }
            }
            // For selectable cosmetics, we only want to get each texture once
            if (definition.Selectable)
            {
                // Get ID of selected cosmetic
                var textureBaseId = definition.GetTextureBaseId(brawlIds);
                // Get list of only textures, favoring the one closest to our ID
                cosmetics = cosmetics.GroupBy(x => x.Texture.Name)
                    .Select(g => g.OrderBy(x => x.Id).LastOrDefault(x => x.Id <= textureBaseId) ?? g.First())
                    .ToList();
                // Set nearest option to selected
                var selection = cosmetics.Where(x => x.Id != null).OrderBy(x => x.Id).LastOrDefault(x => x.Id <= textureBaseId);
                if (selection != null)
                {
                    selection.SelectionOption = false;
                };
                // Order cosmetics
                cosmetics = cosmetics.OrderByDescending(x => x.Texture.Name).ToList();
            }
            return cosmetics;
        }

        /// <summary>
        /// Determine whether specified definition uses shared groups
        /// </summary>
        /// <param name="definition">Definition to evaluate</param>
        /// <returns>Whether or not definition is for shared group cosmetic</returns>
        private bool IsSharedGroupCosmetic(CosmeticDefinition definition)
        {
            return !definition.CostumeCosmetic;
        }

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
            if (_settingsService.AppSettings.ModifyHDTextures)
                PreloadHDTextures();
            return GetCosmetics(fighterIds, definitions, true);
        }

        /// <summary>
        /// Get a list of trophy cosmetics
        /// </summary>
        /// <param name="trophyIds">Trophy IDs to retrieve cosmetics for</param>
        /// <returns>List of cosmetics</returns>
        public List<Cosmetic> GetTrophyCosmetics(BrawlIds trophyIds)
        {
            var settings = _settingsService.BuildSettings;
            var definitions = settings.CosmeticSettings.Where(x => x.CosmeticType == CosmeticType.TrophyThumbnail || x.CosmeticType == CosmeticType.TrophyStandee).ToList();
            // Load HD textures in advance
            if (_settingsService.AppSettings.ModifyHDTextures)
                PreloadHDTextures();
            return GetCosmetics(trophyIds, definitions, true);
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
            if (_settingsService.AppSettings.ModifyHDTextures)
                PreloadHDTextures();
            return GetCosmetics(stageIds, definitions, true);
        }

        /// <summary>
        /// Get a list of all fighter franchise icons
        /// </summary>
        /// <returns>List of franchise icons</returns>
        public CosmeticList GetFranchiseIcons(bool loadHdTextures = false)
        {
            var franchiseIcons = new List<Cosmetic>();
            var settings = _settingsService.BuildSettings;
            var definitions = settings.CosmeticSettings.Where(x => x.CosmeticType == CosmeticType.FranchiseIcon).ToList();
            // Load HD textures in advance
            if (_settingsService.AppSettings.ModifyHDTextures && loadHdTextures)
                PreloadHDTextures();
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
        // TODO: If node path or pat path can't be found, we should report this to user somehow
        private List<Cosmetic> GetCosmetics(BrawlIds brawlIds, List<CosmeticDefinition> definitions, bool restrictRange)
        {
            var cosmetics = new ConcurrentBag<Cosmetic>();
            //Parallel.ForEach(definitions.Where(x => x.Enabled).GroupBy(c => new { c.CosmeticType, c.Style }).ToList(), cosmeticGroup =>
            foreach(var cosmeticGroup in definitions.Where(x => x.Enabled).GroupBy(c => new { c.CosmeticType, c.Style }).ToList())
            {
                // Check each definition in the group for cosmetics
                // Order them to ensure that cosmetics with multiple definitions favor definitions that have multiple cosmetics, also order by size to ensure highest-quality cosmetic is loaded first
                foreach (var cosmetic in cosmeticGroup.OrderByDescending(x => !x.SeparateFiles).ThenByDescending(x => !x.FirstOnly).ThenByDescending(x => x.Size.Width + x.Size.Height))
                {
                    // Check all paths for the cosmetic definition
                    foreach (var path in GetCosmeticPaths(cosmetic, brawlIds))
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
            }
            return cosmetics.ToList();
        }

        /// <summary>
        /// Export all cosmetics in a list
        /// </summary>
        /// <param name="path">Path to export to</param>
        /// <param name="cosmeticList">List of cosmetics to export</param>
        public void ExportCosmetics(string path, CosmeticList cosmeticList)
        {
            var cosmetics = new List<Cosmetic>();
            var cosmeticsToExport = cosmeticList.Items.Copy();
            var allCosmetics = cosmeticsToExport.ToList();
            // Update inherited items so we don't export cosmetics more than once
            foreach(var inheritedStyle in cosmeticList.InheritedStyles)
            {
                // Remove all cosmetics that inherit from the style
                cosmeticsToExport.RemoveAll(x => x.CosmeticType == inheritedStyle.Key.BaseType && x.Style == inheritedStyle.Key.BaseStyle);
                // Copy over all cosmetics that they inherited from
                cosmeticsToExport.AddRange(allCosmetics.Where(x => x.CosmeticType == inheritedStyle.Key.BaseType && x.Style == inheritedStyle.Value));
            }
            foreach (var group in cosmeticsToExport.Distinct().GroupBy(x => new { x.CosmeticType, x.Style }))
            {
                var index = 0;
                foreach (var cosmetic in group.OrderBy(x => x.InternalIndex).OrderBy(x => x.CostumeIndex))
                {
                    cosmetics.Add(cosmetic);
                    ExportCosmetic(path, cosmetic, index);
                    index++;
                }
            }
            var cosmeticListJson = JsonConvert.SerializeObject(cosmetics, Formatting.Indented);
            _fileService.SaveTextFile($"{path}\\CosmeticList.json", cosmeticListJson);
        }

        /// <summary>
        /// Load all cosmetics from folder
        /// </summary>
        /// <param name="path">Path to load from</param>
        /// <returns>Cosmetic list</returns>
        public CosmeticList LoadCosmetics(string path)
        {
            var cosmeticsJson = _fileService.ReadTextFile($"{path}\\CosmeticList.json");
            var cosmetics = JsonConvert.DeserializeObject<List<Cosmetic>>(cosmeticsJson);
            var newCosmetics = new List<Cosmetic>();
            foreach(var cosmetic in cosmetics)
            {
                var newCosmetic = cosmetic.Copy();
                var index = cosmetics.Where(x => x.CosmeticType == newCosmetic.CosmeticType && x.Style == newCosmetic.Style).ToList().IndexOf(cosmetic);
                var image = _fileService.GetFiles($"{path}\\{newCosmetic.CosmeticType}\\{newCosmetic.Style}\\SD", $"{index:D4}.png").FirstOrDefault();
                if (!string.IsNullOrEmpty(image))
                {
                    var imagePath = Path.GetFullPath(image);
                    newCosmetic.ImagePath = imagePath;
                    newCosmetic.Image = _fileService.LoadImage(imagePath);
                }
                var hdImage = _fileService.GetFiles($"{path}\\{newCosmetic.CosmeticType}\\{newCosmetic.Style}\\HD", $"{index:D4}.png").FirstOrDefault();
                if (!string.IsNullOrEmpty(hdImage))
                {
                    var hdImagePath = Path.GetFullPath(hdImage);
                    newCosmetic.HDImagePath = hdImagePath;
                    newCosmetic.HDImage = _fileService.LoadImage(hdImagePath);
                }
                var model = _fileService.GetFiles($"{path}\\{newCosmetic.CosmeticType}\\{newCosmetic.Style}\\Model", $"{index:D4}.mdl0").FirstOrDefault();
                if (!string.IsNullOrEmpty(model))
                {
                    var modelPath = Path.GetFullPath(model);
                    newCosmetic.ModelPath = modelPath;
                }
                // If there's a higher priority definition (one that should always have a cosmetic), set to use that style
                // This prevents unnecessary cosmetics from being loaded into the editor (e.g. a package only has CSS style,
                // but the build only really wants Result style, we should fill Result style instead of CSS)
                // Packages with both styles will still load both, which should ensure cosmetics that are needed are all loaded
                var priorityDefinition = _settingsService.BuildSettings.CosmeticSettings.FirstOrDefault(x => x.Style != newCosmetic.Style && x.CosmeticType == newCosmetic.CosmeticType && x.AlwaysInheritStyle);
                if (!_settingsService.BuildSettings.CosmeticSettings.Any(x => x.Style == newCosmetic.Style && x.CosmeticType == newCosmetic.CosmeticType && (x.AlwaysInheritStyle || x.Required))
                    && priorityDefinition != null && !cosmetics.Any(x => x.CosmeticType == priorityDefinition.CosmeticType && x.Style == priorityDefinition.Style))
                {
                    newCosmetic.Style = priorityDefinition.Style;
                }
                newCosmetics.Add(newCosmetic);
            }
            var cosmeticList = new CosmeticList
            {
                Items = newCosmetics
            };
            // Inherit missing cosmetics that require it
            foreach(var definition in _settingsService.BuildSettings.CosmeticSettings)
            {
                if (definition.AlwaysInheritStyle && !cosmeticList.Items.Any(x => x.CosmeticType == definition.CosmeticType && x.Style == definition.Style))
                {
                    var inheritedStyle = cosmeticList.Items.FirstOrDefault(x => x.CosmeticType == definition.CosmeticType)?.Style;
                    if (!string.IsNullOrEmpty(inheritedStyle))
                    {
                        cosmeticList.InheritedStyles.Add((definition.CosmeticType, definition.Style), inheritedStyle);
                    }
                }
            }
            cosmeticList.MarkAllChanged();
            // Mark color smashing as changed for all
            foreach(var cosmetic in cosmeticList.Items)
            {
                cosmetic.ColorSmashChanged = true;
            }
            return cosmeticList;
        }

        /// <summary>
        /// Export a single cosmetic
        /// </summary>
        /// <param name="path">Base path to export to</param>
        /// <param name="cosmetic">Cosmetic to export</param>
        /// <param name="index">Index of cosmetic in directory</param>
        /// <param name="colorSmashGroup">Color smash group of cosmetic</param>
        private void ExportCosmetic(string path, Cosmetic cosmetic, int index)
        {
            var name = index.ToString("D4");
            var filePath = Path.Combine(path, cosmetic.CosmeticType.ToString(), cosmetic.Style);
            _fileService.SaveImage(cosmetic.Image, $"{Path.Combine(filePath, "SD", name)}.png");
            _fileService.SaveImage(cosmetic.HDImage, $"{Path.Combine(filePath, "HD", name)}.png");
            if (cosmetic.Model != null)
            {
                _fileService.SaveFileAs(cosmetic.Model, $"{Path.Combine(filePath, "Model", name)}.mdl0");
            }
            else if (!string.IsNullOrEmpty(cosmetic.ModelPath))
            {
                _fileService.CopyFile(cosmetic.ModelPath, $"{Path.Combine(filePath, "Model", name)}.mdl0");
            }
            if (cosmetic.ColorSequence != null)
            {
                _fileService.SaveFileAs(cosmetic.ColorSequence, $"{Path.Combine(filePath, "ColorSequence", name)}.clr0");
            }
            if (cosmetic.GetType() == typeof(FranchiseCosmetic))
            {
                var franchiseCosmetic = (FranchiseCosmetic)cosmetic;
                _fileService.SaveImage(franchiseCosmetic.TransparentImage, $"{Path.Combine(filePath, "Transparent", name)}.png");
            }
        }
    }
}
