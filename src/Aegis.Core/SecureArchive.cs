namespace Aegis.Core
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

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
            var firstUserKeyAuthorization = firstUserKey.CreateAuthorization(
                creationParameters.UserKeyFriendlyName,
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
                AuthorizedUserKeys = new List<AuthorizedUserKey> { firstUserKeyAuthorization },

                // Non-serialized properties.
                ArchiveKey = archiveKey,
            };

            return archive;
        }

        /// <summary>
        /// Gets the archive encryption key, or null if the archive is locked.
        /// </summary>
        private ArchiveKey ArchiveKey { get; set; }

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
