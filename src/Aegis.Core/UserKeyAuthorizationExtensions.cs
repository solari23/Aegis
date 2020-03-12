﻿using System;
using System.Text;

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
        public static bool TryDecryptArchiveKey(
            this UserKeyAuthorization authorization,
            UserKey userKey,
            SecuritySettings securitySettings,
            out ArchiveKey archiveKey)
        {
            ArgCheck.NotNull(authorization, nameof(authorization));
            ArgCheck.NotNull(userKey, nameof(userKey));
            // TODO: Validate input securitySettings

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