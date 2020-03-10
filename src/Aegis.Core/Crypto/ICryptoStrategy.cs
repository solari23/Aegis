using System;

namespace Aegis.Core.Crypto
{
    /// <summary>
    /// Standardized interface for cryptographic algorithm usage in Aegis.
    /// </summary>
    internal interface ICryptoStrategy
    {
        /// <summary>
        /// Gets the cryptographic algorithm's propertieries.
        /// </summary>
        EncryptionAlgoProperties AlgorithmProperties { get; }

        /// <summary>
        /// Interface for encrypting data.
        /// </summary>
        /// <param name="plainText">The data to encrypt.</param>
        /// <param name="key">The encryption key.</param>
        /// <param name="optionalAssociatedData">Unencrypted data that can optionally be checked for tampering when using authenticated ciphers.</param>
        /// <returns>The encrypted data.</returns>
        EncryptedPacket Encrypt(
            ReadOnlySpan<byte> plainText,
            ReadOnlySpan<byte> key,
            ReadOnlySpan<byte> optionalAssociatedData = default);

        /// <summary>
        /// Interface for decrypting data.
        /// </summary>
        /// <param name="encryptedData">The data to decrypt.</param>
        /// <param name="key">The decryption key.</param>
        /// <param name="optionalAssociatedData">Unencrypted data that can optionally be checked for tampering when using authenticated ciphers.</param>
        /// <returns>The decrypted data.</returns>
        Span<byte> Decrypt(
            EncryptedPacket encryptedData,
            ReadOnlySpan<byte> key,
            ReadOnlySpan<byte> optionalAssociatedData = default);
    }
}
