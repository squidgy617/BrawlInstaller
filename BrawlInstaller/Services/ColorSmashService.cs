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
        // TODO: Get rid of this, for testing only
        IDialogService _dialogService { get; }

        [ImportingConstructor]
        public ColorSmashService(IFileService fileService, IDialogService dialogService)
        {
            _fileService = fileService;
            _dialogService = dialogService;
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
        public void ColorSmashCosmetics(List<Cosmetic> cosmetics, BRRESNode bres)
        {
            var folder = bres.GetFolder<TEX0Node>();
            var paletteFolder = bres.GetFolder<PLT0Node>();
            var colorSmashDirectory = Path.GetFullPath(ColorSmashDirectory);
            var colorSmashOutDirectory = Path.GetFullPath(ColorSmashOutDirectory);
            RegenerateDirectories();
            // Save images to color smash input folder
            foreach(var cosmetic in cosmetics)
            {
                _fileService.SaveImage(cosmetic.Image, $"{colorSmashDirectory}\\{cosmetics.IndexOf(cosmetic):D5}.png");
            }
            // Get palette count
            var paletteCount = 0;
            paletteCount = cosmetics.Select(x => GetTexture(bres, x.Texture.Name).GetPaletteNode().Palette.Entries.Length).Max();
            paletteCount = Math.Min(paletteCount, 256);
            // Get mip count
            var mipCount = cosmetics.Select(x => GetTexture(bres, x.Texture.Name).LevelOfDetail).Max();
            // Color smash
            var success = ColorSmasher(paletteCount);
            // TODO: Better way to handle this? Trying twice seems silly
            // If color smashing wasn't successful, try again
            if (!success)
            {
                RegenerateDirectories();
                // Save images to color smash input folder
                foreach (var cosmetic in cosmetics)
                {
                    _fileService.SaveImage(cosmetic.Image, $"{colorSmashDirectory}\\{cosmetics.IndexOf(cosmetic):D5}.png");
                }
                // Recalculate palette count, removing any alpha 0 entries
                paletteCount = cosmetics.Select(x => GetTexture(bres, x.Texture.Name).GetPaletteNode().Palette.Entries.Where(y => y.A > 0).Count()).Max();
                paletteCount = Math.Min(paletteCount, 256);
                // Try again
                success = ColorSmasher(paletteCount);
                // If it fails again, throw an error
                if (!success)
                {
                    throw new Exception("Error while trying to color smash. Please ensure all cosmetics are color smashable.");
                }
            }
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
                var file = $"{colorSmashOutDirectory}\\{cosmetics.IndexOf(cosmetic):D5}.png";
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
            _fileService.DeleteDirectory(colorSmashDirectory);
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
            // TODO: Get rid of this, for testing only
            if (node.Height == 0 && node.Width == 0)
            {
                _dialogService.ShowMessage($"Texture imported with height 0 and width 0. Please share the contents of this message with Squidgy :)\nImage: {Path.GetFileName(imageSource)}\nFormat: {format}\nDestination: {destinationNode.Name}", "Test Message");
            }
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
        private bool ColorSmasher(int paletteCount)
        {
            Process cSmash = Process.Start(new ProcessStartInfo
            {
                FileName = "color_smash.exe",
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = $"-c RGB5A3 -n {paletteCount}"
            });
            cSmash?.WaitForExit();
            // If there are any images in the "in" folder that don't exist in the "out" folder, it didn't actually succeed
            return !_fileService.GetFiles(ColorSmashDirectory, "*.png").Any(x => !_fileService.GetFiles(ColorSmashOutDirectory, "*.png").Select(y => Path.GetFileNameWithoutExtension(y)).Contains(Path.GetFileNameWithoutExtension(x)));
        }

        /// <summary>
        /// Regenerate color smash directories
        /// </summary>
        private void RegenerateDirectories()
        {
            var colorSmashDirectory = Path.GetFullPath(ColorSmashDirectory);
            var colorSmashOutDirectory = Path.GetFullPath(ColorSmashOutDirectory);
            _fileService.DeleteDirectory(colorSmashDirectory);
            _fileService.CreateDirectory(colorSmashDirectory);
            _fileService.CreateDirectory(colorSmashOutDirectory);
        }
    }
}
