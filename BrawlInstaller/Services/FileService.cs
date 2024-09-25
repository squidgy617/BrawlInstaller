﻿using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Media.Imaging;
using BrawlInstaller.Common;

namespace BrawlInstaller.Services
{
    public interface IFileService
    {
        /// <inheritdoc cref="FileService.OpenFile(string)"/>
        ResourceNode OpenFile(string path);

        /// <inheritdoc cref="FileService.SaveFile(ResourceNode)"/>
        void SaveFile(ResourceNode node);

        /// <inheritdoc cref="FileService.SaveFileAs(ResourceNode, string)"/>
        void SaveFileAs(ResourceNode node, string path);

        /// <inheritdoc cref="FileService.CloseFile(ResourceNode)"/>
        void CloseFile(ResourceNode node);

        /// <inheritdoc cref="FileService.CopyNode(ResourceNode)"/>
        ResourceNode CopyNode(ResourceNode node);

        /// <inheritdoc cref="FileService.CopyFile(string, string)"/>
        void CopyFile(string inFile, string outFile);

        /// <inheritdoc cref="FileService.DeleteFile(string)"/>
        void DeleteFile(string file);

        /// <inheritdoc cref="FileService.SaveImage(BitmapImage, string)"/>
        void SaveImage(BitmapImage image, string outPath);

        /// <inheritdoc cref="FileService.SaveTextFile(string, string)"/>
        void SaveTextFile(string filePath, string text);

        /// <inheritdoc cref="FileService.LoadImage(string)"/>
        BitmapImage LoadImage(string filePath);

        /// <inheritdoc cref="FileService.ReadRawData(ResourceNode)"/>
        byte[] ReadRawData(ResourceNode node);

        /// <inheritdoc cref="FileService.ReplaceNodeRaw(ResourceNode, byte[])"/>
        void ReplaceNodeRaw(ResourceNode node, byte[] data);
    }
    [Export(typeof(IFileService))]
    internal class FileService : IFileService
    {
        // Services
        ISettingsService _settingsService { get; }

        [ImportingConstructor]
        public FileService(ISettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        // Methods

        /// <summary>
        /// Open a BrawlLib-compatible file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <returns>Root node of file</returns>
        public ResourceNode OpenFile(string path)
        {
            if (!File.Exists(path))
                return null;
            var rootNode = NodeFactory.FromFile(null, path);
            return rootNode;
        }

        /// <summary>
        /// Save a BrawlLib-compatible file
        /// </summary>
        /// <param name="node">Root node of file</param>
        public void SaveFile(ResourceNode node)
        {
            SaveFileAs(node, node.FilePath);
        }

        /// <summary>
        /// Save a BrawlLib-compatible file to a specified path
        /// </summary>
        /// <param name="node">Root node of file</param>
        /// <param name="path">Path to save file to</param>
        public void SaveFileAs(ResourceNode node, string path)
        {
            node.Export(path);
            node.IsDirty = false;
        }

        /// <summary>
        /// Close a BrawlLib-compatible file
        /// </summary>
        /// <param name="node">Root node of file to close</param>
        public void CloseFile(ResourceNode node)
        {
            node.Dispose();
        }

        /// <summary>
        /// Create a duplicate of a ResourceNode
        /// </summary>
        /// <param name="node">ResourceNode to copy</param>
        /// <returns>Copy of node</returns>
        public ResourceNode CopyNode(ResourceNode node)
        {
            //var newNode = NodeFactory.FromAddress(node.Parent, node.WorkingSource.Address, node.WorkingSource.Length);
            var guid = Guid.NewGuid().ToString();
            var path = $"{_settingsService.AppSettings.TempPath}\\{guid}";
            CreateDirectory(path);
            node.Export(path);
            ResourceNode newNode = null;
            if (node.GetType() == typeof(TEX0Node) && ((TEX0Node)node).SharesData)
            {
                newNode = new TEX0Node();
                ((TEX0Node)newNode).SharesData = true;
            }
            else
            {
                newNode = NodeFactory.FromFile(null, path, node.GetType());
            }
            if (newNode.GetType().IsAssignableFrom(typeof(BRESEntryNode)))
            {
                ((BRESEntryNode)newNode).OriginalPath = "";
            }
            File.Delete(path);
            newNode.Name = node.Name;
            return newNode;
        }

        /// <summary>
        /// Copy a file
        /// </summary>
        /// <param name="inFile">Filepath to copy</param>
        /// <param name="outFile">New file path</param>
        public void CopyFile(string inFile, string outFile)
        {
            if (File.Exists(inFile))
            {
                CreateDirectory(outFile);
                File.Copy(inFile, outFile, true);
            }
        }

        /// <summary>
        /// Delete file
        /// </summary>
        /// <param name="file">File to delete</param>
        public void DeleteFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        /// <summary>
        /// Save an image
        /// </summary>
        /// <param name="image">BitmapImage to save</param>
        /// <param name="outFile">Output path of image file</param>
        public void SaveImage(BitmapImage image, string outFile)
        {
            SaveImage(image.ToBitmap(), outFile);
        }

        /// <summary>
        /// Load an image from a file
        /// </summary>
        /// <param name="filePath">Path to image</param>
        /// <returns>BitmapImage of image</returns>
        public BitmapImage LoadImage(string filePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(filePath);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// Save an image
        /// </summary>
        /// <param name="image">Bitmap to save</param>
        /// <param name="outFile">Output path of image file</param>
        private void SaveImage(Bitmap image, string outFile)
        {
            CreateDirectory(outFile);
            image.Save(outFile);
        }

        /// <summary>
        /// Save a text file
        /// </summary>
        /// <param name="filePath">Path to save to</param>
        /// <param name="text">Text to save</param>
        public void SaveTextFile(string filePath, string text)
        {
            File.WriteAllText(filePath, text);
        }

        /// <summary>
        /// Create a directory if it does not exist
        /// </summary>
        /// <param name="path">Directory to create</param>
        private void CreateDirectory(string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
        }

        /// <summary>
        /// Read raw data of a node
        /// </summary>
        /// <param name="node">Node to read data from</param>
        /// <returns>Raw data of node</returns>
        public byte[] ReadRawData(ResourceNode node)
        {
            var length = node.WorkingUncompressed.Length;
            byte[] data = new byte[length];
            Marshal.Copy(node.WorkingUncompressed.Address, data, 0, length);
            return data;
        }

        /// <summary>
        /// Replace raw data of a node
        /// </summary>
        /// <param name="node">Node to replace</param>
        /// <param name="data">Byte array to replace node data with</param>
        public void ReplaceNodeRaw(ResourceNode node, byte[] data)
        {
            unsafe
            {
                fixed (byte* ptr = data)
                {
                    node.ReplaceRaw(ptr, data.Length);
                }
            }
        }
    }
}
