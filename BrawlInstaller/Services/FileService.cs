using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface IFileService
    {
        ResourceNode OpenFile(string path);
        void SaveFile(ResourceNode node);
        void SaveFileAs(ResourceNode node, string path);
        ResourceNode CopyNode(ResourceNode node);
    }
    [Export(typeof(IFileService))]
    internal class FileService : IFileService
    {
        [ImportingConstructor]
        public FileService()
        {

        }

        // Methods
        public ResourceNode OpenFile(string path)
        {
            if (!File.Exists(path))
                return null;
            var rootNode = NodeFactory.FromFile(null, path);
            return rootNode;
        }

        public void SaveFile(ResourceNode node)
        {
            SaveFileAs(node, node.FilePath);
        }

        public void SaveFileAs(ResourceNode node, string path)
        {
            node.Export(path);
            node.IsDirty = false;
        }

        public ResourceNode CopyNode(ResourceNode node)
        {
            node.Export("tempNode");
            var newNode = NodeFactory.FromFile(null, "tempNode");
            File.Delete("tempNode");
            return newNode;
        }
    }
}
