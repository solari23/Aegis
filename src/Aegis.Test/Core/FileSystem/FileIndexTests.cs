using Aegis.Core;
using Aegis.Core.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aegis.Test.Core.FileSystem;

[TestClass]
public class FileIndexTests
{
    [TestMethod]
    public void TestPreventsDoubleAdd()
    {
        var index = new FileIndex();

        var testFileInfo = FileSystemTestingHelpers.CreateFileInfo("/dir/foo.txt");
        index.Add(testFileInfo);
        Assert.IsNotNull(index.GetFileInfo(testFileInfo.Path));

        // Attempt to re-add the same file. Should throw.
        Assert.ThrowsException<AegisInternalErrorException>(() => index.Add(testFileInfo));

        // Remove the file, then add it back. Should not throw.
        index.Remove(testFileInfo.Path);
        index.Add(testFileInfo);

        // Attempt to add another file with the same virtual path. Should throw.
        var fileInfoSamePath = FileSystemTestingHelpers.CreateFileInfo(testFileInfo.Path.ToString());
        Assert.ThrowsException<AegisInternalErrorException>(() => index.Add(fileInfoSamePath));

        // Attempt to add another file with the same ID, different virtual path. Should throw.
        var fileInfoSameId = FileSystemTestingHelpers.CreateFileInfo("/dir/bar.txt", testFileInfo.FileId);
        Assert.ThrowsException<AegisInternalErrorException>(() => index.Add(fileInfoSameId));
    }
}
