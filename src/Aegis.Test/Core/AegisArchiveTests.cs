using Microsoft.VisualStudio.TestTools.UnitTesting;

using Aegis.Core;
using Aegis.Core.FileSystem;
using Aegis.Models;

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
    [DataRow(ArchiveTestHelpers.SampleFiles.SimpleTextFilePath, SecretKind.Password, DisplayName = "Basic Scenario: Simple Text File, Password Secret")]
    [DataRow(ArchiveTestHelpers.SampleFiles.SimpleImageFilePath, SecretKind.Password, DisplayName = "Basic Scenario: Simple Image File, Password Secret")]
    [DataRow(ArchiveTestHelpers.SampleFiles.SimpleTextFilePath, SecretKind.RsaKeyFromCertificate, DisplayName = "Basic Scenario: Simple Text File, RSA Certificate Secret")]
    [DataRow(ArchiveTestHelpers.SampleFiles.SimpleImageFilePath, SecretKind.RsaKeyFromCertificate, DisplayName = "Basic Scenario: Simple Image File, RSA Certificate Secret")]
    public void TestArchive_BasicScenario(string referenceFilePath, SecretKind secretKind)
    {
        // 1. Create a new archive
        // 2. Add a file to it
        // 3. Extract the file, ensure it is identical to the original
        // 4. Close the archive
        // 5. Reopen the archive and decrypt
        // 6. Extract the file again, ensure it is identical to the original

        AegisFileInfo file = null;

        using (var archive = ArchiveTestHelpers.CreateNewEmptyArchive(this.WorkingDirectory, secretKind))
        {
            // The achive should be created in unlocked state.
            Assert.IsFalse(archive.IsLocked);

            using var referenceFileStream = File.OpenRead(referenceFilePath);

            file = archive.PutFile(
                new AegisVirtualFilePath("text.txt"),
                referenceFileStream);

            var extractPath1 = Path.Combine(this.WorkingDirectory, "Text_out1.txt");
            using (var extractStream = File.OpenWrite(extractPath1))
            {
                archive.ExtractFile(file, extractStream);
            }

            ArchiveTestHelpers.CompareFileToReference(
                referenceFilePath: referenceFilePath,
                testFilePath: extractPath1,
                deleteTestFile: true);
        }

        using var reopenedArchive = AegisArchive.Load(
            ArchiveTestHelpers.GetTestArchiveFileSettings(this.WorkingDirectory));

        // The archive is open but still locked. Make sure the flags are set accordingly.
        Assert.IsTrue(reopenedArchive.IsLocked);
        Assert.ThrowsException<ArchiveLockedException>(() => reopenedArchive.GetFileInfo(file.FileId));

        reopenedArchive.Unlock(TestSecrets.GetDefaultUserSecret(secretKind));
        Assert.IsFalse(reopenedArchive.IsLocked);

        var extractPath2 = Path.Combine(this.WorkingDirectory, "Text_out2.txt");
        using (var extractStream = File.OpenWrite(extractPath2))
        {
            reopenedArchive.ExtractFile(file, extractStream);
        }

        ArchiveTestHelpers.CompareFileToReference(
            referenceFilePath: referenceFilePath,
            testFilePath: extractPath2,
            deleteTestFile: true);
    }

    [TestMethod]
    [Description("Tests adding and extracting a large file.")]
    public void TestArchive_LargeFile()
    {
        // The size of the large file to create -- 10 mb
        const int LargeFileSizeInKB = 10_240;

        var largeFilePath = Path.Combine(this.WorkingDirectory, "LargeFile.bin");

        using (var file = File.OpenWrite(largeFilePath))
        {
            var randomKB = new byte[1024];
            Random.Shared.NextBytes(randomKB);

            for (int i = 0; i < LargeFileSizeInKB; i++)
            {
                file.Write(randomKB);
            }
        }

        // Run the basic scenario test on the large file.
        this.TestArchive_BasicScenario(largeFilePath, SecretKind.Password);

        // Clean up the large file.
        File.Delete(largeFilePath);
    }

    [TestMethod]
    [Description("Tests deleting a file from the archive.")]
    public void TestArchive_DeleteFile()
    {
        // 1. Create a new archive, add a file to it
        // 2. Delete the file from the archive, ensure it's gone
        // 3. Close and reopen the archive
        // 4. Make sure the file is still deleted

        AegisFileInfo file = null;

        using (var archive = ArchiveTestHelpers.CreateNewEmptyArchive(this.WorkingDirectory, SecretKind.Password))
        {
            // The achive should be created in unlocked state.
            Assert.IsFalse(archive.IsLocked);

            file = archive.PutFile(
                new AegisVirtualFilePath("text.txt"),
                File.OpenRead(ArchiveTestHelpers.SampleFiles.SimpleTextFilePath));

            archive.RemoveFile(file.Path);

            Assert.IsNull(
                archive.GetFileInfo(file.Path),
                "The deleted file was found in archive, searching with v-path!");
            Assert.IsNull(
                archive.GetFileInfo(file.FileId),
                "The deleted file was found in archive, searching with file ID!");
        }

        using var reopenedArchive = AegisArchive.Load(
            ArchiveTestHelpers.GetTestArchiveFileSettings(this.WorkingDirectory));
        reopenedArchive.Unlock(TestSecrets.GetDefaultUserSecret(SecretKind.Password));

        Assert.IsNull(
            reopenedArchive.GetFileInfo(file.Path),
            "The deleted file was found in reopened archive, searching with v-path!");
        Assert.IsNull(
            reopenedArchive.GetFileInfo(file.FileId),
            "The deleted file was found in reopened archive, searching with file ID!");
    }

    [TestMethod]
    [Description("Tests an arhcive containing multiple files.")]
    public void TestArchive_MultipleFiles()
    {
        // TODO: Implement this test
    }

    [TestMethod]
    [Description("Tests authorizing a second key on an archive.")]
    public void TestArchive_AuthorizeSecondKey()
    {
        // TODO: Implement this test
    }
}
