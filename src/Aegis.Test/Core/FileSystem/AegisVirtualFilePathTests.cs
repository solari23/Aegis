using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using AegisVirtualFilePath = Aegis.Core.FileSystem.AegisVirtualFilePath;

namespace Aegis.Test.Core.FileSystem
{
    [TestClass]
    public class AegisVirtualFilePathTests
    {
        [TestMethod]
        public void TestConstruction_BasicChecks()
        {
            AegisVirtualFilePath vpath;

            // Just a bare file name.
            vpath = new AegisVirtualFilePath(@"foo.ags");
            Assert.AreEqual("foo.ags", vpath.FileName);
            Assert.AreEqual("/foo.ags", vpath.FullPath);
            Assert.AreEqual(0, vpath.DirectoryPath.Components.Length);
            Assert.AreEqual("/", vpath.DirectoryPath.FullPath);
            Assert.AreEqual("<root>", vpath.DirectoryPath.DisplayName);

            // Rooted file name.
            vpath = new AegisVirtualFilePath(@"/foo.ags");
            Assert.AreEqual("foo.ags", vpath.FileName);
            Assert.AreEqual("/foo.ags", vpath.FullPath);
            Assert.AreEqual(0, vpath.DirectoryPath.Components.Length);
            Assert.AreEqual("/", vpath.DirectoryPath.FullPath);
            Assert.AreEqual("<root>", vpath.DirectoryPath.DisplayName);

            // Rooted file name using non-canonical path separator.
            vpath = new AegisVirtualFilePath(@"\foo.ags");
            Assert.AreEqual("foo.ags", vpath.FileName);
            Assert.AreEqual("/foo.ags", vpath.FullPath);
            Assert.AreEqual(0, vpath.DirectoryPath.Components.Length);
            Assert.AreEqual("/", vpath.DirectoryPath.FullPath);
            Assert.AreEqual("<root>", vpath.DirectoryPath.DisplayName);

            // File with a directory path.
            vpath = new AegisVirtualFilePath(@"dir/foo.ags");
            Assert.AreEqual("foo.ags", vpath.FileName);
            Assert.AreEqual("/dir/foo.ags", vpath.FullPath);
            Assert.AreEqual(1, vpath.DirectoryPath.Components.Length);
            Assert.AreEqual("dir", vpath.DirectoryPath.Components[0]);
            Assert.AreEqual("/dir", vpath.DirectoryPath.FullPath);
            Assert.AreEqual("dir", vpath.DirectoryPath.DisplayName);

            // File with a rooted directory path.
            vpath = new AegisVirtualFilePath(@"/dir/foo.ags");
            Assert.AreEqual("foo.ags", vpath.FileName);
            Assert.AreEqual("/dir/foo.ags", vpath.FullPath);
            Assert.AreEqual(1, vpath.DirectoryPath.Components.Length);
            Assert.AreEqual("dir", vpath.DirectoryPath.Components[0]);
            Assert.AreEqual("/dir", vpath.DirectoryPath.FullPath);
            Assert.AreEqual("dir", vpath.DirectoryPath.DisplayName);

            // File with a rooted directory path, using non-canonical path separator.
            vpath = new AegisVirtualFilePath(@"\dir\foo.ags");
            Assert.AreEqual("foo.ags", vpath.FileName);
            Assert.AreEqual("/dir/foo.ags", vpath.FullPath);
            Assert.AreEqual(1, vpath.DirectoryPath.Components.Length);
            Assert.AreEqual("dir", vpath.DirectoryPath.Components[0]);
            Assert.AreEqual("/dir", vpath.DirectoryPath.FullPath);
            Assert.AreEqual("dir", vpath.DirectoryPath.DisplayName);

            // File with a rooted directory path, using mixed path separators (1).
            vpath = new AegisVirtualFilePath(@"/dir\foo.ags");
            Assert.AreEqual("foo.ags", vpath.FileName);
            Assert.AreEqual("/dir/foo.ags", vpath.FullPath);
            Assert.AreEqual(1, vpath.DirectoryPath.Components.Length);
            Assert.AreEqual("dir", vpath.DirectoryPath.Components[0]);
            Assert.AreEqual("/dir", vpath.DirectoryPath.FullPath);
            Assert.AreEqual("dir", vpath.DirectoryPath.DisplayName);

            // File with a rooted directory path, using mixed path separators (2).
            vpath = new AegisVirtualFilePath(@"\dir/foo.ags");
            Assert.AreEqual("foo.ags", vpath.FileName);
            Assert.AreEqual("/dir/foo.ags", vpath.FullPath);
            Assert.AreEqual(1, vpath.DirectoryPath.Components.Length);
            Assert.AreEqual("dir", vpath.DirectoryPath.Components[0]);
            Assert.AreEqual("/dir", vpath.DirectoryPath.FullPath);
            Assert.AreEqual("dir", vpath.DirectoryPath.DisplayName);

            // Longer path.
            vpath = new AegisVirtualFilePath(@"/dir1/dir2/foo.ags");
            Assert.AreEqual("foo.ags", vpath.FileName);
            Assert.AreEqual("/dir1/dir2/foo.ags", vpath.FullPath);
            Assert.AreEqual(2, vpath.DirectoryPath.Components.Length);
            Assert.AreEqual("dir1", vpath.DirectoryPath.Components[0]);
            Assert.AreEqual("dir2", vpath.DirectoryPath.Components[1]);
            Assert.AreEqual("/dir1/dir2", vpath.DirectoryPath.FullPath);
            Assert.AreEqual("dir2", vpath.DirectoryPath.DisplayName);
        }

