﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Aegis.Core.FileSystem
{
    /// <summary>
    /// Data structure representing the virtual tree of files contained in an archive.
    /// </summary>
    internal sealed class VirtualFileTree
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualFileTree"/> class.
        /// </summary>
        public VirtualFileTree()
        {
            this.Root = new VirtualFileTreeNode(
                directoryName: string.Empty,
                parent: null);
        }

        /// <summary>
        /// Gets the root node of the tree.
        /// </summary>
        private VirtualFileTreeNode Root { get; }

        /// <summary>
        /// Adds information about a file to the tree.
        /// </summary>
        /// <param name="fileInfo">The file to add.</param>
        public void Add(AegisFileInfo fileInfo)
        {
            var treeNode = this.GetNodeForDirectoryPath(fileInfo.Path, createIfNotExists: true);
            treeNode.Files.Add(fileInfo.Path.FileName, fileInfo);
        }

        /// <summary>
        /// Removes the file at the given virtual path from the tree.
        /// </summary>
        /// <param name="filePath">The path of the file to remove.</param>
        /// <returns>The <see cref="AegisFileInfo"/> of the file that was removed, or null if it wasn't found.</returns>
        public AegisFileInfo Remove(AegisVirtualFilePath filePath)
        {
            var treeNode = this.GetNodeForDirectoryPath(filePath);

            // TODO: Implement Remove() method on VirtualFileTreeNode.
            // This method will need to prune "empty" directories *except* an empty root.
            return null;
        }

        /// <summary>
        /// Finds the file at the given virtual path from the tree.
        /// </summary>
        /// <param name="filePath">The path of the file to find.</param>
        /// <returns>The <see cref="AegisFileInfo"/> associated with the file, or null if not found.</returns>
        public AegisFileInfo Find(AegisVirtualFilePath filePath)
        {
            var node = this.GetNodeForDirectoryPath(filePath);

            return node?.Files.ContainsKey(filePath.FileName) == true
                ? node.Files[filePath.FileName]
                : null;
        }

        /// <summary>
        /// Executes a traversal of the <see cref="VirtualFileTree"/>.
        /// </summary>
        /// <param name="visitorImplementation">The visitor implementation to execute when visiting each node.</param>
        public void TraverseNodes(IVirtualFileTreeVisitor visitorImplementation)
        {
            var traversal = new Stack<VirtualFileTreeNode>();
            traversal.Push(this.Root);

            var visitedNodes = new HashSet<VirtualFileTreeNode>();

            while (traversal.Count > 0)
            {
                var curNode = traversal.Pop();

                if (visitedNodes.Contains(curNode))
                {
                    // Pre-order traveral visit was previously done.
                    visitorImplementation.OnPostOrderVisit(
                        new ReadOnlySpan<AegisFileInfo>(curNode.Files.Values.ToArray()));
                }
                else
                {
                    visitorImplementation.OnPreOrderVisit(
                        new ReadOnlySpan<AegisFileInfo>(curNode.Files.Values.ToArray()));
                    visitedNodes.Add(curNode);

                    // We'll revisit this node when we're done with its children for the post-order traversal.
                    traversal.Push(curNode);

                    // We'll visit all the node's children too, starting with the leftmost.
                    foreach (var child in curNode.Children.Values.Reverse())
                    {
                        traversal.Push(child);
                    }
                }
            }
        }

        /// <summary>
        /// Retrieves the node at the directory path for the given <see cref="AegisVirtualFilePath"/>.
        /// </summary>
        /// <param name="filePath">The virtual file path information.</param>
        /// <param name="createIfNotExists">Flag indicating we should create the node (including all intermediate nodes) if it doesn't exist.</param>
        /// <returns>The node at the directory path specified, or null if it doesn't exist and <paramref name="createIfNotExists"/> is not specified.</returns>
        /// <remarks>The node may not actually contain the file.</remarks>
        private VirtualFileTreeNode GetNodeForDirectoryPath(AegisVirtualFilePath filePath, bool createIfNotExists = false)
        {
            var curNode = this.Root;

            int dirDepth;
            for (dirDepth = 0; dirDepth < filePath.DirectoryPathComponents.Length; dirDepth++)
            {
                var dir = filePath.DirectoryPathComponents[dirDepth];

                if (!curNode.Children.ContainsKey(dir))
                {
                    if (createIfNotExists)
                    {
                        var newChild = new VirtualFileTreeNode(dir, curNode);
                        curNode.Children.Add(dir, newChild);
                    }
                    else
                    {
                        break;
                    }
                }

                curNode = curNode.Children[dir];
            }

            return dirDepth == filePath.DirectoryPathComponents.Length
                ? curNode
                : null;
        }

        /// <summary>
        /// A node (basically a directory) in the virtual tree of files contained in the archive.
        /// </summary>
        private class VirtualFileTreeNode
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="VirtualFileTreeNode"/> class.
            /// </summary>
            /// <param name="directoryName">The name of the directory at this node.</param>
            /// <param name="parent">The parent of this node.</param>
            public VirtualFileTreeNode(string directoryName, VirtualFileTreeNode parent)
            {
                this.DirectoryName = directoryName;
                this.Parent = parent;
            }

            /// <summary>
            /// Gets the name of the directory at this node.
            /// </summary>
            public string DirectoryName { get; }

            /// <summary>
            /// Gets the parent of this node.
            /// </summary>
            public VirtualFileTreeNode Parent { get; }

            /// <summary>
            /// Gets the children of this node.
            /// </summary>
            public SortedDictionary<string, VirtualFileTreeNode> Children { get; }
                = new SortedDictionary<string, VirtualFileTreeNode>(StringComparer.OrdinalIgnoreCase);

            /// <summary>
            /// Gets the files at this node.
            /// </summary>
            public SortedList<string, AegisFileInfo> Files { get; }
                = new SortedList<string, AegisFileInfo>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
