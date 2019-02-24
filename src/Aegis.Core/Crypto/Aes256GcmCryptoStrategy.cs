namespace Aegis.Core.Crypto
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Implements encryption using the AES algorithm in GCM mode with a 256-bit key.
    /// </summary>
    internal class Aes256GcmCryptoStrategy : ICryptoStrategy
    {
        /// <inheritdoc />
        public EncryptionAlgoProperties AlgorithmProperties => EncryptionAlgoProperties.Aes256GcmAlgoProperties;

        /// <inheritdoc />
        public EncryptedPacket Encrypt(ReadOnlySpan<byte> plainText, ReadOnlySpan<byte> key, ReadOnlySpan<byte> optionalAssociatedData = default)
        {
            ArgCheck.NotEmpty(plainText, nameof(plainText));
            ArgCheck.HasLength(this.AlgorithmProperties.KeySizeInBytes, key, nameof(key));

            var iv = CryptoHelpers.GetRandomBytes(this.AlgorithmProperties.IvSizeInBytes);

            // Containers to hold crypto operation outputs.
            var cipherText = new byte[plainText.Length];
            var authTag = new byte[this.AlgorithmProperties.AuthTagSizeInBytes];

            using var algo = new AesGcm(key);
            algo.Encrypt(iv, plainText, cipherText, authTag, optionalAssociatedData);

            return new EncryptedPacket(cipherText, iv, authTag);
        }

        /// <inheritdoc />
        public Span<byte> Decrypt(EncryptedPacket encryptedData, ReadOnlySpan<byte> key, ReadOnlySpan<byte> optionalAssociatedData = default)
        {
            ArgCheck.NotNull(encryptedData, nameof(encryptedData));
            ArgCheck.HasLength(this.AlgorithmProperties.KeySizeInBytes, key, nameof(key));

            // TODO: Check IV and AuthTag lengths

            // Containers to hold crypto operation outputs.
            var plainText = new byte[encryptedData.CipherText.Count];

            using var algo = new AesGcm(key);
            algo.Decrypt(
                encryptedData.IV.ToArray(),
                encryptedData.CipherText.ToArray(),
                encryptedData.AuthTag.ToArray(),
                plainText,
                optionalAssociatedData);

            return plainText;
        }
    }
}
