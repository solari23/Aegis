namespace Aegis.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;

    using Aegis.Core.Crypto;

    /// <summary>
    /// The core data structure that stores the user's encrypted documents and implements archive functionality.
    /// </summary>
    /// <remarks>
    /// Data members for this class are defined in a Bond schema (see Aegis.bond).
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1047:DoNotDeclareProtectedMembersInSealedTypes", Justification = "The protected ctor is code generated.")]
    public sealed partial class SecureArchive : IDisposable
    {
        /// <summary>
        /// Creates a new <see cref="SecureArchive"/> that contains no files.
        /// </summary>
        /// <param name="fileSettings">Settings for where the <see cref="SecureArchive"/> and related files are stored.</param>
        /// <param name="creationParameters">The <see cref="SecureArchiveCreationParameters"/> to use when creating the archive.</param>
        /// <returns>A new <see cref="SecureArchive"/>.</returns>
        public static SecureArchive CreateNew(
            SecureArchiveFileSettings fileSettings,
            SecureArchiveCreationParameters creationParameters)
        {
            // TODO: Validate input fileSettings
            // TODO: Validate input creationParameters

            var currentTime = DateTime.UtcNow;
            var archiveId = Guid.NewGuid();

            var archiveKey = ArchiveKey.CreateNew(creationParameters.SecuritySettings);

            // Derive and authorize the first user key.
            var keyDerivationSalt = CryptoHelpers.GetRandomBytes(creationParameters.KeyDerivationSaltSizeInBytes);
            using var firstUserKey = UserKey.DeriveFrom(
                creationParameters.UserSecret,
                keyDerivationSalt,
                creationParameters.SecuritySettings);
            var firstUserKeyAuthorization = UserKeyAuthorization.CreateNewAuthorization(
                creationParameters.UserKeyFriendlyName,
                firstUserKey,
                archiveKey,
                creationParameters.SecuritySettings);

            var authCanary = archiveKey.Encrypt(
                CryptoHelpers.GetCryptoStrategy(creationParameters.SecuritySettings.EncryptionAlgo),
                archiveId.ToByteArray());

            var archive = new SecureArchive
            {
                Id = archiveId,
                FileVersion = Constants.CurrentAegisSecureArchiveFileVersion,
                SecuritySettings = creationParameters.SecuritySettings,
                CreateTime = currentTime,
                LastModifiedTime = currentTime,
                KeyDerivationSalt = new List<byte>(keyDerivationSalt),
                AuthCanary = authCanary,
                UserKeyAuthorizations = new List<UserKeyAuthorization> { firstUserKeyAuthorization },

                // Non-serialized properties.
                ArchiveKey = archiveKey,
                FileSettings = fileSettings,
                FileIndex = new FileIndex(),
                IsDirty = true,
            };

            return archive;
        }

        /// <summary>
        /// Loads a <see cref="SecureArchive"/> from disk. This operation does not unlock the archive.
        /// </summary>
        /// <param name="fileSettings">Settings for where the <see cref="SecureArchive"/> and related files are stored.</param>
        /// <returns>The loaded <see cref="SecureArchive"/>.</returns>
        public static SecureArchive Load(SecureArchiveFileSettings fileSettings)
        {
            // TODO: Validate input fileSettings

            // See Aegis file format documentation in file Aegis.bond
            using var fileStream = File.OpenRead(fileSettings.Path);

            if (fileStream.Length < 36)
            {
                throw new ArchiveCorruptedException("The archive is too small to load.");
            }

            var fileVersion = new byte[4];
            var archiveHash = new byte[32];
            var archiveBytes = new byte[fileStream.Length - 4 - 32];

            // Note: Eventually we may want to branch here if new file versions
            // have a different format. For now, there's just the one format.
            fileStream.Read(fileVersion);
            fileStream.Read(archiveHash);
            fileStream.Read(archiveBytes);

            SecureArchive archive;

            try
            {
                archive = BondHelpers.Deserialize<SecureArchive>(archiveBytes);

                if (archive is null)
                {
                    throw new InvalidOperationException("The archive is corrupted and can't be deserialized.");
                }
            }
            catch (Exception e)
            {
                throw new ArchiveCorruptedException("Unable to open the archive because it is corrupted.", e);
            }

            archive.FileSettings = fileSettings;

            // Save the loaded data so the hash can be verified when the archive is unlocked.
            archive.LoadedArchiveDataToBeVerified = (Hash: archiveHash, Data: archiveBytes);

            return archive;
        }

        /// <summary>
        /// Gets the name of the <see cref="SecureArchive"/> file.
        /// </summary>
        public string FileName => Path.GetFileName(this.FileSettings?.Path ?? string.Empty);

        /// <summary>
        /// Gets the full path to the <see cref="SecureArchive"/> file.
        /// </summary>
        public string FullFilePath => this.FileSettings?.Path ?? string.Empty;

        /// <summary>
        /// Gets whether or not the <see cref="SecureArchive"/> is locked.
        /// </summary>
        public bool IsLocked => this.ArchiveKey == null;

        /// <summary>
        /// Gets whether or not the <see cref="SecureArchive"/> has been updated but not saved.
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICryptoStrategy"/> configured for the archive.
        /// </summary>
        internal ICryptoStrategy CryptoStrategy => CryptoHelpers.GetCryptoStrategy(this.SecuritySettings.EncryptionAlgo);

        /// <summary>
        /// Gets the archive encryption key, or null if the archive is locked.
        /// </summary>
        private ArchiveKey ArchiveKey { get; set; }

        /// <summary>
        /// Gets settings for where the <see cref="SecureArchive"/> and related files are stored.
        /// </summary>
        private SecureArchiveFileSettings FileSettings { get; set; }

        /// <summary>
        /// Gets the index of files stored in the <see cref="SecureArchive"/>.
        /// </summary>
        private FileIndex FileIndex { get; set; }

        /// <summary>
        /// Raw archive data loaded from disk. We keep a reference so we can verify the HMAC-256 
        /// hash of the <see cref="SecureArchive"/> on Unlock() to detect file tampering.
        /// </summary>
        private (byte[] Hash, byte[] Data) LoadedArchiveDataToBeVerified { get; set; }

        /// <summary>
        /// Unlocks (i.e. decrypts) the <see cref="SecureArchive"/> with the given raw user secret.
        /// </summary>
        /// <param name="userSecret">The user secret to use to unlock the archive.</param>
        public void Unlock(ReadOnlySpan<byte> userSecret)
        {
            ArgCheck.NotEmpty(userSecret, nameof(userSecret));

            using var userKey = UserKey.DeriveFrom(userSecret, this.KeyDerivationSalt.ToArray(), this.SecuritySettings);
            this.Unlock(userKey);
        }

        /// <summary>
        /// Unlocks (i.e. decrypts) the <see cref="SecureArchive"/> with the given <see cref="UserKey"/>.
        /// </summary>
        /// <param name="userKey">The <see cref="UserKey"/> to use to unlock the archive.</param>
        public void Unlock(UserKey userKey)
        {
            ArgCheck.NotNull(userKey, nameof(userKey));

            // Setting the ArchiveKey property puts the archive into the "unlocked" state.
            // Wait to set the property until after everything is properly unlocked.
            var archiveKey = this.DecryptArchiveKey(userKey);

            // When the archive is loaded from disk, we also load an HMAC of the file data
            // which needs to be validated with the archive key. Check that now.
            if (this.LoadedArchiveDataToBeVerified.Hash != null
                && this.LoadedArchiveDataToBeVerified.Data != null)
            {
                var actualHash = archiveKey.ComputeHmacSha256(this.LoadedArchiveDataToBeVerified.Data);

                if (!CryptoHelpers.SecureEquals(actualHash, this.LoadedArchiveDataToBeVerified.Hash))
                {
                    throw new ArchiveCorruptedException(
                        "The archive's hash does not match the loaded data. The archive may have been tampered with.");
                }
            }

            this.FileIndex = this.EncryptedFileIndex is null || this.EncryptedFileIndex.IsEmpty
                ? new FileIndex()
                : BondHelpers.DecryptAndDeserialize<FileIndex>(this.EncryptedFileIndex, archiveKey, this.SecuritySettings);

            this.ArchiveKey = archiveKey;
        }

        /// <summary>
        /// Encrypts and saves the archive to disk.
        /// </summary>
        public void Save()
        {
            if (this.IsLocked)
            {
                throw new UnauthorizedException("Unable to save archive because it is locked.");
            }

            this.ReserializeIndex();

            if (this.IsDirty)
            {
                // Record the last time the archive was modified.
                this.LastModifiedTime = DateTime.UtcNow;
            }

            // See Aegis file format documentation in file Aegis.bond
            var fileVersion = BitConverter.GetBytes(this.FileVersion);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(fileVersion);
            }

            var archiveBytes = BondHelpers.Serialize(this);
            var archiveHash = this.ArchiveKey.ComputeHmacSha256(archiveBytes);

            Debug.Assert(fileVersion != null && fileVersion.Length == 4, "The fileVersion field size is wrong!");
            Debug.Assert(archiveHash != null && archiveHash.Length == 32, "The aechiveHash size is wrong!");

            using var fileStream = File.OpenWrite(this.FullFilePath);
            fileStream.Write(fileVersion);
            fileStream.Write(archiveHash);
            fileStream.Write(archiveBytes);

            this.IsDirty = false;
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

            var keyAuthorizationRecord = this.UserKeyAuthorizations.FirstOrDefault(
                k => CryptoHelpers.SecureEquals(k.KeyId, userKey.KeyId));

            if (keyAuthorizationRecord is null
                || !keyAuthorizationRecord.TryDecryptArchiveKey(userKey, this.SecuritySettings, out var archiveKey))
            {
                throw new UnauthorizedException(error);
            }

            // Check the auth canary.
            Guid decryptedCanary;

            try
            {
                decryptedCanary = new Guid(archiveKey.Decrypt(this.CryptoStrategy, this.AuthCanary));
            }
            catch
            {
                throw new UnauthorizedException(error);
            }

            if (!CryptoHelpers.SecureEquals(decryptedCanary.ToByteArray(), ((Guid)this.Id).ToByteArray()))
            {
                throw new UnauthorizedException(error);
            }

            // Congrats, you're authorized!
            return archiveKey;
        }

        /// <summary>
        /// Encrypts and serializes the file index.
        /// </summary>
        private void ReserializeIndex()
        {
            // Be extra cautious to avoid corrupting existing archives.
            if (this.IsLocked)
            {
                throw new InvalidOperationException("Attempted to reserialize index while archive is locked!");
            }

            this.EncryptedFileIndex = BondHelpers.SerializeAndEncrypt(this.FileIndex, this.ArchiveKey, this.SecuritySettings);
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
                }

                this.isDisposed = true;
            }
        }

        #endregion
    }
}