        [TestMethod]
        public void TestConstruction_RejectEmptyPath()
        {
            // Check empty strings.
            Assert.ThrowsException<ArgumentNullException>(() => new AegisVirtualFilePath(null));
            Assert.ThrowsException<ArgumentNullException>(() => new AegisVirtualFilePath(string.Empty));
            Assert.ThrowsException<ArgumentNullException>(() => new AegisVirtualFilePath(" "));
            Assert.ThrowsException<ArgumentNullException>(() => new AegisVirtualFilePath("\n"));
            Assert.ThrowsException<ArgumentNullException>(() => new AegisVirtualFilePath("\t"));

            // Check paths containing only separators.
            Assert.ThrowsException<ArgumentException>(() => new AegisVirtualFilePath(@"/"));
            Assert.ThrowsException<ArgumentException>(() => new AegisVirtualFilePath(@"//"));
            Assert.ThrowsException<ArgumentException>(() => new AegisVirtualFilePath(@"\"));
            Assert.ThrowsException<ArgumentException>(() => new AegisVirtualFilePath(@"\\"));
            Assert.ThrowsException<ArgumentException>(() => new AegisVirtualFilePath(@"\"));
            Assert.ThrowsException<ArgumentException>(() => new AegisVirtualFilePath(@"\/"));
            Assert.ThrowsException<ArgumentException>(() => new AegisVirtualFilePath(@"/\"));
        }

        [TestMethod]
        public void TestEquality_BasicChecks()
        {
            AegisVirtualFilePath vpath1;
            AegisVirtualFilePath vpath2;

            // Bare file name -- compare to self.
            vpath1 = new AegisVirtualFilePath("foo.ags");
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(vpath1 == vpath1);
            Assert.IsFalse(vpath1 != vpath1);
            Assert.IsTrue(vpath1.Equals(vpath1));
#pragma warning restore CS1718 // Comparison made to same variable

            // Matching bare file names -- compare to each other.
            vpath1 = new AegisVirtualFilePath("foo.ags");
            vpath2 = new AegisVirtualFilePath("foo.ags");
            Assert.IsTrue(vpath1 == vpath2);
            Assert.IsTrue(vpath2 == vpath1);
            Assert.IsFalse(vpath1 != vpath2);
            Assert.IsFalse(vpath2 != vpath1);
            Assert.IsTrue(vpath1.Equals(vpath2));
            Assert.IsTrue(vpath2.Equals(vpath1));

            // Mismatching bare file names -- compare to each other.
            vpath1 = new AegisVirtualFilePath("foo.ags");
            vpath2 = new AegisVirtualFilePath("bar.ags");
            Assert.IsFalse(vpath1 == vpath2);
            Assert.IsFalse(vpath2 == vpath1);
            Assert.IsTrue(vpath1 != vpath2);
            Assert.IsTrue(vpath2 != vpath1);
            Assert.IsFalse(vpath1.Equals(vpath2));
            Assert.IsFalse(vpath2.Equals(vpath1));

            // File with path -- compare to self.
            vpath1 = new AegisVirtualFilePath("/dir/foo.ags");
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(vpath1 == vpath1);
            Assert.IsFalse(vpath1 != vpath1);
            Assert.IsTrue(vpath1.Equals(vpath1));
#pragma warning restore CS1718 // Comparison made to same variable

            // Matching files with paths -- compare to each other.
            vpath1 = new AegisVirtualFilePath("/dir/foo.ags");
            vpath2 = new AegisVirtualFilePath("/dir/foo.ags");
            Assert.IsTrue(vpath1 == vpath2);
            Assert.IsTrue(vpath2 == vpath1);
            Assert.IsFalse(vpath1 != vpath2);
            Assert.IsFalse(vpath2 != vpath1);
            Assert.IsTrue(vpath1.Equals(vpath2));
            Assert.IsTrue(vpath2.Equals(vpath1));

            // Mismatching files with paths -- compare to each other.
            vpath1 = new AegisVirtualFilePath("/dir/foo.ags");
            vpath2 = new AegisVirtualFilePath("/wat/foo.ags");
            Assert.IsFalse(vpath1 == vpath2);
            Assert.IsFalse(vpath2 == vpath1);
            Assert.IsTrue(vpath1 != vpath2);
            Assert.IsTrue(vpath2 != vpath1);
            Assert.IsFalse(vpath1.Equals(vpath2));
            Assert.IsFalse(vpath2.Equals(vpath1));
        }

        [TestMethod]
        public void TestEquality_NullComparisonChecks()
        {
            AegisVirtualFilePath vpath1;
            AegisVirtualFilePath vpath2;

            // Check equality for just one null reference.
            vpath1 = null;
            Assert.AreEqual(null, vpath1);

            // Both references are null.
            vpath1 = null;
            vpath2 = null;
            Assert.IsTrue(vpath1 == vpath2);
            Assert.IsTrue(vpath2 == vpath1);
            Assert.IsFalse(vpath1 != vpath2);
            Assert.IsFalse(vpath2 != vpath1);

            // Non-null reference compared to null.
            vpath1 = new AegisVirtualFilePath("foo.ags");
            Assert.IsFalse(vpath1 == null);
            Assert.IsFalse(null == vpath1);
            Assert.IsFalse(vpath1.Equals(null));
            Assert.IsFalse(vpath1.Equals((object)null));
        }

        [TestMethod]
        public void TestEquality_CaseInsensitivity()
        {
            AegisVirtualFilePath vpath1 = null;
            AegisVirtualFilePath vpath2 = null;

            // Bare file names.
            vpath1 = new AegisVirtualFilePath("foo.ags");
            vpath2 = new AegisVirtualFilePath("FOO.AGS");
            Assert.IsTrue(vpath1 == vpath2);
            Assert.IsTrue(vpath2 == vpath1);
            Assert.IsFalse(vpath1 != vpath2);
            Assert.IsFalse(vpath2 != vpath1);
            Assert.IsTrue(vpath1.Equals(vpath2));
            Assert.IsTrue(vpath2.Equals(vpath1));

            // Files with paths -- file name casing differs.
            vpath1 = new AegisVirtualFilePath("/dir/foo.ags");
            vpath2 = new AegisVirtualFilePath("/dir/FOO.AGS");
            Assert.IsTrue(vpath1 == vpath2);
            Assert.IsTrue(vpath2 == vpath1);
            Assert.IsFalse(vpath1 != vpath2);
            Assert.IsFalse(vpath2 != vpath1);
            Assert.IsTrue(vpath1.Equals(vpath2));
            Assert.IsTrue(vpath2.Equals(vpath1));

            // Files with paths -- directory casing differs.
            vpath1 = new AegisVirtualFilePath("/dir/foo.ags");
            vpath2 = new AegisVirtualFilePath("/DIR/foo.ags");
            Assert.IsTrue(vpath1 == vpath2);
            Assert.IsTrue(vpath2 == vpath1);
            Assert.IsFalse(vpath1 != vpath2);
            Assert.IsFalse(vpath2 != vpath1);
            Assert.IsTrue(vpath1.Equals(vpath2));
            Assert.IsTrue(vpath2.Equals(vpath1));
        }

        [TestMethod]
        public void TestComparison_BasicChecks()
        {
            AegisVirtualFilePath vpath1;
            AegisVirtualFilePath vpath2;

            // Bare file name -- compare to self.
            vpath1 = new AegisVirtualFilePath("foo.ags");
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(vpath1 <= vpath1);
            Assert.IsTrue(vpath1 >= vpath1);
            Assert.IsFalse(vpath1 < vpath1);
            Assert.IsFalse(vpath1 > vpath1);
            Assert.AreEqual(0, vpath1.CompareTo(vpath1));
#pragma warning restore CS1718 // Comparison made to same variable

            // Identical bare file names -- compare to each other.
            vpath1 = new AegisVirtualFilePath("foo.ags");
            vpath2 = new AegisVirtualFilePath("foo.ags");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsTrue(vpath2 <= vpath1);
            Assert.IsTrue(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsFalse(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsFalse(vpath2 > vpath1);
            Assert.AreEqual(0, vpath1.CompareTo(vpath2));
            Assert.AreEqual(0, vpath2.CompareTo(vpath1));

            // Mismatching bare file names -- compare to each other.
            vpath1 = new AegisVirtualFilePath("bar.ags");
            vpath2 = new AegisVirtualFilePath("foo.ags");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsFalse(vpath2 <= vpath1);
            Assert.IsFalse(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsTrue(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsTrue(vpath2 > vpath1);
            Assert.IsTrue(vpath1.CompareTo(vpath2) < 0);
            Assert.IsTrue(vpath2.CompareTo(vpath1) > 0);

            // File with path -- compare to self.
            vpath1 = new AegisVirtualFilePath("/dir/foo.ags");
#pragma warning disable CS1718 // Comparison made to same variable
            Assert.IsTrue(vpath1 <= vpath1);
            Assert.IsTrue(vpath1 >= vpath1);
            Assert.IsFalse(vpath1 < vpath1);
            Assert.IsFalse(vpath1 > vpath1);
            Assert.AreEqual(0, vpath1.CompareTo(vpath1));
#pragma warning restore CS1718 // Comparison made to same variable

            // Identical files with paths -- compare to each other.
            vpath1 = new AegisVirtualFilePath("/dir/foo.ags");
            vpath2 = new AegisVirtualFilePath("/dir/foo.ags");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsTrue(vpath2 <= vpath1);
            Assert.IsTrue(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsFalse(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsFalse(vpath2 > vpath1);
            Assert.AreEqual(0, vpath1.CompareTo(vpath2));
            Assert.AreEqual(0, vpath2.CompareTo(vpath1));

            // Mismatching files with paths -- file names mismatch -- compare to each other.
            vpath1 = new AegisVirtualFilePath("/dir/bar.ags");
            vpath2 = new AegisVirtualFilePath("/dir/foo.ags");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsFalse(vpath2 <= vpath1);
            Assert.IsFalse(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsTrue(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsTrue(vpath2 > vpath1);
            Assert.IsTrue(vpath1.CompareTo(vpath2) < 0);
            Assert.IsTrue(vpath2.CompareTo(vpath1) > 0);

            // Mismatching files with paths -- directory names mismatch -- compare to each other.
            vpath1 = new AegisVirtualFilePath("/dir/foo.ags");
            vpath2 = new AegisVirtualFilePath("/wat/foo.ags");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsFalse(vpath2 <= vpath1);
            Assert.IsFalse(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsTrue(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsTrue(vpath2 > vpath1);
            Assert.IsTrue(vpath1.CompareTo(vpath2) < 0);
            Assert.IsTrue(vpath2.CompareTo(vpath1) > 0);
        }

        [TestMethod]
        public void TestComparison_NullComparisonChecks()
        {
            AegisVirtualFilePath vpath1;
            AegisVirtualFilePath vpath2;

            // Both references are null.
            vpath1 = null;
            vpath2 = null;
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsTrue(vpath2 <= vpath1);
            Assert.IsTrue(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsFalse(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsFalse(vpath2 > vpath1);

            // Non-null reference compared to null.
            vpath1 = new AegisVirtualFilePath("foo.ags");
            Assert.IsFalse(vpath1 <= null);
            Assert.IsTrue(null <= vpath1);
            Assert.IsTrue(vpath1 >= null);
            Assert.IsFalse(null >= vpath1);
            Assert.IsFalse(vpath1 < null);
            Assert.IsTrue(null < vpath1);
            Assert.IsTrue(vpath1 > null);
            Assert.IsFalse(null > vpath1);
            Assert.IsTrue(vpath1.CompareTo(null) > 0);
        }

        [TestMethod]
        public void TestComparison_CaseInsensitivity()
        {
            AegisVirtualFilePath vpath1;
            AegisVirtualFilePath vpath2;

            // Matching bare file names with case difference.
            vpath1 = new AegisVirtualFilePath("foo.ags");
            vpath2 = new AegisVirtualFilePath("FOO.AGS");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsTrue(vpath2 <= vpath1);
            Assert.IsTrue(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsFalse(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsFalse(vpath2 > vpath1);
            Assert.AreEqual(0, vpath1.CompareTo(vpath2));
            Assert.AreEqual(0, vpath2.CompareTo(vpath1));

            // Matching files with paths, file names have case difference.
            vpath1 = new AegisVirtualFilePath("/dir/foo.ags");
            vpath2 = new AegisVirtualFilePath("/dir/FOO.AGS");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsTrue(vpath2 <= vpath1);
            Assert.IsTrue(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsFalse(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsFalse(vpath2 > vpath1);
            Assert.AreEqual(0, vpath1.CompareTo(vpath2));
            Assert.AreEqual(0, vpath2.CompareTo(vpath1));

            // Matching files with paths, directory names have case difference.
            vpath1 = new AegisVirtualFilePath("/dir/foo.ags");
            vpath2 = new AegisVirtualFilePath("/DIR/foo.ags");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsTrue(vpath2 <= vpath1);
            Assert.IsTrue(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsFalse(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsFalse(vpath2 > vpath1);
            Assert.AreEqual(0, vpath1.CompareTo(vpath2));
            Assert.AreEqual(0, vpath2.CompareTo(vpath1));
        }

        [TestMethod]
        public void TestComparison_DifferingPathLengths()
        {
            AegisVirtualFilePath vpath1;
            AegisVirtualFilePath vpath2;

            // One directory is parent to the other (path1 preceeds path2).
            vpath1 = new AegisVirtualFilePath("/dir1/foo.ags");
            vpath2 = new AegisVirtualFilePath("/dir1/dir2/foo.ags");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsFalse(vpath2 <= vpath1);
            Assert.IsFalse(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsTrue(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsTrue(vpath2 > vpath1);
            Assert.IsTrue(vpath1.CompareTo(vpath2) < 0);
            Assert.IsTrue(vpath2.CompareTo(vpath1) > 0);

            // Directory paths are siblings (path1 preceeds path2).
            vpath1 = new AegisVirtualFilePath("/dir1/dir2/foo.ags");
            vpath2 = new AegisVirtualFilePath("/dir1/dir3/foo.ags");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsFalse(vpath2 <= vpath1);
            Assert.IsFalse(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsTrue(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsTrue(vpath2 > vpath1);
            Assert.IsTrue(vpath1.CompareTo(vpath2) < 0);
            Assert.IsTrue(vpath2.CompareTo(vpath1) > 0);

            // One directory name is shorter than the other (path1 preceeds path2).
            vpath1 = new AegisVirtualFilePath("/d/foo.ags");
            vpath2 = new AegisVirtualFilePath("/dir/foo.ags");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsFalse(vpath2 <= vpath1);
            Assert.IsFalse(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsTrue(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsTrue(vpath2 > vpath1);
            Assert.IsTrue(vpath1.CompareTo(vpath2) < 0);
            Assert.IsTrue(vpath2.CompareTo(vpath1) > 0);

            // Directory paths are unrelated (path1 preceeds path2).
            vpath1 = new AegisVirtualFilePath("/dir1/foo.ags");
            vpath2 = new AegisVirtualFilePath("/dir2/dir3/foo.ags");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsFalse(vpath2 <= vpath1);
            Assert.IsFalse(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsTrue(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsTrue(vpath2 > vpath1);
            Assert.IsTrue(vpath1.CompareTo(vpath2) < 0);
            Assert.IsTrue(vpath2.CompareTo(vpath1) > 0);

            // Bare file vs path -- similar to being in parent directory (path1 preceeds path2).
            vpath1 = new AegisVirtualFilePath("foo.ags");
            vpath2 = new AegisVirtualFilePath("/dir1/foo.ags");
            Assert.IsTrue(vpath1 <= vpath2);
            Assert.IsFalse(vpath2 <= vpath1);
            Assert.IsFalse(vpath1 >= vpath2);
            Assert.IsTrue(vpath2 >= vpath1);
            Assert.IsTrue(vpath1 < vpath2);
            Assert.IsFalse(vpath2 < vpath1);
            Assert.IsFalse(vpath1 > vpath2);
            Assert.IsTrue(vpath2 > vpath1);
            Assert.IsTrue(vpath1.CompareTo(vpath2) < 0);
            Assert.IsTrue(vpath2.CompareTo(vpath1) > 0);
        }
    }
}
