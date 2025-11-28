using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.Json;

using Aegis.Core.CredentialsInterface;
using Aegis.Core.Crypto;
using Aegis.Core.FileSystem;
using Aegis.Models;

namespace Aegis.Core;

/// <summary>
/// The core data structure that implements archive functionality.
/// </summary>
public sealed class AegisArchive : IDisposable
{
    /// <summary>
    /// Creates a new <see cref="AegisArchive"/> that contains no files.
    /// </summary>
    /// <param name="fileSettings">Settings for where the archive and related files are stored.</param>
    /// <param name="creationParameters">The <see cref="SecureArchiveCreationParameters"/> to use when creating the archive.</param>
    /// <returns>A new <see cref="AegisArchive"/> that is opened but not yet persisted to disk.</returns>
    public static AegisArchive CreateNew(
        SecureArchiveFileSettings fileSettings,
        SecureArchiveCreationParameters creationParameters)
    {
        ArgCheck.IsValid(fileSettings);
        ArgCheck.IsValid(creationParameters);

        var currentTime = DateTimeOffset.UtcNow;
        var archiveId = creationParameters.ArchiveId;

        ArchiveKey tempArchiveKey = null;

        AegisArchive tempArchive = null;
        AegisArchive archive = null;

        try
        {
            tempArchiveKey = ArchiveKey.CreateNew(creationParameters.SecuritySettings);

            // Derive and authorize the first user key.
            var keyDerivationSalt = CryptoHelpers.GetRandomBytes(creationParameters.KeyDerivationSaltSizeInBytes);

            var firstUserKeyAuthorization = UserKeyAuthorizationExtensions.CreateNewAuthorization(
                creationParameters.FirstKeyAuthorizationParams,
                keyDerivationSalt,
                tempArchiveKey,
                creationParameters.SecuritySettings);

            var authCanary = tempArchiveKey.Encrypt(
                CryptoHelpers.GetCryptoStrategy(creationParameters.SecuritySettings.EncryptionAlgo),
                archiveId.ToByteArray());

            var archiveMetadata = new SecureArchiveMetadata
            {
                Id = archiveId,
                SecuritySettings = creationParameters.SecuritySettings,
                CreateTime = currentTime,
                LastModifiedTime = currentTime,
                KeyDerivationSalt = keyDerivationSalt,
                AuthCanary = authCanary,
                UserKeyAuthorizations = [firstUserKeyAuthorization],
            };

            tempArchive = new AegisArchive
            {
                ArchiveMetadata = archiveMetadata,
                ArchiveKey = tempArchiveKey,
                FileSettings = fileSettings,
                FileIndex = new FileIndex(),
                SecureArchive = OpenSecureArchiveFile(fileSettings, createNewArchive: true),
            };

            tempArchive.PersistMetadata();

            // Transfer the archive reference to the return variable.
            archive = tempArchive;
            tempArchive = null;

            // Dispose ownership of the archive key now belongs to the archive.
            tempArchiveKey = null;
        }
        finally
        {
            tempArchive?.Dispose();
            tempArchiveKey?.Dispose();
        }

        return archive;
    }

