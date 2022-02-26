using System.Text;

using Aegis.Core;
using Aegis.Core.CredentialsInterface;
using Aegis.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aegis.Test;

/// <summary>
/// Helpers for test cases that deal with Aegis archives.
/// </summary>
public static class ArchiveTestHelpers
{
    public static class SampleFiles
    {
        /// <summary>
        /// The path to a basic text sample file for use in tests.
        /// </summary>
        public const string SimpleTextFilePath = @"SampleFiles\SimpleTextFile.txt";

        /// <summary>
        /// The path to a basic JPG for use in tests.
        /// </summary>
        public const string SimpleImageFilePath = @"SampleFiles\SimpleImage.jpg";
    }

    /// <summary>
    /// The default password for test archives.
    /// </summary>
    public const string DefaultPassword = "P@$$w3rd";

    /// <summary>
    /// The default file name for test archives.
    /// </summary>
    public const string DefaultTestArchiveName = "TestArchive.ags";

    /// <summary>
    /// Gets the <see cref="DefaultPassword"/> as a <see cref="RawUserSecret"/>.
    /// </summary>
    public static RawUserSecret DefaultPasswordUserSecret =>
        new RawUserSecret(Encoding.UTF8.GetBytes(DefaultPassword));

    public static SecureArchiveFileSettings GetTestArchiveFileSettings(string workingDirectory, string archiveName = null) =>
        new SecureArchiveFileSettings(
            archiveFilePath: Path.Combine(workingDirectory, archiveName ?? DefaultTestArchiveName),
            tempDirectory: workingDirectory);

    /// <summary>
    /// Creates a new Aegis archive for testing.
    /// The password for the archive will be <see cref="DefaultPassword"/>.
    /// </summary>
    /// <param name="workingDirectory">The working directory where the archive will be created.</param>
    /// <param name="archiveName">The name of the archive (Optional. Default: <see cref="DefaultTestArchiveName"/>.)</param>
    /// <returns>The new archive, which will be open and unlocked.</returns>
    public static AegisArchive CreateNewEmptyArchive(string workingDirectory, string archiveName = null) =>
        AegisArchive.CreateNew(
            GetTestArchiveFileSettings(workingDirectory, archiveName),
            new SecureArchiveCreationParameters(
                new UserKeyAuthorizationParameters(DefaultPasswordUserSecret)
                {
                    FriendlyName = "TestPassword",
                    SecretMetadata = new PasswordSecretMetadata(),
                }));

    /// <summary>
    /// Compares a test file to a reference file and asserts that they match.
    /// </summary>
    /// <param name="referenceFilePath">The path to the reference file.</param>
    /// <param name="testFilePath">The path to the test file.</param>
    /// <param name="deleteTestFile">Whether to delete the test file from disk when done checking.</param>
    public static void CompareFileToReference(
        string referenceFilePath,
        string testFilePath,
        bool deleteTestFile = false)
    {
        Assert.IsTrue(
            File.Exists(testFilePath),
            $"The test file at path '{testFilePath}' is missing!");

        var referenceFileBytes = File.ReadAllBytes(referenceFilePath);
        var testFileBytes = File.ReadAllBytes(testFilePath);

        Assert.AreEqual(
            referenceFileBytes.Length,
            testFileBytes.Length, $"The test file at path '{testFilePath}' does not have the expected size!");

        for (int i = 0; i < testFileBytes.Length; i++)
        {
            if (referenceFileBytes[i] != testFileBytes[i])
            {
                Assert.Fail($"The test file at path '{testFilePath}' differs in content from the reference file!");
            }
        }

        if (deleteTestFile)
        {
            File.Delete(testFilePath);
        }
    }
}
