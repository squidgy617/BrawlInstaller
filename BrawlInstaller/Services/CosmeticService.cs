﻿using BrawlInstaller.Classes;
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
        List<Cosmetic> GetFighterCosmetics(FighterIds fighterIds);
        List<Cosmetic> GetFranchiseIcons();
        void ImportCosmetics(CosmeticDefinition definition, List<Cosmetic> cosmetics, int id);
    }
    [Export(typeof(ICosmeticService))]
    internal class CosmeticService : ICosmeticService
    {
        // Properties
        private List<string> HDImages { get; set; } = new List<string>();

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

        // Import a texture from an image path
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

        public TEX0Node ReimportTexture(BRRESNode destinationNode, Cosmetic cosmetic, WiiPixelFormat format, System.Drawing.Size size)
        {
            var texture = cosmetic.Texture;
            var palette = cosmetic.Palette;
            var index = texture.Index;
            var folder = destinationNode.GetFolder<TEX0Node>();
            var name = texture.Name;
            cosmetic.Image.Save("tempNode.png");
            texture.Remove(true);
            palette?.Remove();
            var node = ImportTexture(destinationNode, "tempNode.png", format, size);
            node.Name = name;
            folder.RemoveChild(node);
            folder.InsertChild(node, index);
            return node;
        }

        public MDL0Node ImportModel(BRRESNode destinationNode, string modelSource)
        {
            var node = new MDL0Node();
            node.Replace(modelSource);
            var folder = destinationNode.GetOrCreateFolder<MDL0Node>();
            folder.AddChild(node);
            return node;
        }

        // Import a texture node
        public TEX0Node ImportTexture(BRRESNode destinationNode, TEX0Node texture)
        {
            destinationNode.GetOrCreateFolder<TEX0Node>()?.AddChild(texture);
            return texture;
        }

        // Import a palette node
        public PLT0Node ImportPalette(BRRESNode destinationNode, PLT0Node palette)
        {
            destinationNode.GetOrCreateFolder<PLT0Node>()?.AddChild(palette);
            return palette;
        }

        // Import a model node
        public MDL0Node ImportModel(BRRESNode destinationNode, MDL0Node model)
        {
            destinationNode.GetOrCreateFolder<MDL0Node>()?.AddChild(model);
            return model;
        }

        // Import a color sequence
        public CLR0Node ImportColorSequence(BRRESNode destinationNode, CLR0Node colorSequence)
        {
            destinationNode.GetOrCreateFolder<CLR0Node>()?.AddChild(colorSequence);
            return colorSequence;
        }

        // Create a new pat entry node
        public PAT0TextureEntryNode CreatePatEntry(ResourceNode destinationNode, int frameIndex, string texture="", string palette="")
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

        // Remove texture pattern entries based on definition rules
        public void RemovePatEntries(ResourceNode rootNode, CosmeticDefinition definition, int id)
        {
            var patTextures = GetPatTextureNodes(rootNode, definition);
            foreach (var patTexture in patTextures)
            {
                var patEntries = patTexture.Children.Where(x => CheckIdRange(definition.PatSettings, definition, id, Convert.ToInt32(((PAT0TextureEntryNode)x).FrameIndex))).ToList();
                // Remove texture and palette associated with pat entries
                foreach(var patEntry in patEntries)
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
                    ((PAT0Node)pat0).FrameCount = (int)patTexture.Children.Max(x => ((PAT0TextureEntryNode)x).FrameIndex) + definition.PatSettings.FramesPerImage;
                }
            }
        }

        // Get texture pattern nodes based on definition rules
        public List<ResourceNode> GetPatTextureNodes(ResourceNode rootnode, CosmeticDefinition definition)
        {
            var pat0s = new List<ResourceNode>();
            if (definition.PatSettings != null)
            {
                foreach (var path in definition.PatSettings.Paths)
                {
                    var pat0 = rootnode.FindChild(path);
                    if (pat0 != null)
                    {
                        pat0s.Add(pat0);
                    }
                }
            }
            return pat0s;
        }

        // Remove textures based on definition rules
        public void RemoveTextures(BRRESNode parentNode, CosmeticDefinition definition, int id, bool restrictRange)
        {
            var folder = parentNode.GetFolder<TEX0Node>();
            if (folder != null)
            {
                folder.Children.RemoveAll(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix));
            }
            RemovePalettes(parentNode, definition, id, restrictRange);
        }

        // Remove palettes based on definition rules
        public void RemovePalettes(BRRESNode parentNode, CosmeticDefinition definition, int id, bool restrictRange)
        {
            var folder = parentNode.GetFolder<PLT0Node>();
            if (folder != null)
            {
                folder.Children.RemoveAll(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix));
            }
        }

        // Remove models based on definition rules
        public void RemoveModels(BRRESNode parentNode, CosmeticDefinition definition, int id, bool restrictRange)
        {
            var folder = parentNode.GetFolder<MDL0Node>();
            if (folder != null)
            {
                folder.Children.RemoveAll(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix));
            }
            RemoveColorSequences(parentNode, definition, id, restrictRange);
        }

        // Remove color sequences based on definition rules
        public void RemoveColorSequences(BRRESNode parentNode, CosmeticDefinition definition, int id, bool restrictRange)
        {
            var folder = parentNode.GetFolder<CLR0Node>();
            if (folder != null)
            {
                folder.Children.RemoveAll(x => !restrictRange || CheckIdRange(definition, id, x.Name, definition.Prefix));
            }
        }

        // Import a single cosmetic based on definition rules
        public Cosmetic ImportCosmetic(CosmeticDefinition definition, Cosmetic cosmetic, int id, ResourceNode rootNode)
        {
            var node = definition.InstallLocation.NodePath != "" ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
            var parentNode = (BRRESNode)node;
            id = definition.Offset + id;
            // If we have a texture node of the same properties, import that
            if (cosmetic.Texture != null && (cosmetic.Texture.SharesData ||
                (cosmetic.Texture.Width == definition.Size.Value.Width && cosmetic.Texture.Height == definition.Size.Value.Height
                && cosmetic.Texture.Format == definition.Format
                && (!cosmetic.Texture.SharesData && !definition.FirstOnly && !definition.SeparateFiles))))
            {
                var texture = ImportTexture(parentNode, cosmetic.Texture);
                texture.Name = $"{definition.Prefix}.{FormatCosmeticId(definition, id, cosmetic.CostumeIndex)}";
                if (cosmetic.Palette != null)
                {
                    var palette = ImportPalette(parentNode, cosmetic.Palette);
                    palette.Name = texture.Name;
                }
            }
            // If we have an image from filesystem, import that
            else if (cosmetic.ImagePath != "")
            {
                var texture = ImportTexture(parentNode, cosmetic.ImagePath, definition.Format, definition.Size ?? new System.Drawing.Size(64, 64));
                texture.Name = $"{definition.Prefix}.{FormatCosmeticId(definition, id, cosmetic.CostumeIndex)}";
                cosmetic.Texture = (TEX0Node)_fileService.CopyNode(texture);
                cosmetic.Palette = texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(texture.GetPaletteNode()) : null;
                // We do this so the referenced (copied) texture is what's stored in the BRRES, instead of the original
                texture.Remove(true);
                ImportTexture(parentNode, cosmetic.Texture);
                ImportPalette(parentNode, cosmetic.Palette);
            }
            // TODO: Verify this actually works
            // If we should only import one cosmetic and it's a color smashed texture, reimport
            else if (cosmetic.Texture.SharesData && (definition.FirstOnly || definition.SeparateFiles))
            {
                var texture = ReimportTexture(parentNode, cosmetic, definition.Format, definition.Size ?? new System.Drawing.Size(64, 64));
                texture.Name = $"{definition.Prefix}.{FormatCosmeticId(definition, id, cosmetic.CostumeIndex)}";
                cosmetic.Texture = (TEX0Node)_fileService.CopyNode(texture);
                cosmetic.Palette = texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(texture.GetPaletteNode()) : null;
                // We do this so the referenced (copied) texture is what's stored in the BRRES, instead of the original
                texture.Remove(true);
                ImportTexture(parentNode, cosmetic.Texture);
                ImportPalette(parentNode, cosmetic.Palette);
            }
            // Create pat entry
            if (definition.PatSettings != null)
            {
                foreach(var path in definition.PatSettings.Paths)
                {
                    node = rootNode.FindChild(path);
                    if (node != null)
                    {
                        CreatePatEntry(node, GetCosmeticId(definition, id, cosmetic.CostumeIndex), cosmetic?.Texture?.Name, cosmetic?.Palette?.Name);
                    }
                }
            }
            // If we have a model node, import that
            if (cosmetic.Model != null && definition.ModelPath != null)
            {
                node = definition.ModelPath != "" ? rootNode.FindChild(definition.ModelPath) : rootNode;
                parentNode = (BRRESNode)node;
                var model = ImportModel(parentNode, cosmetic.Model);
                model.Name = $"{definition.Prefix}{FormatCosmeticId(definition, id, cosmetic.CostumeIndex)}_TopN";
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
                model.Name = $"{definition.Prefix}{FormatCosmeticId(definition, id, cosmetic.CostumeIndex)}_TopN";
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

        // Remove cosmetics based on definition rules
        public void RemoveCosmetics(ResourceNode rootNode, CosmeticDefinition definition, int id)
        {
            if (definition.PatSettings != null)
            {
                RemovePatEntries(rootNode, definition, id);
            }
            var parentNode = definition.InstallLocation.NodePath != "" ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
            if (parentNode != null)
            {
                RemoveTextures((BRRESNode)parentNode, definition, id, !definition.InstallLocation.FilePath.EndsWith("\\") && parentNode.GetType() != typeof(ARCNode));
                RemoveModels((BRRESNode)parentNode, definition, id, !definition.InstallLocation.FilePath.EndsWith("\\") && parentNode.GetType() != typeof(ARCNode));
            }
        }

        // Remove separate file cosmetics
        public void RemoveCosmetics(CosmeticDefinition definition, int id)
        {
            var files = GetCosmeticPaths(definition, id);
            files.ForEach(x => File.Delete(x));
        }

        // Get color smash groups from a list of cosmetics
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

        // Color smash a list of cosmetics in a BRRES
        public void ColorSmashCosmetics(List<Cosmetic> cosmetics, ResourceNode rootNode, CosmeticDefinition definition)
        {
            var node = definition.InstallLocation.NodePath != "" ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
            if (node != null)
            {
                var bres = (BRRESNode)node;
                // Flip SharesData to false for all that need updating
                var sharesDataList = cosmetics.Where(x => x.ColorSmashChanged && x.SharesData == false).ToList();
                foreach(var cosmetic in sharesDataList)
                {
                    cosmetic.Texture = ReimportTexture(bres, cosmetic, definition.Format, definition.Size ?? new System.Drawing.Size(64, 64));
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
                    cosmetic.Texture = (TEX0Node)_fileService.CopyNode(cosmetic.Texture);
                    cosmetic.Palette = cosmetic.Texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(cosmetic.Texture.GetPaletteNode()) : null;
                }
            }
        }

        // Import cosmetics based on definition rules
        public void ImportCosmetics(CosmeticDefinition definition, List<Cosmetic> cosmetics, int id)
        {
            // If the definition doesn't use separate files, find the files and update them
            if (!definition.SeparateFiles)
            {
                var paths = GetCosmeticPaths(definition, id);
                foreach (var path in paths)
                {
                    var rootNode = _fileService.OpenFile(path);
                    if (rootNode != null)
                    {
                        RemoveCosmetics(rootNode, definition, id);
                        foreach (var cosmetic in cosmetics.OrderBy(x => x.InternalIndex))
                        {
                            ImportCosmetic(definition, cosmetic, id, rootNode);
                        }
                        if (!definition.FirstOnly && !definition.SeparateFiles)
                            ColorSmashCosmetics(cosmetics.OrderBy(x => x.InternalIndex).ToList(), rootNode, definition);
                        _fileService.SaveFile(rootNode);
                        _fileService.CloseFile(rootNode);
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
                    _fileService.SaveFileAs(rootNode, $"{_settingsService.BuildPath}{definition.InstallLocation.FilePath}" +
                        $"{definition.Prefix}{FormatCosmeticId(definition, id, cosmetic.CostumeIndex)}.{definition.InstallLocation.FileExtension}");
                    _fileService.CloseFile(rootNode);
                }
            }
        }

        // Format cosmetic ID to string based on definition
        public string FormatCosmeticId(CosmeticDefinition definition, int cosmeticId)
        {
            var id = (cosmeticId * definition.Multiplier).ToString("D" + definition.SuffixDigits);
            return id;
        }

        public string FormatCosmeticId(CosmeticDefinition definition, int cosmeticId, int? costumeIndex)
        {
            var id = ((cosmeticId * definition.Multiplier) + (costumeIndex ?? 0)).ToString("D" + definition.SuffixDigits);
            return id;
        }

        // Get all files containing cosmetics specified by the cosmetic definition
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

        // Check if ID associated with a cosmetic is within the range specified by the cosmetic definition
        public bool CheckIdRange(CosmeticDefinition definition, int id, string name, string prefix)
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

        public bool CheckIdRange(PatSettings patSettings, CosmeticDefinition definition, int id, int index)
        {
            return CheckIdRange(idType: patSettings.IdType ?? definition.IdType, multiplier: patSettings.Multiplier ?? definition.Multiplier, id, index);
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

        // Get costume index from node name
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

        // Calculate costume index
        public int GetCostumeIndex(int index, int multiplier, int id)
        {
            if (multiplier <= 1)
                return 0;
            index = index - (id * multiplier);
            return index;
        }

        // Get ID from name string
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

        // Get ID from definition
        public int GetCosmeticId(CosmeticDefinition definition, int cosmeticId, int? costumeIndex)
        {
            var id = (cosmeticId * definition.Multiplier) + costumeIndex ?? 0;
            return id;
        }

        // Get textures associated with provided cosmetic definition and IDs
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
                    id = fighterIds.GetIdOfType(definition.PatSettings.IdType ?? definition.IdType) + (definition.PatSettings.Offset ?? definition.Offset);
                    patEntries = pat.Children.Where(x => !restrictRange || CheckIdRange(definition.PatSettings, definition, id, Convert.ToInt32(((PAT0TextureEntryNode)x).FrameIndex))).ToList();
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
            // If the node path is an ARC node, search for a matching BRRES first and don't restrict range for textures
            if (start.GetType() == typeof(ARCNode))
            {
                start = start.Children.First(x => x.ResourceFileType == ResourceType.BRES && ((BRRESNode)x).FileIndex == id);
                restrictRange = false;
            }
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

        // Get models associated with provided cosmetic definition and IDs
        public List<MDL0Node> GetModels(CosmeticDefinition definition, ResourceNode node, FighterIds fighterIds, bool restrictRange)
        {
            // Try to get all models for cosmetic definition
            var nodes = new List<MDL0Node>();
            var id = fighterIds.GetIdOfType(definition.IdType) + definition.Offset;
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

        // Loads a list of all HD textures so we can more easily search for texture matches
        public List<string> PreloadHDTextures()
        {
            var directories = Directory.GetDirectories(_settingsService.BuildSettings.FilePathSettings.HDTextures, "*", SearchOption.AllDirectories);
            var hdImages = new ConcurrentBag<string>();
            Parallel.ForEach(directories, directory =>
            {
                var images = Directory.GetFiles(directory, "*.png").ToList();
                Parallel.ForEach(images, image =>
                {
                    hdImages.Add(image);
                });
            });
            HDImages = hdImages.ToList();
            return HDImages;
        }

        // Get HD texture by Dolphin texture name
        public string GetHDTexture(string textureName)
        {
            return HDImages.AsParallel().FirstOrDefault(x => Path.GetFileNameWithoutExtension(x) == textureName);
        }

        // Get BitmapImage of HD texture by Dolphin name
        public BitmapImage GetHDImage(string textureName)
        {
            var file = GetHDTexture(textureName);
            if (!string.IsNullOrEmpty(file))
            {
                return new BitmapImage(new Uri(file));
            }
            return null;
        }

        // Get cosmetics associated with provided cosmetic definition and IDs
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
                    HDImage = GetHDImage(texture.Texture?.DolphinTextureName),
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
                        ColorSequence = model.GetColorSequence(),
                        Id = GetCosmeticId(model.Name, definition)
                    });
                }
            }
            return cosmetics;
        }

        // TODO: When importing for a FirstOnly/SeparateFiles definition, check if SharesData. If so, save the image and import that instead of importing TEX0.

        // TODO: When importing a character, franchise icons with a null ID or an ID greater than any existing franchise icon will be installed as new. Any others
        // will overwrite existing ones.

        // TODO: separate RSPs and CSPs so they can be toggled on install

        // Get a list of fighter cosmetics
        public List<Cosmetic> GetFighterCosmetics(FighterIds fighterIds)
        {
            var settings = _settingsService.BuildSettings;
            var definitions = settings.CosmeticSettings.Where(x => x.CosmeticType != CosmeticType.FranchiseIcon).ToList();
            // Load HD textures in advance
            PreloadHDTextures();
            return GetCosmetics(fighterIds, definitions, true);
        }

        // Get a list of all fighter franchise icons
        public List<Cosmetic> GetFranchiseIcons()
        {
            var franchiseIcons = new List<Cosmetic>();
            var settings = _settingsService.BuildSettings;
            var definitions = settings.CosmeticSettings.Where(x => x.CosmeticType == CosmeticType.FranchiseIcon).ToList();
            // Get all franchise icons
            var allIcons = GetCosmetics(new FighterIds(), definitions, false);
            // Aggregate the models and transparent textures
            foreach(var icon in allIcons.Where(x => x.Texture != null).GroupBy(x => x.Id).Select(x => x.First()).ToList())
            {
                franchiseIcons.Add(new Cosmetic
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
                    Model = allIcons.FirstOrDefault(x => x.Id == icon.Id && x.Model != null)?.Model,
                    ColorSequence = allIcons.FirstOrDefault(x => x.Id == icon.Id && x.ColorSequence != null)?.ColorSequence
                });
            }
            return franchiseIcons;
        }

        // Get a list of cosmetics associated with provided IDs
        public List<Cosmetic> GetCosmetics(FighterIds fighterIds, List<CosmeticDefinition> definitions, bool restrictRange)
        {
            var cosmetics = new List<Cosmetic>();
            // Order them to ensure that cosmetics with multiple definitions pick the definitions favor definitions that have multiple cosmetics
            foreach (var cosmeticGroup in definitions.OrderByDescending(x => !x.SeparateFiles).OrderByDescending(x => !x.FirstOnly)
                .GroupBy(c => new { c.CosmeticType, c.Style }).ToList())
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
