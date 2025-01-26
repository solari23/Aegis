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
        // 1. Create a new archive, add 2 files to it
        // 2. Extract the files, ensure they are identical to originals
        // 3. Close and reopen the archive
        // 4. Extract the files, ensure they are identical to originals
        // 5. Delete one of the files
        // 6. Ensure the other file is still in the archive

        using (var archive = ArchiveTestHelpers.CreateNewEmptyArchive(this.WorkingDirectory, SecretKind.Password))
        {
            // The achive should be created in unlocked state.
            Assert.IsFalse(archive.IsLocked);

            AegisFileInfo file1 = archive.PutFile(
                new AegisVirtualFilePath("txt/text.txt"),
                File.OpenRead(ArchiveTestHelpers.SampleFiles.SimpleTextFilePath));

            AegisFileInfo file2 = archive.PutFile(
                new AegisVirtualFilePath("img/img.jpg"),
                File.OpenRead(ArchiveTestHelpers.SampleFiles.SimpleImageFilePath));

            var extractPath1 = Path.Combine(this.WorkingDirectory, "Text_out1.txt");
            using (var extractStream = File.OpenWrite(extractPath1))
            {
                archive.ExtractFile(file1, extractStream);
            }

            ArchiveTestHelpers.CompareFileToReference(
                referenceFilePath: ArchiveTestHelpers.SampleFiles.SimpleTextFilePath,
                testFilePath: extractPath1,
                deleteTestFile: true);

            var extractPath2 = Path.Combine(this.WorkingDirectory, "Img_out1.jpg");
            using (var extractStream = File.OpenWrite(extractPath2))
            {
                archive.ExtractFile(file2, extractStream);
            }

            ArchiveTestHelpers.CompareFileToReference(
                referenceFilePath: ArchiveTestHelpers.SampleFiles.SimpleImageFilePath,
                testFilePath: extractPath2,
                deleteTestFile: true);
        }
   
        using (var reopenedArchive = AegisArchive.Load(
            ArchiveTestHelpers.GetTestArchiveFileSettings(this.WorkingDirectory)))
        {
            reopenedArchive.Unlock(TestSecrets.GetDefaultUserSecret(SecretKind.Password));

            AegisFileInfo file1 = reopenedArchive.GetFileInfo(
                new AegisVirtualFilePath("txt/text.txt"));
            AegisFileInfo file2 = reopenedArchive.GetFileInfo(
                new AegisVirtualFilePath("img/img.jpg"));

            var extractPath1 = Path.Combine(this.WorkingDirectory, "Text_out2.txt");
            using (var extractStream = File.OpenWrite(extractPath1))
            {
                reopenedArchive.ExtractFile(file1, extractStream);
            }

            ArchiveTestHelpers.CompareFileToReference(
                referenceFilePath: ArchiveTestHelpers.SampleFiles.SimpleTextFilePath,
                testFilePath: extractPath1,
                deleteTestFile: true);

            var extractPath2 = Path.Combine(this.WorkingDirectory, "Img_out2.jpg");
            using (var extractStream = File.OpenWrite(extractPath2))
            {
                reopenedArchive.ExtractFile(file2, extractStream);
            }

            ArchiveTestHelpers.CompareFileToReference(
                referenceFilePath: ArchiveTestHelpers.SampleFiles.SimpleImageFilePath,
                testFilePath: extractPath2,
                deleteTestFile: true);

            reopenedArchive.RemoveFile(file2.Path);
            Assert.IsNull(
                reopenedArchive.GetFileInfo(file2.Path),
                "The deleted file was found in archive, searching with v-path!");
            Assert.IsNull(
                reopenedArchive.GetFileInfo(file2.FileId),
                "The deleted file was found in archive, searching with file ID!");

            Assert.IsNotNull(
                reopenedArchive.GetFileInfo(file1.Path),
                "The retained file was not found after the other was deleted, searching with v-path!");
            Assert.IsNotNull(
                reopenedArchive.GetFileInfo(file1.FileId),
                "The retained file was not found after the other was deleted, searching with file ID!");

            var extractPath3 = Path.Combine(this.WorkingDirectory, "Text_out3.txt");
            using (var extractStream = File.OpenWrite(extractPath3))
            {
                reopenedArchive.ExtractFile(file1, extractStream);
            }

            ArchiveTestHelpers.CompareFileToReference(
                referenceFilePath: ArchiveTestHelpers.SampleFiles.SimpleTextFilePath,
                testFilePath: extractPath3,
                deleteTestFile: true);
        }
    }

    [TestMethod]
    [Description("Tests authorizing a second key on an archive.")]
    public void TestArchive_AuthorizeSecondKey()
    {
        // 1. Create a new archive with password
        // 2. Add a file to it
        // 3. Authorize a certificate key
        // 4. Close the archive
        // 5. Reopen the archive with the password
        // 6. Close the archive
        // 7. Reopen the archive with the certificate key
        // 8. Extract the file and verify it matches the original
        // 9. De-authorize the password
        // 10. Close the archive
        // 11. Attempt to open with password - should fail
        // 12. Reopen the archive with the certificate key.
        // 13. Extract the file and verify it matches the original

        AegisFileInfo file = null;

        // 1 -> 4
        using (var archive = ArchiveTestHelpers.CreateNewEmptyArchive(this.WorkingDirectory, SecretKind.Password))
        {
            file = archive.PutFile(
                new AegisVirtualFilePath("txt/text.txt"),
                File.OpenRead(ArchiveTestHelpers.SampleFiles.SimpleTextFilePath));

            archive.AuthorizeNewKey(
                TestSecrets.GetDefaultUserKeyAuthorizationParameters(SecretKind.RsaKeyFromCertificate));
        }

        // 5 -> 6
        using (var archive = AegisArchive.Load(ArchiveTestHelpers.GetTestArchiveFileSettings(this.WorkingDirectory)))
        {
            Assert.IsTrue(archive.IsLocked);

            archive.Unlock(TestSecrets.GetDefaultUserSecret(SecretKind.Password));
            Assert.IsFalse(archive.IsLocked);
            Assert.IsNotNull(archive.GetFileInfo(file.FileId));
        }

        // 7 -> 10
        using (var archive = AegisArchive.Load(ArchiveTestHelpers.GetTestArchiveFileSettings(this.WorkingDirectory)))
        {
            Assert.IsTrue(archive.IsLocked);
            archive.Unlock(TestSecrets.GetDefaultUserSecret(SecretKind.RsaKeyFromCertificate));
            Assert.IsFalse(archive.IsLocked);

            var extractPath = Path.Combine(this.WorkingDirectory, "Text_out1.txt");
            using (var extractStream = File.OpenWrite(extractPath))
            {
                archive.ExtractFile(file, extractStream);
            }
            ArchiveTestHelpers.CompareFileToReference(
                referenceFilePath: ArchiveTestHelpers.SampleFiles.SimpleTextFilePath,
                testFilePath: extractPath,
                deleteTestFile: true);

            var passwordAuthorizationRecord = archive
                .GetUserKeyAuthorizations()
                .Where(az => az.SecretMetadata.SecretKind == SecretKind.Password).First();
            archive.RevokeKey(passwordAuthorizationRecord.AuthorizationId);
        }

        // 11 -> 13
        using (var archive = AegisArchive.Load(ArchiveTestHelpers.GetTestArchiveFileSettings(this.WorkingDirectory)))
        {
            Assert.ThrowsException<UnauthorizedException>(
                () => archive.Unlock(TestSecrets.GetDefaultUserSecret(SecretKind.Password)));
            Assert.IsTrue(archive.IsLocked);

            archive.Unlock(TestSecrets.GetDefaultUserSecret(SecretKind.RsaKeyFromCertificate));
            Assert.IsFalse(archive.IsLocked);

            var extractPath = Path.Combine(this.WorkingDirectory, "Text_out2.txt");
            using (var extractStream = File.OpenWrite(extractPath))
            {
                archive.ExtractFile(file, extractStream);
            }
            ArchiveTestHelpers.CompareFileToReference(
                referenceFilePath: ArchiveTestHelpers.SampleFiles.SimpleTextFilePath,
                testFilePath: extractPath,
                deleteTestFile: true);
        }
    }
}
