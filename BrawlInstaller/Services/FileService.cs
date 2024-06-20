using BrawlLib.SSBB.ResourceNodes;
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
    }
    [Export(typeof(IFileService))]
    internal class FileService : IFileService
    {
        [ImportingConstructor]
        public FileService()
        {

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
            node.Export(guid);
            var newNode = NodeFactory.FromFile(null, guid);
            File.Delete(guid);
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
                File.Copy(inFile, outFile, true);
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
    }
}
