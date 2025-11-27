using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Aegis.Core;
using Aegis.Core.CredentialsInterface;
using Aegis.Core.Crypto;
using Aegis.Models;
using Aegis.Test.Core.FileSystem;

namespace Aegis.Test.Core;

[TestClass]
public class CompatibilityTests
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
    [Description("Runs a battery of tests on an existing archive file with known state. This is to guard against regressions that break/corrupt existing files.")]
    public void RunCompatibilityTests()
    {
        var testArchiveName = Guid.NewGuid().ToString("N") + ".ags";
        var testArchivePath = Path.Combine(this.WorkingDirectory, testArchiveName);
        var testArchiveFileSettings = new SecureArchiveFileSettings(
            archiveFilePath: testArchivePath,
            tempDirectory: this.WorkingDirectory);

        // Create a copy of the known archive, so we don't clobber it.
        File.Copy(ArchiveTestHelpers.SampleFiles.KnownArchiveFile, testArchivePath);

        using (var archive = AegisArchive.Load(testArchiveFileSettings))
        {
            // Check the authorization records for the pre-existing secrets.
            var authorizations = archive.GetUserKeyAuthorizations();
            Assert.HasCount(2, authorizations);

            var passwordKeyRecord = authorizations[0];
            Assert.IsInstanceOfType(passwordKeyRecord.SecretMetadata, typeof(PasswordSecretMetadata));
            Assert.AreEqual(SecretKind.Password, passwordKeyRecord.SecretMetadata.SecretKind);
            Assert.AreEqual("Known Password", passwordKeyRecord.FriendlyName);

            var certKeyRecord = authorizations[1];
            Assert.IsInstanceOfType(certKeyRecord.SecretMetadata, typeof(RsaKeyFromCertificateSecretMetadata));
            Assert.AreEqual(SecretKind.RsaKeyFromCertificate, certKeyRecord.SecretMetadata.SecretKind);
            Assert.AreEqual("TestRsaCertificate", certKeyRecord.FriendlyName);
            Assert.AreEqual("6CB6E4FDDECE5FD19C6194D41D63D097DD694A3E", ((RsaKeyFromCertificateSecretMetadata)certKeyRecord.SecretMetadata).Thumbprint);
        }

        // Run checks with the default password.
        var defaultPasswordSecret = TestSecrets.GetDefaultUserSecret(SecretKind.Password);
        CheckKnownFileExtractions(testArchiveFileSettings, defaultPasswordSecret, this.WorkingDirectory);

        // Run checks with the default certificate secret.
        var defaultCertSecret = TestSecrets.GetDefaultUserSecret(SecretKind.RsaKeyFromCertificate);
        CheckKnownFileExtractions(testArchiveFileSettings, defaultCertSecret, this.WorkingDirectory);

        var newPasswordSecret = RawUserSecret.FromPasswordString("NewPassword!!");

        using (var archive = AegisArchive.Load(testArchiveFileSettings))
        {
            archive.Unlock(defaultPasswordSecret);

            // Add another password secret and a file.
            archive.AuthorizeNewKey(new UserKeyAuthorizationParameters(newPasswordSecret)
            {
                FriendlyName = "New Password",
                SecretMetadata = new PasswordSecretMetadata(),
            });

            using var newFileStream = new MemoryStream(Encoding.UTF8.GetBytes("New file data!!"));
            archive.PutFile("path/to/new/file.txt", newFileStream);
        }

        // Rerun checks with the pre-existing secrets.
        CheckKnownFileExtractions(testArchiveFileSettings, defaultPasswordSecret, this.WorkingDirectory);
        CheckKnownFileExtractions(testArchiveFileSettings, defaultCertSecret, this.WorkingDirectory);

        // Run checks with new password.
        CheckKnownFileExtractions(testArchiveFileSettings, newPasswordSecret, this.WorkingDirectory);

        // If we made it this far, the test has passed.
        // No need to keep the test archive around for debugging, so clean it up.
        File.Delete(testArchivePath);
    }

    private static void CheckKnownFileExtractions(
        SecureArchiveFileSettings archiveSettings,
        RawUserSecret rawUserSecret,
        string workingDirectory)
    {
        using var archive = AegisArchive.Load(archiveSettings);

        Assert.IsTrue(archive.IsLocked);

        archive.Unlock(rawUserSecret);
        Assert.IsFalse(archive.IsLocked);

        var testFileVisitor = new VirtualFileTreeTestVisitor();
        archive.TraverseFileTree(testFileVisitor);
        Assert.IsTrue(testFileVisitor.PreOrderVisitFiles.Any(f => f.Path.FullPath == "/text.txt"));
        Assert.IsTrue(testFileVisitor.PreOrderVisitFiles.Any(f => f.Path.FullPath == "/img/img.jpg"));

        var txtFile = archive.GetFileInfo("text.txt");
        Assert.IsNotNull(txtFile);
        ArchiveTestHelpers.ExtractAndCheckFileContents(archive, txtFile, workingDirectory, ArchiveTestHelpers.SampleFiles.SimpleTextFilePath);

        var imgFile = archive.GetFileInfo("img/img.jpg");
        Assert.IsNotNull(imgFile);
        ArchiveTestHelpers.ExtractAndCheckFileContents(archive, imgFile, workingDirectory, ArchiveTestHelpers.SampleFiles.SimpleImageFilePath);
    }
}
