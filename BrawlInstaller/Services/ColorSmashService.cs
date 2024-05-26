using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlLib.Internal.Windows.Forms;
using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.Wii.Textures;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BrawlLib.BrawlManagerLib.TextureContainer;

namespace BrawlInstaller.Services
{
    public interface IColorSmashService
    {
        void ColorSmashCosmetics(List<Cosmetic> cosmetics, BRRESNode bres);
    }
    [Export(typeof(IColorSmashService))]
    internal class ColorSmashService : IColorSmashService
    {
        // Services
        IFileService _fileService { get; }

        [ImportingConstructor]
        public ColorSmashService(IFileService fileService)
        {
            _fileService = fileService;
        }

        // Constants
        public const string ColorSmashDirectory = "cs";
        public const string ColorSmashOutDirectory = ColorSmashDirectory + "\\out";

        // Methods

        // Color smash TEX0 nodes
        // TODO: handle errors
        public void ColorSmashCosmetics(List<Cosmetic> cosmetics, BRRESNode bres)
        {
            var folder = bres.GetFolder<TEX0Node>();
            if (Directory.Exists(ColorSmashDirectory))
                Directory.Delete(ColorSmashDirectory, true);
            Directory.CreateDirectory(ColorSmashDirectory);
            Directory.CreateDirectory(ColorSmashOutDirectory);
            // Save images to color smash input folder
            foreach(var cosmetic in cosmetics)
            {
                cosmetic.Image.Save($"{ColorSmashDirectory}\\{cosmetics.IndexOf(cosmetic):D5}.png");
            }
            // Get palette count
            var paletteCount = 0;
            paletteCount = cosmetics.Select(x => GetTexture(bres, x.Texture.Name).GetPaletteNode().Palette.Entries.Length).Max();
            paletteCount = Math.Min(paletteCount, 256);
            // Get mip count
            var mipCount = cosmetics.Select(x => GetTexture(bres, x.Texture.Name).LevelOfDetail).Max();
            // Color smash
            ColorSmasher(paletteCount);
            foreach(var cosmetic in cosmetics)
            {
                var index = cosmetic.Texture.Index;
                var name = cosmetic.Texture.Name;
                // Clean up existing texture
                cosmetic.Texture?.Remove(true);
                cosmetic.Texture?.Dispose();
                cosmetic.Palette?.Dispose();
                // Import color smashed image
                var file = $"{ColorSmashOutDirectory}\\{cosmetics.IndexOf(cosmetic):D5}.png";
                var texture = ColorSmashTextureImport(bres, file, WiiPixelFormat.CI8, mipCount);
                // Update and move texture node
                texture.OriginalPath = "";
                texture.Name = name;
                folder.RemoveChild(texture);
                folder.InsertChild(texture, index);
                // Flip sharesdata
                if (cosmetic != cosmetics.LastOrDefault())
                    texture.SetSharesData(true, false);
                cosmetic.Texture = (TEX0Node)_fileService.CopyNode(texture);
                cosmetic.Palette = texture.GetPaletteNode() != null ? (PLT0Node)_fileService.CopyNode(texture.GetPaletteNode()) : null;
                // Flip ColorSmashChanged so we don't redo color smashing on all files
                cosmetic.ColorSmashChanged = false;
                // Set this so it saves properly
                texture.IsDirty = true;
                if (texture.HasPalette)
                    texture.GetPaletteNode().IsDirty = true;
            }
            if (Directory.Exists(ColorSmashDirectory))
                Directory.Delete(ColorSmashDirectory, true);
        }

        public TEX0Node ColorSmashTextureImport(BRRESNode destinationNode, string imageSource, WiiPixelFormat format, int mipCount = 1)
        {
            var dialog = new TextureConverterDialog();
            dialog.ImageSource = imageSource;
            dialog.InitialFormat = format;
            dialog.ChkImportPalette.Checked = true;
            dialog.Automatic = true;
            dialog.numLOD.Value = mipCount;
            dialog.ShowDialog(null, destinationNode);
            var node = dialog.TEX0TextureNode;
            dialog.Dispose();
            return node;
        }

