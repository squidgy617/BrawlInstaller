using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Classes
{
    public static class FilePatches
    {
        public static List<Type> Containers = new List<Type> 
        { 
            typeof(ARCNode),
            typeof(BRRESNode),
            typeof(BLOCNode),
            typeof(BRESGroupNode),
            typeof(TyDataNode),
            typeof(TyDataListNode),
            typeof(GDORNode),
            typeof(GDBFNode),
            typeof(MDL0GroupNode),
            typeof(GIB2Node),
            typeof(GMOVNode),
            typeof(GET1Node),
            typeof(GLK2Node),
            typeof(GNDVNode),
            typeof(GEG1Node)
        };

        public static List<Type> Folders = new List<Type>
        {
            typeof(BRESGroupNode),
            typeof(MDL0GroupNode)
        };
    }

    public class NodeDef
    {
        public ResourceNode Node { get; set; } = null;
        public string MD5 { get; set; } = string.Empty;
        public int Index { get; set; } = 0;
        public string Path { get; set; } = string.Empty;
        public List<NodeDef> Children { get; set; } = new List<NodeDef>();
        public bool IsChanged { get; set; } = false;
        public NodeChangeType Change { get; set; } = NodeChangeType.None;
    }

    public enum NodeChangeType
    {
        None,
        Altered,
        Added,
        Removed
    }
}
