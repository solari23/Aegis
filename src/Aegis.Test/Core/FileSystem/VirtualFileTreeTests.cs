using System;
using System.Collections.Generic;

using Aegis.Core.FileSystem;
using Aegis.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aegis.Test.Core.FileSystem
{
    [TestClass]
    public class VirtualFileTreeTests
    {
        [TestMethod]
        public void TestAddRemove_FileAtRoot()
        {
            VirtualFileTreeTestVisitor visitor;

            var tree = new VirtualFileTree();
            var rootFile1 = CreateFileInfo("foo.txt");
            var rootFile2 = CreateFileInfo("bar.txt");

            // Initial state -- empty tree.
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(0, visitor.PreOrderHits);
            Assert.AreEqual(0, visitor.PreOrderVisitFiles.Count);
            Assert.AreEqual(0, visitor.PostOrderHits);
            Assert.AreEqual(0, visitor.PostOrderVisitFiles.Count);

            // Add a file to the root, make sure visitor sees it.
            tree.Add(rootFile1);
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(1, visitor.PreOrderHits);
            CheckFileOrder(visitor.PreOrderVisitFiles, rootFile1);
            Assert.AreEqual(1, visitor.PostOrderHits);
            CheckFileOrder(visitor.PostOrderVisitFiles, rootFile1);

            // Remove the file, make sure the tree is back in the empty state.
            tree.Remove(rootFile1.Path);
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(0, visitor.PreOrderHits);
            Assert.AreEqual(0, visitor.PreOrderVisitFiles.Count);
            Assert.AreEqual(0, visitor.PostOrderHits);
            Assert.AreEqual(0, visitor.PostOrderVisitFiles.Count);

            // Add both files to the root, visitor should see them in alphabetical order.
            tree.Add(rootFile1);
            tree.Add(rootFile2);
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(1, visitor.PreOrderHits);
            CheckFileOrder(visitor.PreOrderVisitFiles, rootFile2, rootFile1);
            Assert.AreEqual(1, visitor.PostOrderHits);
            CheckFileOrder(visitor.PostOrderVisitFiles, rootFile2, rootFile1);

            // Remove one file, make sure the other remains unaffected.
            tree.Remove(rootFile1.Path);
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(1, visitor.PreOrderHits);
            CheckFileOrder(visitor.PreOrderVisitFiles, rootFile2);
            Assert.AreEqual(1, visitor.PostOrderHits);
            CheckFileOrder(visitor.PostOrderVisitFiles, rootFile2);
        }

        [TestMethod]
        public void TestAddRemove_FilesInMultipleDirs()
        {
            VirtualFileTreeTestVisitor visitor;

            var tree = new VirtualFileTree();

            // The test files are spread into 4 directories:
            // (/)
            //  `- root.txt
            //  `- (dir1)
            //  |       `- d1.txt
            //  |       `- (nested)
            //  |                 `- d1nest.txt
            //  `- (dir2)
            //          `- d2.txt
            var rootFile = CreateFileInfo("root.txt");
            var dir1File = CreateFileInfo("/dir1/d1.txt");
            var nestedDir1File = CreateFileInfo("/dir1/nested/d1nest.txt");
            var dir2File = CreateFileInfo("/dir2/d2.txt");

            // Initial state -- empty tree.
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(0, visitor.PreOrderHits);
            Assert.AreEqual(0, visitor.PreOrderVisitFiles.Count);
            Assert.AreEqual(0, visitor.PostOrderHits);
            Assert.AreEqual(0, visitor.PostOrderVisitFiles.Count);

            // Add all the files -- check the pre/post order traversal
            // across the 4 directories.
            tree.Add(rootFile);
            tree.Add(dir1File);
            tree.Add(nestedDir1File);
            tree.Add(dir2File);
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(4, visitor.PreOrderHits);
            CheckFileOrder(visitor.PreOrderVisitFiles, rootFile, dir1File, nestedDir1File, dir2File);
            Assert.AreEqual(4, visitor.PostOrderHits);
            CheckFileOrder(visitor.PostOrderVisitFiles, nestedDir1File, dir1File, dir2File, rootFile);

            // Collapse the "nested" directory by removing that file.
            tree.Remove(nestedDir1File.Path);
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(3, visitor.PreOrderHits);
            CheckFileOrder(visitor.PreOrderVisitFiles, rootFile, dir1File, dir2File);
            Assert.AreEqual(3, visitor.PostOrderHits);
            CheckFileOrder(visitor.PostOrderVisitFiles, dir1File, dir2File, rootFile);

            // Collapse "dir1" altogether.
            tree.Remove(dir1File.Path);
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(2, visitor.PreOrderHits);
            CheckFileOrder(visitor.PreOrderVisitFiles, rootFile, dir2File);
            Assert.AreEqual(2, visitor.PostOrderHits);
            CheckFileOrder(visitor.PostOrderVisitFiles, dir2File, rootFile);

            // Re-add the "nested dir1" file without creating the directory at dir1.
            tree.Add(nestedDir1File);
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(3, visitor.PreOrderHits);
            CheckFileOrder(visitor.PreOrderVisitFiles, rootFile, nestedDir1File, dir2File);
            Assert.AreEqual(3, visitor.PostOrderHits);
            CheckFileOrder(visitor.PostOrderVisitFiles, nestedDir1File, dir2File, rootFile);

            // Add the file at dir1 back, we should be back to the fill tree again.
            tree.Add(dir1File);
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(4, visitor.PreOrderHits);
            CheckFileOrder(visitor.PreOrderVisitFiles, rootFile, dir1File, nestedDir1File, dir2File);
            Assert.AreEqual(4, visitor.PostOrderHits);
            CheckFileOrder(visitor.PostOrderVisitFiles, nestedDir1File, dir1File, dir2File, rootFile);

            // Remove the file at dir1, the directory shouldn't fully collapse because
            // of the file still in the nested directory.
            tree.Remove(dir1File.Path);
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(3, visitor.PreOrderHits);
            CheckFileOrder(visitor.PreOrderVisitFiles, rootFile, nestedDir1File, dir2File);
            Assert.AreEqual(3, visitor.PostOrderHits);
            CheckFileOrder(visitor.PostOrderVisitFiles, nestedDir1File, dir2File, rootFile);

            // Removing "nested" should now collapse all of "dir1".
            tree.Remove(nestedDir1File.Path);
            visitor = new VirtualFileTreeTestVisitor();
            tree.TraverseNodes(visitor);
            Assert.AreEqual(2, visitor.PreOrderHits);
            CheckFileOrder(visitor.PreOrderVisitFiles, rootFile, dir2File);
            Assert.AreEqual(2, visitor.PostOrderHits);
            CheckFileOrder(visitor.PostOrderVisitFiles, dir2File, rootFile);
        }

        private void CheckFileOrder(List<AegisFileInfo> fileList, params AegisFileInfo[] expectedFileOrder)
        {
            Assert.AreEqual(expectedFileOrder.Length, fileList.Count);

            for (int i = 0; i < fileList.Count; i++)
            {
                Assert.AreEqual(expectedFileOrder[i], fileList[i]);
            }
        }

        private AegisFileInfo CreateFileInfo(string virtualFilePath)
            => new AegisFileInfo(
                new AegisVirtualFilePath(virtualFilePath),
                new FileIndexEntry
                {
                    FileId = Guid.NewGuid(),
                    FilePath = virtualFilePath,
                    AddedTime = DateTimeOffset.UtcNow,
                    LastModifiedTime = DateTimeOffset.UtcNow,
                });

        private class VirtualFileTreeTestVisitor : IVirtualFileTreeVisitor
        {
            public int PreOrderHits { get; private set; } = 0;

            public List<AegisFileInfo> PreOrderVisitFiles { get; } = new List<AegisFileInfo>();

            public int PostOrderHits { get; private set; } = 0;

            public List<AegisFileInfo> PostOrderVisitFiles { get; } = new List<AegisFileInfo>();

            public void OnPreOrderVisit(ReadOnlySpan<AegisFileInfo> files)
            {
                this.PreOrderHits++;
                this.PreOrderVisitFiles.AddRange(files.ToArray());
            }

            public void OnPostOrderVisit(ReadOnlySpan<AegisFileInfo> files)
            {
                this.PostOrderHits++;
                this.PostOrderVisitFiles.AddRange(files.ToArray());
            }
        }
    }
}
