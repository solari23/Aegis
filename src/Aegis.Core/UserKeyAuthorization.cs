namespace Aegis.Core
{
    using Aegis.Core.Crypto;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;

    /// <summary>
    /// Metadata associated with user keys authorized to unlock the SecureArchive.
    /// </summary>
    /// <remarks>
    /// Data members for this class are defined in a Bond schema (see Aegis.bond).
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1047:DoNotDeclareProtectedMembersInSealedTypes", Justification = "The protected ctor is code generated.")]
    public sealed partial class UserKeyAuthorization
    {
        /// <summary>
        /// Creates a new <see cref="UserKeyAuthorization"/> entry for a user key and a particular archive.
        /// </summary>
        /// <param name="friendlyName">A friendly name to help the user identify the key.</param>
        /// <param name="userKey">The <see cref="UserKey"/> to authorize.</param>
        /// <param name="archiveKey">The key used to encrypt the archive that the user key is being authorized for.</param>
        /// <param name="securitySettings">The archive's <see cref="SecuritySettings"/>.</param>
        /// <returns>The new <see cref="UserKeyAuthorization"/> entry.</returns>
        internal static UserKeyAuthorization CreateNewAuthorization(
            string friendlyName,
            UserKey userKey,
            ArchiveKey archiveKey,
            SecuritySettings securitySettings)
        {
            ArgCheck.NotEmpty(friendlyName, nameof(friendlyName));
            ArgCheck.NotNull(userKey, nameof(userKey));
            ArgCheck.NotNull(archiveKey, nameof(archiveKey));
            // TODO: Validate input securitySettings

            // The SecureArchive file format requires that the friendly name and keyId be
            // checked for tampering when using authenticated cyphers.
            var additionalData = Encoding.UTF8.GetBytes(friendlyName + userKey.KeyId);

            var cryptoStrategy = CryptoHelpers.GetCryptoStrategy(securitySettings.EncryptionAlgo);
            var encryptedArchiveKey = userKey.EncryptSecret(cryptoStrategy, archiveKey, additionalData);

            return new UserKeyAuthorization
            {
                FriendlyName = friendlyName,
                KeyId = userKey.KeyId,
                TimeAdded = DateTime.UtcNow,
                EncryptedArchiveKey = encryptedArchiveKey,
            };
        }

        /// <summary>
        /// Checks if the input <see cref="UserKey"/> is authorized to decrypt the archive.
        /// </summary>
        /// <param name="userKey">The <see cref="UserKey"/> to authorize.</param>
        /// <param name="securitySettings">The archive's <see cref="SecuritySettings"/>.</param>
        /// <param name="archiveKey">The decrypted <see cref="ArchiveKey"/>, or null if the input <see cref="UserKey"/> is not authorized.</param>
        /// <returns>True if the <see cref="UserKey"/> is authorized to decrypt the archive and false otherwise.</returns>
        internal bool TryAuthorize(UserKey userKey, SecuritySettings securitySettings, out ArchiveKey archiveKey)
        {
            ArgCheck.NotNull(userKey, nameof(userKey));

            archiveKey = null;

            if (userKey.KeyId != this.KeyId)
            {
                return false;
            }

            try
            {
                // The SecureArchive file format requires that the friendly name and keyId be
                // checked for tampering when using authenticated cyphers.
                var additionalData = Encoding.UTF8.GetBytes(this.FriendlyName + this.KeyId);

                var cryptoStrategy = CryptoHelpers.GetCryptoStrategy(securitySettings.EncryptionAlgo);
                var decryptedArchiveKey = userKey.Decrypt(cryptoStrategy, this.EncryptedArchiveKey, additionalData);

                if (!decryptedArchiveKey.IsEmpty)
                {
                    archiveKey = new ArchiveKey(decryptedArchiveKey.ToArray());
                }
            }
            catch
            {
                return false;
            }

            return archiveKey != null;
        }
    }
}
