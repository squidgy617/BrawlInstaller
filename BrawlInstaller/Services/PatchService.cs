using BrawlInstaller.Classes;
using BrawlInstaller.Common;
using BrawlLib.SSBB.ResourceNodes;
using Newtonsoft.Json;
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
        /// <inheritdoc cref="PatchService.CompareFiles(ResourceNode, ResourceNode)"/>
        FilePatch CompareFiles(ResourceNode leftFile, ResourceNode rightFile);

        /// <inheritdoc cref="PatchService.ExportFilePatch(FilePatch, string)"/>
        void ExportFilePatch(FilePatch filePatch, string outFile);

        /// <inheritdoc cref="PatchService.OpenFilePatch(string)"/>
        FilePatch OpenFilePatch(string inFile);

        /// <inheritdoc cref="PatchService.ApplyFilePatch(FilePatch, string)"/>
        void ApplyFilePatch(FilePatch patch, string targetFile);
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

        /// <summary>
        /// Compare two files
        /// </summary>
        /// <param name="leftFile">Left file for comparison</param>
        /// <param name="rightFile">Right file for comparison</param>
        /// <returns>File patch with altered nodes</returns>
        public FilePatch CompareFiles(ResourceNode leftFile, ResourceNode rightFile)
        {
            // Get nodes for both files for comparison
            var leftFileNodeDefs = GetNodeDefinitions(leftFile).FlattenList();
            var finalNodeDefs = GetNodeDefinitions(rightFile);
            var rightFileNodeDefs = finalNodeDefs.FlattenList();

            // Iterate through nodes from right file
            var skipNodeList = new List<NodeDef>();
            foreach(var rightNodeDef in rightFileNodeDefs.Where(x => !skipNodeList.Contains(x)))
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
                            var isContainer = rightNodeDef.IsContainer() && leftNodeDef.IsContainer();
                            rightNodeDef.IsChanged = true; // TODO: Probably have to do some stuff from exportPatchNode in old logic
                            rightNodeDef.Change = !isContainer ? NodeChangeType.Altered : NodeChangeType.Container;
                            // If not a container, mark children to be skipped, so they don't show up as changes unnecessarily
                            if (!isContainer)
                            {
                                skipNodeList.AddRange(rightNodeDef.GetChildrenRecursive());
                            }
                        }
                        break;
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
            return new FilePatch { NodeDefs = finalNodeDefs };
        }

        /// <summary>
        /// Apply a patch to a file
        /// </summary>
        /// <param name="patch">Patch to apply</param>
        /// <param name="targetFile">Target file</param>
        public void ApplyFilePatch(FilePatch patch, string targetFile)
        {
            var file = _fileService.OpenFile(targetFile);
            if (file != null)
            {
                var nodeDefs = patch.NodeDefs.RecursiveSelect(x => x.IsEnabled);
                foreach(var nodeDef in nodeDefs)
                {
                    ApplyNodeChange(file, nodeDef);
                }
                _fileService.SaveFile(file);
                _fileService.CloseFile(file);
            }
        }

        /// <summary>
        /// Apply a change to a node in file
        /// </summary>
        /// <param name="rootNode">Root node of file to change</param>
        /// <param name="nodeChange">Definition of change to apply</param>
        private void ApplyNodeChange(ResourceNode rootNode, NodeDef nodeChange)
        {
            // Get top-level parent
            var currentNodeDef = nodeChange;
            while (currentNodeDef.Parent != null)
            {
                currentNodeDef = currentNodeDef.Parent;
            }
            // Add node recursively
            ApplyNodeChangeRecursive(rootNode, currentNodeDef);
        }

        /// <summary>
        /// Recursively apply a node change
        /// </summary>
        /// <param name="rootNode">Root node of file to change</param>
        /// <param name="nodeChange">Definition of next node to apply change to</param>
        /// <param name="finalNodeDef">Definition of final change to make</param>
        private void ApplyNodeChangeRecursive(ResourceNode rootNode, NodeDef nodeChange)
        {
            ResourceNode changedNode = null;
            // Search for a match, skipping past a number of elements based on the index
            var match = rootNode.Children.Where(x => x.TreePath == nodeChange.Path).Skip(nodeChange.Index - 1).FirstOrDefault();
            changedNode = match;
            // Take action on the node
            // TODO: Handle changes to containers that are just param changes and stuff
            if (nodeChange.Change != NodeChangeType.None && nodeChange.Change != NodeChangeType.Container)
            {
                // Remove existing node
                if (match != null)
                {
                    rootNode.RemoveChild(match);
                }
            }
            if (nodeChange.Change == NodeChangeType.Altered || nodeChange.Change == NodeChangeType.Added)
            {
                // Add new node
                var newNode = _fileService.CreateNode(nodeChange.NodeType);
                rootNode.InsertChild(newNode, nodeChange.ContainerIndex);
                if (nodeChange.Node != null && !FilePatches.Folders.Contains(nodeChange.NodeType))
                {
                    newNode.Replace(nodeChange.Node);
                }
                // If it is on filesystem, pull it from there
                else if (!string.IsNullOrEmpty(nodeChange.NodeFilePath) && !FilePatches.Folders.Contains(nodeChange.NodeType))
                {
                    newNode.Replace(nodeChange.NodeFilePath);
                }
                newNode.Name = nodeChange.Name;
                // Handle color smashing
                if (nodeChange.NodeType == typeof(TEX0Node) && !string.IsNullOrEmpty(nodeChange.GroupName) && nodeChange.GroupName != nodeChange.Name)
                {
                    ((TEX0Node)newNode).SharesData = true;
                }
                // Update unique properties as needed
                foreach(var property in newNode.GetType().GetProperties().Where(x => FilePatches.UniqueProperties.Contains(x.Name)))
                {
                    // Get siblings with matching properties
                    var siblings = rootNode.Children.Where(x => x != newNode);
                    var usedValues = siblings.Select(x => x.GetType().GetProperties().FirstOrDefault(y => y.Name == property.Name)?.GetValue(x)).Where(x => x != null);
                    int value = (int)property.GetValue(newNode);
                    // Increment the value until it doesn't match any of the sibling values
                    while (usedValues.Contains(value))
                    {
                        value += 1;
                    }
                    property.SetValue(newNode, value);
                }
                changedNode = newNode;
                // If node was placed in middle of color smash group, move it
                if (nodeChange.GroupName == "" && changedNode != null && changedNode.GetType() == typeof(TEX0Node) && changedNode.PrevSibling() != null)
                {
                    while (changedNode.PrevSibling() != null && ((TEX0Node)changedNode.PrevSibling()).SharesData)
                    {
                        changedNode.MoveUp();
                    }
                }
            }
            // Apply property changes to container nodes
            // TODO: Do we even need this? Is it for MDL0 containers?
            // TODO: Move to FileService?
            if (changedNode != null && nodeChange.Change == NodeChangeType.Container && nodeChange.Node != null && !FilePatches.Folders.Contains(nodeChange.NodeType)
                && !FilePatches.ParentRequired.Contains(nodeChange.NodeType)) // Nodes that should always be replaced will not have properties updated
            {
                // Copy node properties from new node to existing
                foreach(var property in changedNode.GetType().GetProperties())
                {
                    if (property.CanWrite && property.GetSetMethod() != null)
                    {
                        if (property.GetValue(changedNode) != property.GetValue(nodeChange.Node) && !FilePatches.ParamBlacklist.Contains(property.Name))
                        {
                            property.SetValue(changedNode, property.GetValue(nodeChange.Node));
                        }
                    }
                }
            }
            // Apply ARC settings if applicable
            if (changedNode != null && changedNode.GetType().IsSubclassOf(typeof(ARCEntryNode)))
            {
                var arcEntryNode = changedNode as ARCEntryNode;
                arcEntryNode.FileType = nodeChange.ARCSettings.FileType;
                arcEntryNode.FileIndex = nodeChange.ARCSettings.FileIndex;
                arcEntryNode.GroupID = nodeChange.ARCSettings.GroupID;
                arcEntryNode.RedirectIndex = nodeChange.ARCSettings.RedirectIndex;
                arcEntryNode.RedirectTarget = nodeChange.ARCSettings.RedirectTarget;
            }
            // TODO: Force add, param updates, settings updates, all that good stuff
            // Drill down and apply changes
            if (nodeChange.IsContainer())
            {
                foreach(var nodeChangeChild in nodeChange.Children)
                {
                    if (changedNode != null)
                    {
                        ApplyNodeChangeRecursive(changedNode, nodeChangeChild);
                    }
                }
            }
        }

        /// <summary>
        /// Export file patch to selected location
        /// </summary>
        /// <param name="nodeDefs">Node definitions to export</param>
        /// <param name="outFile">Location to export to</param>
        public void ExportFilePatch(FilePatch filePatch, string outFile)
        {
            var nodeDefs = filePatch.NodeDefs.RecursiveSelect(x => x.IsEnabled).ToList();
            var path = _settingsService.AppSettings.TempPath + "\\FilePatchExport";
            // Save node list
            var json = JsonConvert.SerializeObject(nodeDefs, Formatting.Indented);
            _fileService.SaveTextFile($"{path}\\FilePatch.json", json);
            // Save nodes
            foreach(var nodeDef in nodeDefs.FlattenList())
            {
                if (!FilePatches.Folders.Contains(nodeDef.Node.GetType()))
                {
                    _fileService.SaveFileAs(nodeDef.Node, $"{path}\\{nodeDef.Id}");
                }
            }
            // Delete patch if it's being overwritten
            _fileService.DeleteFile(outFile);
            // Generate patch file
            _fileService.GenerateZipFileFromDirectory(path, outFile);
            _fileService.DeleteDirectory(path);
        }

        /// <summary>
        /// Open file patch
        /// </summary>
        /// <param name="inFile">File to open</param>
        /// <returns>File patch</returns>
        public FilePatch OpenFilePatch(string inFile)
        {
            var nodeDefs = new List<NodeDef>();
            var path = _settingsService.AppSettings.TempPath + "\\FilePatchImport";
            _fileService.ExtractZipFile(inFile, path);
            // Read node list
            var json = _fileService.ReadTextFile($"{path}\\FilePatch.json");
            if (!string.IsNullOrEmpty(json))
            {
                nodeDefs = JsonConvert.DeserializeObject<List<NodeDef>>(json);
                Parallel.ForEach(nodeDefs.FlattenList(), nodeDef =>
                {
                    if (!FilePatches.ParentRequired.Contains(nodeDef.NodeType))
                    {
                        nodeDef.Node = _fileService.OpenFile($"{path}\\{nodeDef.Id}");
                    }
                    else
                    {
                        nodeDef.NodeFilePath = $"{path}\\{nodeDef.Id}";
                    }
                });
            }
            // TODO: Delete folder after? If we can keep nodes in mem (Probably can't delete it, because bones must come from filesystem)
            return new FilePatch { NodeDefs = nodeDefs };
        }

        /// <summary>
        /// Get definitions of nodes for patch
        /// </summary>
        /// <param name="filePath">Path to get nodes from</param>
        /// <returns>Node definition</returns>
        private List<NodeDef> GetNodeDefinitions(ResourceNode rootNode)
        {
            var nodeDefs = new List<NodeDef>();
            if (rootNode != null)
            {
                foreach (var node in rootNode.Children)
                {
                    var nodeDef = GetNodeDefinition(node);
                    nodeDefs.Add(nodeDef);
                }
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
                Name = node.Name,
                NodeType = node.GetType(),
                ContainerIndex = node.Index
            };
            // Get color smash group if applicable
            if (node.GetType() == typeof(TEX0Node))
            {
                var texNode = node as TEX0Node;
                if (texNode.SharesData)
                {
                    var currentNode = texNode;
                    while (currentNode.NextSibling() != null)
                    {
                        currentNode = currentNode.NextSibling() as TEX0Node;
                        if (!currentNode.SharesData)
                        {
                            break;
                        }
                    }
                    nodeDef.GroupName = currentNode.Name;
                }
                else if (node.PrevSibling() != null && ((TEX0Node)node.PrevSibling()).SharesData)
                {
                    nodeDef.GroupName = node.Name;
                }
            }
            // Get ARC settings if applicable
            if (node.GetType().IsSubclassOf(typeof(ARCEntryNode)))
            {
                var arcEntryNode = node as ARCEntryNode;
                nodeDef.ARCSettings = new ARCEntrySettings
                {
                    FileType = arcEntryNode.FileType,
                    FileIndex = arcEntryNode.FileIndex,
                    GroupID = arcEntryNode.GroupID,
                    RedirectIndex = arcEntryNode.RedirectIndex,
                    RedirectTarget = arcEntryNode.RedirectTarget
                };
            }
            // Drill down into containers
            if (nodeDef.IsContainer())
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
        /// Get root node def for a color smash group
        /// </summary>
        /// <param name="nodeDef">Node definition to get root node for</param>
        /// <param name="nodeDefs">List of node definitions</param>
        /// <returns>Root node def of group</returns>
        private NodeDef GetNodeGroupRoot(NodeDef nodeDef, List<NodeDef> nodeDefs)
        {
            var groupRoot = nodeDef;
            var groupNodes = nodeDefs.Where(x => x.Node.Parent == nodeDef.Node.Parent).ToList();
            if (nodeDef.Node?.GetType() == typeof(TEX0Node) && ((TEX0Node)nodeDef.Node).SharesData)
            {
                var currentNode = nodeDef;
                while (groupNodes.IndexOf(currentNode) + 1 < groupNodes.Count)
                {
                    currentNode = groupNodes[groupNodes.IndexOf(currentNode) + 1];
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
