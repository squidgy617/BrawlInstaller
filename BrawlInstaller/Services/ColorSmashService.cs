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
        /// <inheritdoc cref="ColorSmashService.ColorSmashCosmetics(List{Cosmetic}, BRRESNode)"/>
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

        /// <summary>
        /// Color smash provided cosmetics
        /// </summary>
        /// <param name="cosmetics">Cosmetics to color smash</param>
        /// <param name="bres">Parent BRRES for textures</param>
        // TODO: handle errors
        public void ColorSmashCosmetics(List<Cosmetic> cosmetics, BRRESNode bres)
        {
            var folder = bres.GetFolder<TEX0Node>();
            var paletteFolder = bres.GetFolder<PLT0Node>();
            _fileService.DeleteDirectory(ColorSmashDirectory);
            _fileService.CreateDirectory(ColorSmashDirectory);
            _fileService.CreateDirectory(ColorSmashOutDirectory);
            // Save images to color smash input folder
            foreach(var cosmetic in cosmetics)
            {
                _fileService.SaveImage(cosmetic.Image, $"{ColorSmashDirectory}\\{cosmetics.IndexOf(cosmetic):D5}.png");
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
                var currentTexture = GetTexture(bres, cosmetic.Texture.Name);
                var currentPalette = currentTexture?.GetPaletteNode();
                var index = currentTexture.Index;
                var name = currentTexture.Name;
                // Clean up existing texture
                folder.Children.Remove(currentTexture);
                paletteFolder.Children.Remove(currentPalette);
                currentTexture?.Dispose();
                currentPalette?.Dispose();
                // Import color smashed image
                var file = $"{ColorSmashOutDirectory}\\{cosmetics.IndexOf(cosmetic):D5}.png";
                var texture = ColorSmashTextureImport(bres, file, WiiPixelFormat.CI8, mipCount);
                // Update and move texture node
                texture.OriginalPath = "";
                if (texture.GetPaletteNode() != null)
                    texture.GetPaletteNode().Name = name;
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
            _fileService.DeleteDirectory(ColorSmashDirectory);
        }

        /// <summary>
        /// Import color smashed texture
        /// </summary>
        /// <param name="destinationNode">Destination node to import textures to</param>
        /// <param name="imageSource">Source of image to import</param>
        /// <param name="format">Encoding format</param>
        /// <param name="mipCount">Mipmap count</param>
        /// <returns>TEX0Node</returns>
        private TEX0Node ColorSmashTextureImport(BRRESNode destinationNode, string imageSource, WiiPixelFormat format, int mipCount = 1)
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

        /// <summary>
        /// Get texture by name
        /// </summary>
        /// <param name="parentNode">Parent node of texture</param>
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
        /// Run color smasher on textures
        /// </summary>
        /// <param name="paletteCount">Palette count to use</param>
        private void ColorSmasher(int paletteCount)
        {
            Process cSmash = Process.Start(new ProcessStartInfo
            {
                FileName = "color_smash.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"-c RGB5A3 -n {paletteCount}"
            });
            cSmash?.WaitForExit();
        }
    }
}