        // GetTexture reimplemented from CosmeticService
        public TEX0Node GetTexture(ResourceNode rootNode, CosmeticDefinition definition, string name)
        {
            var parentNode = !string.IsNullOrEmpty(definition.InstallLocation.NodePath) ? rootNode.FindChild(definition.InstallLocation.NodePath) : rootNode;
            if (parentNode != null)
            {
                return GetTexture((BRRESNode)parentNode, name);
            }
            return null;
        }

        public TEX0Node GetTexture(BRRESNode parentNode, string name)
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

        public void ColorSmasher(int paletteCount)
        {
            var directory = Directory.CreateDirectory(ColorSmashDirectory);
            var outDirectory = Directory.CreateDirectory(ColorSmashOutDirectory);
            Process cSmash = Process.Start(new ProcessStartInfo
            {
                FileName = "color_smash.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"-c RGB5A3 -n {paletteCount}"
            });
            cSmash?.WaitForExit();
        }

        public void ColorSmasher(int paletteCount, BRRESNode b, int index, Dictionary<int, string> names, int mips = 1)
        {
            var directory = Directory.CreateDirectory(ColorSmashDirectory);
            var outDirectory = Directory.CreateDirectory(ColorSmashOutDirectory);
            Process cSmash = Process.Start(new ProcessStartInfo
            {
                FileName = "color_smash.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"-c RGB5A3 -n {paletteCount}"
            });
            cSmash?.WaitForExit();

            List<TEX0Node> textureList = new List<TEX0Node>();
            int count = 0;
            BRESGroupNode texGroup = b.GetOrCreateFolder<TEX0Node>();
            foreach (FileInfo f in outDirectory.GetFiles())
            {
                FileInfo f2 = new FileInfo($"{directory.FullName}\\{f.Name}");
                int i = int.Parse(f.Name.Substring(0, f.Name.IndexOf(".", StringComparison.Ordinal)));
                using (TextureConverterDialog dlg = new TextureConverterDialog())
                {
                    dlg.ImageSource = f.FullName;
                    dlg.ChkImportPalette.Checked = true;
                    dlg.numLOD.Value = mips;
                    dlg.Automatic = true;
                    dlg.StartingFormat = WiiPixelFormat.CI8;

                    if (dlg.ShowDialog(null, b) != DialogResult.OK)
                    {
                        continue;
                    }

                    TEX0Node t = dlg.TEX0TextureNode;
                    t.Name = names[i];
                    textureList.Add(t);
                    dlg.Dispose();
                    t.OriginalPath = "";
                    if (texGroup.Children.Count > count + 1)
                    {
                        texGroup.RemoveChild(t);
                        texGroup.InsertChild(t, false, index + count);
                    }

                    count++;
                }

                f2.Delete();
            }

            if (textureList.Count > 0)
            {
                textureList.Remove(textureList.Last());
                foreach (TEX0Node t in textureList)
                {
                    t.SharesData = true;
                }
            }

            if (directory.GetFiles().Length > 0)
            {

                foreach (FileInfo f in directory.GetFiles())
                {
                    int i = int.Parse(f.Name.Substring(0, f.Name.IndexOf(".", StringComparison.Ordinal)));
                    using (TextureConverterDialog dlg = new TextureConverterDialog())
                    {
                        dlg.ImageSource = f.FullName;
                        dlg.numLOD.Value = mips;
                        dlg.Automatic = true;

                        if (dlg.ShowDialog(null, b) != DialogResult.OK)
                        {
                            continue;
                        }

                        TEX0Node t = dlg.TEX0TextureNode;
                        t.Name = names[i];

                        dlg.Dispose();
                        t.OriginalPath = "";
                        if (texGroup.Children.Count > count + 1)
                        {
                            texGroup.RemoveChild(t);
                            texGroup.InsertChild(t, false, index + count);
                        }

                        count++;
                    }
                }
            }
        }
    }
}
