using Aegis.Core;
using Aegis.Core.FileSystem;
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

        /// <summary>
        /// The path to an existing Aegis archive file with known contents, used for compatibility tests.
        /// </summary>
        public const string KnownArchiveFile = @"SampleFiles\KnownArchive.ags";
    }

    /// <summary>
    /// The default file name for test archives.
    /// </summary>
    public const string DefaultTestArchiveName = "TestArchive.ags";

    public static SecureArchiveFileSettings GetTestArchiveFileSettings(string workingDirectory, string archiveName = null) =>
        new SecureArchiveFileSettings(
            archiveFilePath: Path.Combine(workingDirectory, archiveName ?? DefaultTestArchiveName),
            tempDirectory: workingDirectory);

    /// <summary>
    /// Creates a new Aegis archive for testing.
    /// The password for the archive will be <see cref="DefaultPassword"/>.
    /// </summary>
    /// <param name="workingDirectory">The working directory where the archive will be created.</param>
    /// <param name="initialSecretKind">The type of secret to use when initially creating the archive.</param>
    /// <param name="archiveName">The name of the archive (Optional. Default: <see cref="DefaultTestArchiveName"/>.)</param>
    /// <returns>The new archive, which will be open and unlocked.</returns>
    public static AegisArchive CreateNewEmptyArchive(string workingDirectory, SecretKind initialSecretKind, string archiveName = null) =>
        AegisArchive.CreateNew(
            GetTestArchiveFileSettings(workingDirectory, archiveName),
            new SecureArchiveCreationParameters(
                TestSecrets.GetDefaultUserKeyAuthorizationParameters(initialSecretKind)));

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

    /// <summary>
    /// Simplifies the process of extracting a file from an archive and verifying its contents against a reference file.
    /// </summary>
    public static void ExtractAndCheckFileContents(
        AegisArchive archive,
        AegisFileInfo file,
        string extractionDirectory,
        string referenceFilePath)
    {
        var extractFullPath = Path.Combine(extractionDirectory, Guid.NewGuid().ToString("N"));

        // The 'using' block ensures that the file write handle is closed before we read for comparison.
        using (var extractStream = File.OpenWrite(extractFullPath))
        {
            archive.ExtractFile(file, extractStream);
        }

        CompareFileToReference(
            referenceFilePath: referenceFilePath,
            testFilePath: extractFullPath,
            deleteTestFile: true);
    }
}