    /// <summary>
    /// Loads a <see cref="AegisBondArchive"/> from disk. This operation does not unlock the archive.
    /// </summary>
    /// <param name="fileSettings">Settings for where the archive and related files are stored.</param>
    /// <returns>The loaded <see cref="AegisBondArchive"/>.</returns>
    public static AegisArchive Load(SecureArchiveFileSettings fileSettings)
    {
        ArgCheck.IsValid(fileSettings);

        ZipArchive tempSecureArchive = null;
        AegisArchive archive = null;

        try
        {
            // Open the secure archive and read the metadata entry.
            tempSecureArchive = OpenSecureArchiveFile(fileSettings);

            var metadataEntry = tempSecureArchive.GetEntry(AegisConstants.SecureArchiveMetadataEntryName);

            if (metadataEntry is null)
            {
                throw new ArchiveCorruptedException("The secure archive does not contain any internal metadata!");
            }

            using var metadataReader = new StreamReader(metadataEntry.Open());

            var metadataJson = metadataReader.ReadToEnd();
            var metadata = JsonSerializer.Deserialize<SecureArchiveMetadata>(metadataJson);

            if (metadata is null)
            {
                throw new ArchiveCorruptedException("Unable to parse the archive internal metadata!");
            }

            archive = new AegisArchive
            {
                ArchiveMetadata = metadata,
                FileSettings = fileSettings,
                SecureArchive = tempSecureArchive,
            };

            // We've transfered dispose ownership of tempSecureArchive to the archive.
            tempSecureArchive = null;
        }
        finally
        {
            tempSecureArchive?.Dispose();
        }

        return archive;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AegisArchive"/> class.
    /// </summary>
    private AegisArchive()
    {
        // Hidden ctor.
    }

    /// <summary>
    /// Gets the unique identifier of the archive.
    /// </summary>
    public Guid Id => this.ArchiveMetadata.Id;

    /// <summary>
    /// Gets the name of the archive file.
    /// </summary>
    public string FileName => Path.GetFileName(this.FileSettings?.Path ?? string.Empty);

    /// <summary>
    /// Gets the full path to the archive file on disk.
    /// </summary>
    public string FullFilePath => this.FileSettings?.Path ?? string.Empty;

    /// <summary>
    /// Gets whether or not the archive is locked.
    /// </summary>
    public bool IsLocked => this.ArchiveKey == null;

    /// <summary>
    /// Gets the compressed archive where data is stored.
    /// </summary>
    private ZipArchive SecureArchive { get; set; }

    /// <summary>
    /// Gets the underlying archive metadata.
    /// </summary>
    private SecureArchiveMetadata ArchiveMetadata { get; set; }

    /// <summary>
    /// Gets the <see cref="ICryptoStrategy"/> configured for the archive.
    /// </summary>
    private ICryptoStrategy CryptoStrategy => CryptoHelpers.GetCryptoStrategy(this.ArchiveMetadata.SecuritySettings.EncryptionAlgo);

    /// <summary>
    /// Gets the archive encryption key, or null if the archive is locked.
    /// </summary>
    private ArchiveKey ArchiveKey { get; set; }

    /// <summary>
    /// Gets settings for where the archive and related files are stored.
    /// </summary>
    private SecureArchiveFileSettings FileSettings { get; set; }

    /// <summary>
    /// Gets the index of files stored in the archive.
    /// </summary>
    private FileIndex FileIndex { get; set; }

    /// <summary>
    /// Unlocks (i.e. decrypts) the archive with the given raw user secret.
    /// </summary>
    /// <param name="userSecret">The user secret to use to unlock the archive.</param>
    public void Unlock(RawUserSecret userSecret)
    {
        ArgCheck.NotNull(userSecret);

        using var userKey = UserKey.DeriveFrom(
            userSecret,
            this.ArchiveMetadata.KeyDerivationSalt.ToArray(),
            this.ArchiveMetadata.SecuritySettings);
        this.Unlock(userKey);
    }

    /// <summary>
    /// Unlocks (i.e. decrypts) the archive with the given <see cref="UserKey"/>.
    /// </summary>
    /// <param name="userKey">The <see cref="UserKey"/> to use to unlock the archive.</param>
    public void Unlock(UserKey userKey)
    {
        ArgCheck.NotNull(userKey);

        // Setting the ArchiveKey property puts the archive into the "unlocked" state.
        // Wait to set the property until after everything is properly unlocked.
        var archiveKey = this.DecryptArchiveKey(userKey);

        this.FileIndex = this.ArchiveMetadata.EncryptedFileIndex is null || this.ArchiveMetadata.EncryptedFileIndex.IsEmpty()
            ? new FileIndex()
            : FileIndex.DecryptFrom(this.ArchiveMetadata.EncryptedFileIndex, archiveKey, this.ArchiveMetadata.SecuritySettings);

        this.ArchiveKey = archiveKey;
    }

    /// <summary>
    /// Retrieves information about a file in the archive.
    /// </summary>
    /// <param name="fileId">The ID of the file.</param>
    /// <returns>The <see cref="AegisFileInfo"/> about the file, or null if it isn't found.</returns>
    public AegisFileInfo GetFileInfo(Guid fileId)
    {
        this.ThrowIfLocked();

        return this.FileIndex.GetFileInfo(fileId);
    }

    /// <summary>
    /// Retrieves information about a file in the archive.
    /// </summary>
    /// <param name="filePath">The virual path to the file.</param>
    /// <returns>The <see cref="AegisFileInfo"/> about the file, or null if it isn't found.</returns>
    public AegisFileInfo GetFileInfo(AegisVirtualFilePath filePath)
    {
        ArgCheck.NotNull(filePath);
        this.ThrowIfLocked();

        return this.FileIndex.GetFileInfo(filePath);
    }

    /// <summary>
    /// Executes a traversal of the virtual tree of archived files.
    /// </summary>
    /// <param name="visitorImplementation">The visitor implementation to execute when visiting each node.</param>
    public void TraverseFileTree(IVirtualFileTreeVisitor visitorImplementation)
    {
        ArgCheck.NotNull(visitorImplementation);
        this.ThrowIfLocked();

        this.FileIndex.TraverseFileTree(visitorImplementation);
    }

    /// <summary>
    /// Adds a file to the archive.
    /// </summary>
    /// <param name="virtualPath">The virtual path to add the file at.</param>
    /// <param name="fileStream">The file data stream.</param>
    /// <param name="overwriteIfExists">
    ///     Whether or not to overwrite existing files at that path.
    ///     If false, an <see cref="InvalidOperationException"/> is thrown if a file already exists at the path.
    /// </param>
    /// <returns>Metadata about the file added to the archive.</returns>
    public AegisFileInfo PutFile(AegisVirtualFilePath virtualPath, Stream fileStream, bool overwriteIfExists = false)
    {
        ArgCheck.NotNull(virtualPath);
        ArgCheck.NotNull(fileStream);
        this.ThrowIfLocked();

        // Remove any existing files stored at the path, if we're allowed to.
        var existingFileInfo = this.FileIndex.GetFileInfo(virtualPath);
        if (existingFileInfo != null)
        {
            if (!overwriteIfExists)
            {
                throw new InvalidOperationException(
                    $"A file with ID '{existingFileInfo.FileId}' already exists at virtual path '{virtualPath}'!");
            }

            this.RemoveFile(existingFileInfo.FileId);
        }

        var currentTime = DateTimeOffset.UtcNow;

        var fileInfo = existingFileInfo ?? new AegisFileInfo(
            virtualPath,
            new FileIndexEntry
            {
                FileId = Guid.NewGuid(),
                FilePath = virtualPath.ToString(),
                AddedTime = currentTime,
                LastModifiedTime = currentTime,
            });
        fileInfo.IndexEntry.LastModifiedTime = currentTime;

        // Encrypt the file.
        // This mechanism won't work for very large files since it loads the whole thing into memory.
        // For now, this is an acceptable limit for Aegis file archiving.
        using var memoryStream = new MemoryStream();
        fileStream.CopyTo(memoryStream);
        var encryptedFile = this.ArchiveKey.Encrypt(
            this.CryptoStrategy,
            memoryStream.ToArray());
        encryptedFile.WriteToBinaryStream(this.SecureArchive.GetEntryWriteStream(fileInfo.ArchiveEntryName));

        try
        {
            this.FileIndex.Add(fileInfo);
            this.PersistMetadata();
        }
        catch
        {
            // Creating file metadata failed.
            // Revert adding file to keep the archive to keep it consistent.
            this.SecureArchive.DeleteEntryIfExists(fileInfo.ArchiveEntryName);

            throw;
        }

        this.FlushArchive();

        return fileInfo;
    }

    /// <summary>
    /// Decrypts and extract a file from the archive.
    /// </summary>
    /// <param name="fileInfo">The file to extract.</param>
    /// <param name="outputStream">The stream to extract the file to.</param>
    public void ExtractFile(AegisFileInfo fileInfo, Stream outputStream)
    {
        ArgCheck.NotNull(fileInfo);
        ArgCheck.NotNull(outputStream);
        this.ThrowIfLocked();

        var archiveEntry = this.SecureArchive.GetEntry(fileInfo.ArchiveEntryName);

        if (archiveEntry is null)
        {
            throw new ArchiveCorruptedException($"Unable to find archived entry for file ID {fileInfo.FileId}");
        }

        using var archiveStream = archiveEntry.Open();
        var encryptedFileData = EncryptedPacketExtensions.ReadFromBinaryStream(
            archiveStream,
            this.ArchiveMetadata.SecuritySettings.EncryptionAlgo);
        var plainTextFileBytes = this.ArchiveKey.Decrypt(this.CryptoStrategy, encryptedFileData);
        outputStream.Write(plainTextFileBytes);
    }

    /// <summary>
    /// Removes a file from the archive.
    /// </summary>
    /// <param name="filePath">The virtual path to the file.</param>
    /// <remarks>If no file exists at that path, this operation is a silent no-op.</remarks>
    public void RemoveFile(AegisVirtualFilePath filePath)
    {
        ArgCheck.NotNull(filePath);
        this.ThrowIfLocked();

        var fileInfo = this.GetFileInfo(filePath);
        if (fileInfo != null)
        {
            this.RemoveFile(fileInfo.FileId);
        }
    }

    /// <summary>
    /// Removes a file from the archive.
    /// </summary>
    /// <param name="fileId">The ID of file.</param>
    /// <remarks>If no file exists with the given ID, this operation is a silent no-op.</remarks>
    public void RemoveFile(Guid fileId)
    {
        this.ThrowIfLocked();

        var fileInfo = this.GetFileInfo(fileId);
        if (fileInfo is null)
        {
            throw new EntityNotFoundException(
                $"Archived file with ID '{fileId}' was not found.");
        }

        this.FileIndex.Remove(fileInfo.FileId);
        this.PersistMetadata();

        try
        {
            this.SecureArchive.DeleteEntryIfExists(fileInfo.ArchiveEntryName);
        }
        catch
        {
            // Deleting file data failed.
            // To keep the archive consistent, attempt to recreate the index entry.
            this.FileIndex.Add(fileInfo);
            this.PersistMetadata();

            throw;
        }

        this.FlushArchive();
    }

    /// <summary>
    /// Authorizes a new user key to access the archive.
    /// </summary>
    /// <param name="authorizationParameters">The new user key authorization parameters.</param>
    public void AuthorizeNewKey(UserKeyAuthorizationParameters authorizationParameters)
    {
        ArgCheck.IsValid(authorizationParameters);

        this.ThrowIfLocked();

        var newAuthorization = UserKeyAuthorizationExtensions.CreateNewAuthorization(
            authorizationParameters,
            this.ArchiveMetadata.KeyDerivationSalt.ToArray(),
            this.ArchiveKey,
            this.ArchiveMetadata.SecuritySettings);

        this.ArchiveMetadata.UserKeyAuthorizations.Add(newAuthorization);

        try
        {
            this.PersistMetadata();
        }
        catch
        {
            // Revert the change. Keep in-memory structure consistent.
            this.ArchiveMetadata.UserKeyAuthorizations.RemoveAt(
                this.ArchiveMetadata.UserKeyAuthorizations.Count - 1);
            throw;
        }
    }

    /// <summary>
    /// Revokes an authorized key so it can no longer unlock the archive.
    /// </summary>
    /// <param name="authorizationId">The authorization ID of the key to revoke.</param>
    /// <exception cref="InvalidOperationException">The specified authorization record was not found.</exception>
    public void RevokeKey(Guid authorizationId)
    {
        var matchingAuthorizations = this.ArchiveMetadata.UserKeyAuthorizations
            .Where(k => k.AuthorizationId == authorizationId)
            .ToArray();

        if (matchingAuthorizations.Length == 0)
        {
            throw new EntityNotFoundException(
                $"Authorization with ID '{authorizationId}' was not found.");
        }

        if (matchingAuthorizations.Length > 1)
        {
            // There should never be two authorizations with the same ID.
            // If this happens, fail closed to avoid any further archive corruption.
            throw new ArchiveCorruptedException(
                $"Found 2 key authorizations with ID '{authorizationId}'. Cancelling revocation operation.");
        }

        if (this.ArchiveMetadata.UserKeyAuthorizations.Count == 1)
        {
            throw new InvalidOperationException($"Can't revoke the last authorized key.");
        }

        // We'll track the item being removed. If saving the changes fails, we'll
        // revert the removal to put the in-memory structure back into a consistent state.
        UserKeyAuthorization removedAuthorization = null;
        int removedIndex = -1;

        for (int i = 0; i < this.ArchiveMetadata.UserKeyAuthorizations.Count; i++)
        {
            if (this.ArchiveMetadata.UserKeyAuthorizations[i].AuthorizationId == authorizationId)
            {
                removedAuthorization = this.ArchiveMetadata.UserKeyAuthorizations[i];
                removedIndex = i;

                this.ArchiveMetadata.UserKeyAuthorizations.RemoveAt(i);

                break;
            }
        }

        try
        {
            this.PersistMetadata();
        }
        catch
        {
            // Revert the change. Keep in-memory structure consistent.
            this.ArchiveMetadata.UserKeyAuthorizations.Insert(
                removedIndex,
                removedAuthorization);
            throw;
        }
    }

    /// <summary>
    /// Gets the user key authorization records for the archive.
    /// </summary>
    /// <returns>A copy of the user key authorizations.</returns>
    public List<UserKeyAuthorization> GetUserKeyAuthorizations() =>
        // Slightly hacky -- we'll use the serialization cloning trick.
        // We don't want to return the original references.
        JsonSerializer.Deserialize<List<UserKeyAuthorization>>(
            JsonSerializer.Serialize(this.ArchiveMetadata.UserKeyAuthorizations));

    /// <summary>
    /// Gets the archive's <see cref="ArchiveMetadata"/> serialized as JSON.
    /// </summary>
    /// <returns>The serialized metadata.</returns>
    public string GetArchiveMetadataJson() =>
        JsonSerializer.Serialize(this.ArchiveMetadata, JsonHelpers.DefaultSerializerOptions);

    /// <summary>
    /// Opens a secure archive (zip) file on disk.
    /// </summary>
    /// <param name="fileSettings">The <see cref="SecureArchiveFileSettings"/> for the archive to open.</param>
    /// <param name="createNewArchive">Indicates whether we expect to be creating a new secure archive.</param>
    private static ZipArchive OpenSecureArchiveFile(SecureArchiveFileSettings fileSettings, bool createNewArchive = false)
    {
        if (createNewArchive)
        {
            // Create the output directory if it doesn't already exist.
            var directoryPath = Path.GetDirectoryName(fileSettings.Path);
            if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        else if (!File.Exists(fileSettings.Path))
        {
            throw new FileNotFoundException($"No archive file was found at {fileSettings.Path}", fileSettings.Path);
        }

        return ZipFile.Open(fileSettings.Path, ZipArchiveMode.Update);
    }

    /// <summary>
    /// Checks if the input <see cref="UserKey"/> is authorized to unlock the 
    /// <see cref="SecureArchive"/> and uses it to decrypt the <see cref="ArchiveKey"/>.
    /// </summary>
    /// <param name="userKey">The <see cref="UserKey"/> to </param>
    /// <returns>The data encryption key for the <see cref="SecureArchive"/>.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the input key is not authorized to unlock the archive.</exception>
    private ArchiveKey DecryptArchiveKey(UserKey userKey)
    {
        const string error = "The user key is not authorized to unlock the archive.";

        var keyAuthorizationRecord = this.ArchiveMetadata.UserKeyAuthorizations.FirstOrDefault(
            k => CryptoHelpers.SecureEquals(k.KeyId, userKey.KeyId));

        if (keyAuthorizationRecord is null
            || !keyAuthorizationRecord.TryDecryptArchiveKey(userKey, this.ArchiveMetadata.SecuritySettings, out var archiveKey))
        {
            throw new UnauthorizedException(error);
        }

        // Check the auth canary.
        Guid decryptedCanary;

        try
        {
            decryptedCanary = new Guid(archiveKey.Decrypt(this.CryptoStrategy, this.ArchiveMetadata.AuthCanary));
        }
        catch
        {
            throw new UnauthorizedException(error);
        }

        if (!CryptoHelpers.SecureEquals(decryptedCanary.ToByteArray(), this.ArchiveMetadata.Id.ToByteArray()))
        {
            throw new UnauthorizedException(error);
        }

        // Congrats, you're authorized!
        return archiveKey;
    }

    /// <summary>
    /// Persists the <see cref="SecureArchiveMetadata"/> to the secure archive file.
    /// </summary>
    /// <remarks>
    /// The updates will still not be flushed to disk. To fully persist, also call <see cref="FlushArchive"/>.
    /// </remarks>
    private void PersistMetadata()
    {
        // Be extra cautious to avoid corrupting existing archives.
        if (this.IsLocked)
        {
            // In this case the fact that we're running this operation on a locked archive
            // is an internal error. Do not throw ArchiveLockedException, which indicates user error.
            throw new AegisInternalErrorException("Attempted to reserialize index/metadata while archive is locked!");
        }

        if (this.FileIndex is null)
        {
            throw new AegisInternalErrorException("FileIndex is null - unexpected when the archive is unlocked!");
        }

        // Flush the file index contents back into metadata, in case they've changed.
        this.ArchiveMetadata.EncryptedFileIndex = this.FileIndex.Encrypt(
            this.ArchiveKey,
            this.ArchiveMetadata.SecuritySettings);

        this.ArchiveMetadata.LastModifiedTime = DateTimeOffset.UtcNow;

        // Serialize the metadata to JSON and write it to the archive.
        var metadataJson = this.GetArchiveMetadataJson();
        using var archiveWriter = new StreamWriter(
            this.SecureArchive.GetEntryWriteStream(AegisConstants.SecureArchiveMetadataEntryName));
        archiveWriter.Write(metadataJson);
    }

    /// <summary>
    /// Flushes the <see cref="SecureArchive"/> ZIP file to disk by disposing and re-opening it.
    /// </summary>
    /// <remarks>
    /// This is a workaround due to the .NET ZipArchive SDK not actually flushing data as you
    /// write it and not providing a Flush() method. Without this method, if e.g. the user removes
    /// a file and closes Aegis without properly closing the archive then the update will be lost.
    /// </remarks>
    private void FlushArchive()
    {
        if (this.SecureArchive != null)
        {
            this.SecureArchive.Dispose();
            this.SecureArchive = ZipFile.Open(this.FileSettings.Path, ZipArchiveMode.Update);
        }
    }

    /// <summary>
    /// Helper to throw an <see cref="ArchiveLockedException"/> if the archive is locked.
    /// </summary>
    [DebuggerStepThrough]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void ThrowIfLocked()
    {
        if (this.IsLocked)
        {
            throw new ArchiveLockedException();
        }
    }

    #region IDisposable Support

    /// <summary>
    /// Flag to detect redundant calls to dispose the object.
    /// </summary>
    private bool isDisposed = false;

    /// <summary>
    /// Disposes the current object when it is no longer required.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the current object when it is no longer required.
    /// </summary>
    /// <param name="disposing">Whether or not the operation is coming from Dispose() (as opposed to a finalizer).</param>
    protected void Dispose(bool disposing)
    {
        if (!this.isDisposed)
        {
            if (disposing)
            {
                this.ArchiveKey?.Dispose();
                this.SecureArchive?.Dispose();
            }

            this.isDisposed = true;
        }
    }

    #endregion
}
