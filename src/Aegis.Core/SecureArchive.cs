namespace Aegis.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Aegis.Core.Crypto;

    /// <summary>
    /// The core data structure that represents SecureArchive files. The SecureArchive is a
    /// self-contained file that holds the user's encrypted documents.
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
        /// <param name="creationParameters">The <see cref="SecureArchiveCreationParameters"/> to use when creating the archive.</param>
        /// <returns>A new <see cref="SecureArchive"/>.</returns>
        public static SecureArchive CreateNew(SecureArchiveCreationParameters creationParameters)
        {
            // TODO: Validate the input creation parameters

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
            };

            return archive;
        }

        /// <summary>
        /// Gets the <see cref="ICryptoStrategy"/> configured for the archive.
        /// </summary>
        internal ICryptoStrategy CryptoStrategy => CryptoHelpers.GetCryptoStrategy(this.SecuritySettings.EncryptionAlgo);

        /// <summary>
        /// Gets the archive encryption key, or null if the archive is locked.
        /// </summary>
        private ArchiveKey ArchiveKey { get; set; }

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

            // TODO: Decrypt the file index.
            
            this.ArchiveKey = archiveKey;
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
            const string error = "The user key is not authorized to unlock the archive!";

            var keyAuthorizationRecord = this.UserKeyAuthorizations.FirstOrDefault(k => k.KeyId == userKey.KeyId);

            if (keyAuthorizationRecord is null
                || !keyAuthorizationRecord.TryAuthorize(userKey, this.SecuritySettings, out var archiveKey))
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

            if (decryptedCanary != this.Id)
            {
                throw new UnauthorizedException(error);
            }

            // Congrats, you're authorized!
            return archiveKey;
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
