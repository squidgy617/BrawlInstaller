﻿using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface IFileService
    {
        ResourceNode OpenFile(string path);
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
            var rootNode = NodeFactory.FromFile(null, path);
            return rootNode;
        }
    }
}
