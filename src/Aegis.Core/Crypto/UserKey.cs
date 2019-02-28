﻿namespace Aegis.Core.Crypto
{
    using System;
    using System.Text;

    /// <summary>
    /// An encryption key derived from a user secret used to decrypt the archive's <see cref="ArchiveKey"/>.
    /// </summary>
    public class UserKey : Secret
    {
        /// <summary>
        /// Derives a <see cref="UserKey"/> from the input user secret.
        /// </summary>
        /// <param name="userSecret">The secret used to derive the key.</param>
        /// <param name="keyDerivationSalt">The salt used to derive the key.</param>
        /// <param name="securitySettings">Security parameters for the <see cref="SecureArchive"/>.</param>
        public static UserKey DeriveFrom(
            ReadOnlySpan<byte> userSecret,
            ReadOnlySpan<byte> keyDerivationSalt,
            SecuritySettings securitySettings)
        {
            ArgCheck.NotEmpty(userSecret, nameof(userSecret));
            ArgCheck.NotEmpty(keyDerivationSalt, nameof(keyDerivationSalt));
            // TODO: Validate input securitySettings

            var cryptoAlgoProperties = EncryptionAlgoProperties.For(securitySettings.EncryptionAlgo);

            // We'll use the user secret to derive the user key as well as the key ID.
            // The algorithm is:
            //   let N be the number of bytes for the key
            //   let M be the number of bytes for the keyId
            //   keyMatter := Derive N + M bytes from the secret using the KDF
            //   key := keyMatter[0..N-1]
            //   keyId := base64url(keyMatter[N..])
            var keyDerivationStrategy = CryptoHelpers.GetKeyDerivationStrategy(securitySettings.KeyDerivationFunction);
            var keyMatter = keyDerivationStrategy.DeriveKeyMatter(
                cryptoAlgoProperties.KeySizeInBytes + securitySettings.KeyIdSizeInBytes,
                userSecret,
                keyDerivationSalt,
                securitySettings.KeyDerivationWorkFactor);

            var key = keyMatter.Slice(0, cryptoAlgoProperties.KeySizeInBytes);
            var keyId = Helpers.Base64UrlEncode(keyMatter.Slice(cryptoAlgoProperties.KeySizeInBytes));

            return new UserKey(keyId, key.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserKey"/> class.
        /// </summary>
        /// <param name="keyId">The identifier for the user key.</param>
        /// <param name="key">The raw user key.</param>
        private UserKey(string keyId, byte[] key)
            : base(key)
        {
            this.KeyId = keyId;
        }

        /// <summary>
        /// Gets an identifer for the user key.
        /// </summary>
        public string KeyId { get; }

        /// <summary>
        /// Creates an authorization entry for the current user key for a particular archive.
        /// </summary>
        /// <param name="friendlyName">A friendly name to help the user identify the key.</param>
        /// <param name="archiveKey">The key used to encrypt the archive that the user key is being authorized for.</param>
        /// <param name="securitySettings">The archive's <see cref="SecuritySettings"/>.</param>
        /// <returns>The <see cref="UserKeyAuthorization"/> entry.</returns>
        internal UserKeyAuthorization CreateAuthorization(
            string friendlyName,
            ArchiveKey archiveKey,
            SecuritySettings securitySettings)
        {
            ArgCheck.NotEmpty(friendlyName, nameof(friendlyName));
            ArgCheck.NotNull(archiveKey, nameof(archiveKey));
            // TODO: Validate input securitySettings

            var additionalData = Encoding.UTF8.GetBytes(friendlyName + this.KeyId);

            var cryptoStrategy = CryptoHelpers.GetCryptoStrategy(securitySettings.EncryptionAlgo);
            var encryptedArchiveKey = cryptoStrategy.Encrypt(archiveKey.Key, this.Key, additionalData);

            return new UserKeyAuthorization(friendlyName, this.KeyId, encryptedArchiveKey);
        }
    }
}