﻿using BrawlLib.SSBB.ResourceNodes;
using BrawlLib.SSBB.Types;
using Newtonsoft.Json;
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

        public static List<string> ParamBlacklist = new List<string>
        {
            "FileType",
            "FileIndex",
            "GroupID",
            "RedirectIndex",
            "RedirectTarget",
            "Children",
            "Parent"
        };
    }

    public class FilePatch
    {
        public List<NodeDef> NodeDefs { get; set; } = new List<NodeDef>();
    }

    public class NodeDef
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [JsonIgnore] public ResourceNode Node { get; set; } = null;
        public string MD5 { get; set; } = string.Empty;
        public int Index { get; set; } = 0;
        public string Path { get; set; } = string.Empty;
        public List<NodeDef> Children { get; set; } = new List<NodeDef>();
        [JsonIgnore] public NodeDef Parent { get; set; } = null;
        public bool IsChanged { get; set; } = false;
        public NodeChangeType Change { get; set; } = NodeChangeType.None;
        public ResourceType ResourceType { get; set; } = ResourceType.Unknown;
        public string Name { get; set; }
        [JsonIgnore] public string Symbol { get => GetSymbol(); }
        public string GroupName { get; set; } = string.Empty;
        public Type NodeType { get; set; }
        public int ContainerIndex { get; set; }
        public ARCEntrySettings ARCSettings { get; set; } = null;

        private string GetSymbol()
        {
            switch (Change)
            {
                case NodeChangeType.Altered:
                case NodeChangeType.Container:
                    return "~ ";
                case NodeChangeType.Added:
                    return "+ ";
                case NodeChangeType.Removed: 
                    return "- ";
                default:
                    return "";
            }
        }

        public void AddChild(NodeDef child)
        {
            Children.Add(child);
            child.Parent = this;
        }

        public bool IsContainer()
        {
            // Whitelisted containers are always true
            if (FilePatches.Containers.Contains(NodeType))
            {
                return true;
            }
            // MDL0Nodes are only containers if they don't have anything other than Bones and Definitions
            if (NodeType == typeof(MDL0Node))
            {
                if (!Children.Any(x => x.Name != "Bones" || x.Name != "Definitions") && !Node.Children.Any(x => x?.Name != "Bones" || x?.Name != "Definitions"))
                {
                    return true;
                }
                return false;
            }
            // MDL0BoneNodes are only containers if they have children
            if (NodeType == typeof(MDL0BoneNode) && (Children.Count > 0 || Node.Children?.Count > 0))
            {
                return true;
            }
            // If no checks passed, it's not a container
            return false;
        }
    }

    public class ARCEntrySettings
    {
        public ARCFileType FileType { get; set; }
        public short FileIndex { get; set; }
        public byte GroupID { get; set; }
        public short RedirectIndex { get; set; }
        public string RedirectTarget { get; set; }
    }

    public enum NodeChangeType
    {
        None,
        Altered,
        Added,
        Removed,
        Container
    }
}
