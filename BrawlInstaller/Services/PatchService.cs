using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlLib.SSBB.ResourceNodes;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlInstaller.Services
{
    public interface IPatchService
    {
        /// <inheritdoc cref="PatchService.CompareFiles(string, string)"/>
        List<NodeDef> CompareFiles(string leftFile, string rightFile);
    }

    [Export(typeof(IPatchService))]
    internal class PatchService : IPatchService
    {
        // Services
        ISettingsService _settingsService { get; }
        IFileService _fileService { get; }

        [ImportingConstructor]
        public PatchService(ISettingsService settingsService, IFileService fileService)
        {
            _settingsService = settingsService;
            _fileService = fileService;
        }

        // Methods

        public List<NodeDef> CompareFiles(string leftFile, string rightFile)
        {
            // Get nodes for both files for comparison
            var leftFileNodeDefs = GetNodeDefinitions(leftFile).FlattenList();
            var finalNodeDefs = GetNodeDefinitions(rightFile);
            var rightFileNodeDefs = finalNodeDefs.FlattenList();

            // Iterate through nodes from right file
            foreach(var rightNodeDef in rightFileNodeDefs)
            {
                NodeDef removeNode = null;
                var matchFound = false;
                // For every right node, search the left nodes for a match
                foreach(var leftNodeDef in leftFileNodeDefs)
                {
                    // If the path matches, we've found a match
                    // Use TreePath instead of TreePathAbsolute, because TreePath excludes root node, whichi is sometimes the file name
                    if (rightNodeDef.Path == leftNodeDef.Path)
                    {
                        removeNode = leftNodeDef;
                        matchFound = true;
                        // If we've found a match, but MD5s do NOT match, this is an altered node and should be part of the patch
                        // If the node is color smashed, use the root of the color smash group to determine if altered
                        if (rightNodeDef.MD5 != leftNodeDef.MD5 || GetNodeGroupRoot(rightNodeDef, rightFileNodeDefs).MD5 != GetNodeGroupRoot(leftNodeDef, leftFileNodeDefs).MD5)
                        {
                            rightNodeDef.IsChanged = true; // TODO: Probably have to do some stuff from exportPatchNode in old logic
                            rightNodeDef.Change = NodeChangeType.Altered;
                            break;
                        }
                    }
                }
                // If we never found a match for a node in the right file, it's a brand new node, and should also be exported
                if (!matchFound)
                {
                    rightNodeDef.IsChanged = true; // TODO: Probably have to do some stuff from exportPatchNode in old logic
                    rightNodeDef.Change = NodeChangeType.Added;
                }
                // If we found a match at all, the left file node should be removed from the list for comparison, to speed up searchs and prevent false positives
                if (removeNode != null)
                {
                    leftFileNodeDefs.Remove(removeNode);
                }
            }
            // Mark nodes for removal
            foreach(var removedNode in leftFileNodeDefs)
            {
                removedNode.IsChanged = true;
                removedNode.Change = NodeChangeType.Removed;
                finalNodeDefs.AddNode(removedNode);
            }
            finalNodeDefs = finalNodeDefs.RecursiveSelect(x => x.IsChanged || x.Children.Any(y => y.IsChanged)).ToList();
            return finalNodeDefs;
        }

        /// <summary>
        /// Get definitions of nodes for patch
        /// </summary>
        /// <param name="filePath">Path to get nodes from</param>
        /// <returns>Node definition</returns>
        private List<NodeDef> GetNodeDefinitions(string filePath)
        {
            var rootNode = _fileService.OpenFile(filePath);
            var nodeDefs = new List<NodeDef>();
            if (rootNode != null)
            {
                foreach (var node in rootNode.Children)
                {
                    var nodeDef = GetNodeDefinition(node);
                    nodeDefs.Add(nodeDef);
                }
                //_fileService.CloseFile(rootNode);
                // TODO: Close files after getting them. Probably need to do that outside of this method, may need to make it accept a ResourceNode instead
            }
            return nodeDefs;
        }

        /// <summary>
        /// Get definition of a single node
        /// </summary>
        /// <param name="node">Node to get definition for</param>
        /// <returns>Node definitions</returns>
        private NodeDef GetNodeDefinition(ResourceNode node)
        {
            NodeDef nodeDef = new NodeDef 
            { 
                Index = GetPatchNodeIndex(node),
                MD5 = node.MD5Str(),
                Node = node,
                Path = node.TreePath,
                ResourceType = node.ResourceFileType,
                Name = node.Name
            };
            if (IsContainer(node))
            {
                foreach (var child in node.Children)
                {
                    nodeDef.AddChild(GetNodeDefinition(child));
                }
            }
            return nodeDef;
        }

        /// <summary>
        /// Get index of patch node
        /// </summary>
        /// <param name="node">Node to get index for</param>
        /// <returns>Index</returns>
        private int GetPatchNodeIndex(ResourceNode node)
        {
            if (node.Parent != null)
            {
                var i = 0;
                foreach (var child in node.Parent.Children)
                {
                    if (child.Name == node.Name)
                    {
                        i++;
                    }
                    if (child.Index == node.Index)
                    {
                        return i;
                    }
                }
            }
            return 1;
        }

        /// <summary>
        /// Checks if a node is considered a container or not
        /// </summary>
        /// <param name="node">Node to check</param>
        /// <returns>Whether or not node is container</returns>
        private bool IsContainer(ResourceNode node)
        {
            // Whitelisted containers are always true
            if (FilePatches.Containers.Contains(node.GetType()))
            {
                return true;
            }
            // MDL0Nodes are only containers if they don't have anything other than Bones and Definitions
            if (node.GetType() == typeof(MDL0Node))
            {
                if (!node.Children.Any(x => x.Name != "Bones" || x.Name != "Definitions"))
                {
                    return true;
                }
                return false;
            }
            // MDL0BoneNodes are only containers if they have children
            if (node.GetType() == typeof(MDL0BoneNode) && node.Children.Count > 0)
            {
                return true;
            }
            // If no checks passed, it's not a container
            return false;
        }

        /// <summary>
        /// Get root node def for a color smash group
        /// </summary>
        /// <param name="nodeDef">Node definition to get root node for</param>
        /// <param name="nodeDefs">List of node definitions</param>
        /// <returns>Root node def of group</returns>
        private NodeDef GetNodeGroupRoot(NodeDef nodeDef, List<NodeDef> nodeDefs)
        {
            var groupRoot = nodeDef;
            if (nodeDef.Node?.GetType() == typeof(TEX0Node) && ((TEX0Node)nodeDef.Node).SharesData)
            {
                var currentNode = nodeDef;
                while (nodeDefs.IndexOf(currentNode) + 1 < nodeDefs.Count)
                {
                    currentNode = nodeDefs[nodeDefs.IndexOf(currentNode) + 1];
                    if (!((TEX0Node)nodeDef.Node).SharesData)
                    {
                        break;
                    }
                }
                groupRoot = currentNode;
            }
            return groupRoot;
        }
    }
}
