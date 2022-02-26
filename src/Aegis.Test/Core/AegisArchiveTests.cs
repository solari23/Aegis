using Microsoft.VisualStudio.TestTools.UnitTesting;

using Aegis.Core;
using Aegis.Core.FileSystem;

namespace Aegis.Test.Core;

[TestClass]
public class AegisArchiveTests
{
    public TestContext TestContext { get; set; }

    private string WorkingDirectory { get; set; }

    [TestInitialize]
    public void TestInitialize()
    {
        this.WorkingDirectory = Path.Combine(
            this.TestContext.TestRunDirectory,
            $"{this.TestContext.TestName}_Work");
        Directory.CreateDirectory(this.WorkingDirectory);
    }

    [TestMethod]
    [Description("Tests adding and extracting a single file.")]
    public void TestArchive_BasicScenario()
    {
        // 1. Create a new archive
        // 2. Add a file to it
        // 3. Extract the file, ensure it is identical to the original
        // 4. Close the archive
        // 5. Reopen the archive and decrypt
        // 6. Extract the file again, ensure it is identical to the original

        AegisFileInfo file = null;

        using (var archive = ArchiveTestHelpers.CreateNewEmptyArchive(this.WorkingDirectory))
        {
            // The achive should be created in unlocked state.
            Assert.IsFalse(archive.IsLocked);

            file = archive.PutFile(
                new AegisVirtualFilePath("text.txt"),
                File.OpenRead(ArchiveTestHelpers.SampleFiles.SimpleTextFilePath));

            var extractPath1 = Path.Combine(this.WorkingDirectory, "Text_out1.txt");
            using (var extractStream = File.OpenWrite(extractPath1))
            {
                archive.ExtractFile(file, extractStream);
            }

            ArchiveTestHelpers.CompareFileToReference(
                referenceFilePath: ArchiveTestHelpers.SampleFiles.SimpleTextFilePath,
                testFilePath: extractPath1,
                deleteTestFile: true);
        }

        using var reopenedArchive = AegisArchive.Load(
            ArchiveTestHelpers.GetTestArchiveFileSettings(this.WorkingDirectory));

        // The archive is open but still locked. Make sure the flags are set accordingly.
        Assert.IsTrue(reopenedArchive.IsLocked);
        Assert.ThrowsException<ArchiveLockedException>(() => reopenedArchive.GetFileInfo(file.FileId));

        reopenedArchive.Unlock(ArchiveTestHelpers.DefaultPasswordUserSecret);
        Assert.IsFalse(reopenedArchive.IsLocked);

        var extractPath2 = Path.Combine(this.WorkingDirectory, "Text_out2.txt");
        using (var extractStream = File.OpenWrite(extractPath2))
        {
            reopenedArchive.ExtractFile(file, extractStream);
        }

        ArchiveTestHelpers.CompareFileToReference(
            referenceFilePath: ArchiveTestHelpers.SampleFiles.SimpleTextFilePath,
            testFilePath: extractPath2,
            deleteTestFile: true);
    }

    [TestMethod]
    public void TestArchive_DeleteFile()
    {
        // TODO
    }
}
