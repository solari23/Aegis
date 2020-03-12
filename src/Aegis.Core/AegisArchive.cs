using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

using Aegis.Core.Crypto;
using Aegis.Models;

namespace Aegis.Core
{
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
            // TODO: Validate input fileSettings
            // TODO: Validate input creationParameters

            var currentTime = DateTimeOffset.UtcNow;
            var archiveId = Guid.NewGuid();

            var archiveKey = ArchiveKey.CreateNew(creationParameters.SecuritySettings);

            // Derive and authorize the first user key.
            var keyDerivationSalt = CryptoHelpers.GetRandomBytes(creationParameters.KeyDerivationSaltSizeInBytes);
            using var firstUserKey = UserKey.DeriveFrom(
                creationParameters.UserSecret,
                keyDerivationSalt,
                creationParameters.SecuritySettings);

            var firstUserKeyAuthorization = UserKeyAuthorizationExtensions.CreateNewAuthorization(
                creationParameters.UserKeyFriendlyName,
                firstUserKey,
                archiveKey,
                creationParameters.SecuritySettings);

            var authCanary = archiveKey.Encrypt(
                CryptoHelpers.GetCryptoStrategy(creationParameters.SecuritySettings.EncryptionAlgo),
                archiveId.ToByteArray());

            var archiveMetadata = new SecureArchiveMetadata
            {
                Id = archiveId,
                SecuritySettings = creationParameters.SecuritySettings,
                CreateTime = currentTime,
                LastModifiedTime = currentTime,
                KeyDerivationSalt = new List<byte>(keyDerivationSalt),
                AuthCanary = authCanary,
                UserKeyAuthorizations = new List<UserKeyAuthorization> { firstUserKeyAuthorization },
            };

            var archive = new AegisArchive
            {
                ArchiveMetadata = archiveMetadata,
                ArchiveKey = archiveKey,
                FileSettings = fileSettings,
                FileIndex = new FileIndex(),
                SecureArchive = OpenSecureArchiveFile(fileSettings, createNewArchive: true),
            };

            archive.PersistMetadata();

            return archive;
        }

        /// <summary>
        /// Loads a <see cref="AegisBondArchive"/> from disk. This operation does not unlock the archive.
        /// </summary>
        /// <param name="fileSettings">Settings for where the archive and related files are stored.</param>
        /// <returns>The loaded <see cref="AegisBondArchive"/>.</returns>
        public static AegisArchive Load(SecureArchiveFileSettings fileSettings)
        {
            // TODO: Validate input fileSettings

            ZipArchive secureArchive = null;

            try
            {
                // Open the secure archive and read the metadata entry.
                secureArchive = OpenSecureArchiveFile(fileSettings);

                var metadataEntry = secureArchive.GetEntry(Constants.SecureArchiveMetadataEntryName);

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

                return new AegisArchive
                {
                    ArchiveMetadata = metadata,
                    FileSettings = fileSettings,
                    SecureArchive = secureArchive,
                };
            }
            catch
            {
                // If we failed to open the archive for any reason, make sure we release the
                // hold on the underlying ZIP file.
                secureArchive?.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AegisArchive"/> class.
        /// </summary>
        private AegisArchive()
        {
            // Hidden ctor.
        }

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
        public void Unlock(ReadOnlySpan<byte> userSecret)
        {
            ArgCheck.NotEmpty(userSecret, nameof(userSecret));

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
            ArgCheck.NotNull(userKey, nameof(userKey));

            // Setting the ArchiveKey property puts the archive into the "unlocked" state.
            // Wait to set the property until after everything is properly unlocked.
            var archiveKey = this.DecryptArchiveKey(userKey);

            this.FileIndex = this.ArchiveMetadata.EncryptedFileIndex is null || this.ArchiveMetadata.EncryptedFileIndex.IsEmpty()
                ? new FileIndex()
                : FileIndex.DecryptFrom(this.ArchiveMetadata.EncryptedFileIndex, archiveKey, this.ArchiveMetadata.SecuritySettings);

            this.ArchiveKey = archiveKey;
        }

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
        private void PersistMetadata()
        {
            // Be extra cautious to avoid corrupting existing archives.
            if (this.IsLocked)
            {
                throw new InvalidOperationException("Attempted to reserialize index/metadata while archive is locked!");
            }

            if (this.FileIndex is null)
            {
                throw new InvalidOperationException("FileIndex is null - unexpected when the archive is unlocked!");
            }

            // Flush the file index contents back into metadata, in case they've changed.
            this.ArchiveMetadata.EncryptedFileIndex = this.FileIndex.Encrypt(
                this.ArchiveKey,
                this.ArchiveMetadata.SecuritySettings);

            this.ArchiveMetadata.LastModifiedTime = DateTimeOffset.UtcNow;

            // Serialize the metadata to JSON and write it to the archive.
            var metadataJson = JsonSerializer.Serialize(this.ArchiveMetadata, JsonHelpers.DefaultSerializerOptions);

            var metadataArchiveEntry = this.SecureArchive.CreateEntry(Constants.SecureArchiveMetadataEntryName);
            using var archiveWriter = new StreamWriter(metadataArchiveEntry.Open());
            archiveWriter.Write(metadataJson);
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
}
