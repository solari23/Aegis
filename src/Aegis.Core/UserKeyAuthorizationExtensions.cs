using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using Aegis.Core.CredentialsInterface;
using Aegis.Core.Crypto;
using Aegis.Models;

namespace Aegis.Core
{
    /// <summary>
    /// A collection of extension methods for the <see cref="UserKeyAuthorization"/> class.
    /// </summary>
    internal static class UserKeyAuthorizationExtensions
    {
        /// <summary>
        /// Creates a new <see cref="UserKeyAuthorization"/> entry for a user key and a particular archive.
        /// </summary>
        /// <param name="friendlyName">A friendly name to help the user identify the key.</param>
        /// <param name="userKey">The <see cref="UserKey"/> to authorize.</param>
        /// <param name="archiveKey">The key used to encrypt the archive that the user key is being authorized for.</param>
        /// <param name="securitySettings">The archive's <see cref="SecuritySettings"/>.</param>
        /// <returns>The new <see cref="UserKeyAuthorization"/> entry.</returns>
        public static UserKeyAuthorization CreateNewAuthorization(
            UserKeyAuthorizationParameters newKeyParams,
            ReadOnlySpan<byte> keyDerivationSalt,
            ArchiveKey archiveKey,
            SecuritySettings securitySettings)
        {
            ArgCheck.IsValid(newKeyParams, nameof(newKeyParams));
            ArgCheck.NotEmpty(keyDerivationSalt, nameof(keyDerivationSalt));
            ArgCheck.NotNull(archiveKey, nameof(archiveKey));
            ArgCheck.IsValid(securitySettings, nameof(securitySettings));

            using var userKey = UserKey.DeriveFrom(
                newKeyParams.UserSecret,
                keyDerivationSalt,
                securitySettings);

            // The SecureArchive file format requires that the friendly name and keyId be
            // checked for tampering when using authenticated cyphers.
            var additionalData = Encoding.UTF8.GetBytes(newKeyParams.FriendlyName + userKey.KeyId);

            var cryptoStrategy = CryptoHelpers.GetCryptoStrategy(securitySettings.EncryptionAlgo);
            var encryptedArchiveKey = userKey.EncryptSecret(cryptoStrategy, archiveKey, additionalData);

            return new UserKeyAuthorization
            {
                AuthorizationId = Guid.NewGuid(),
                FriendlyName = newKeyParams.FriendlyName,
                KeyId = userKey.KeyId,
                TimeAdded = DateTime.UtcNow,
                EncryptedArchiveKey = encryptedArchiveKey,
                SecretMetadata = newKeyParams.SecretMetadata,
            };
        }


        /// <summary>
        /// Attempts to use the given <see cref="UserKey"/> to decrypt the <see cref="ArchiveKey"/>.
        /// </summary>
        /// <param name="authorization">The target <see cref="UserKeyAuthorization"/> instance.</param>
        /// <param name="userKey">The <see cref="UserKey"/> to use.</param>
        /// <param name="securitySettings">The archive's <see cref="SecuritySettings"/>.</param>
        /// <param name="archiveKey">The decrypted <see cref="ArchiveKey"/>, or null if the input <see cref="UserKey"/> is not authorized.</param>
        /// <returns>True if the method was able to decrypt and false if decryption fails.</returns>
        /// <remarks>
        /// The decrypted <see cref="ArchiveKey"/> will still need to be tested to see if it can decrypt the <see cref="SecureArchive"/>.
        /// </remarks>
        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This is implementing a 'try' pattern.")]
        public static bool TryDecryptArchiveKey(
            this UserKeyAuthorization authorization,
            UserKey userKey,
            SecuritySettings securitySettings,
            out ArchiveKey archiveKey)
        {
            ArgCheck.NotNull(authorization, nameof(authorization));
            ArgCheck.NotNull(userKey, nameof(userKey));
            ArgCheck.IsValid(securitySettings, nameof(securitySettings));

            archiveKey = null;

            if (!CryptoHelpers.SecureEquals(userKey.KeyId, authorization.KeyId))
            {
                return false;
            }

            try
            {
                // The SecureArchive file format requires that the friendly name and keyId be
                // checked for tampering when using authenticated cyphers.
                var additionalData = Encoding.UTF8.GetBytes(authorization.FriendlyName + authorization.KeyId);

                var cryptoStrategy = CryptoHelpers.GetCryptoStrategy(securitySettings.EncryptionAlgo);
                var decryptedArchiveKey = userKey.Decrypt(cryptoStrategy, authorization.EncryptedArchiveKey, additionalData);

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
