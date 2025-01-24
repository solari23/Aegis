using Aegis.Models;

namespace Aegis.Core.Crypto;

/// <summary>
/// A <see cref="Secret"/> used for encryption.
/// </summary>
public abstract class EncryptionSecret : Secret
{
    /// <summary>
    /// Creates a new instance of the <see cref="EncryptionSecret"/> class.
    /// </summary>
    /// <param name="keyData">The secret key data. This container should be considered the owner for this data.</param>
    protected EncryptionSecret(byte[] keyData)
        : base(keyData)
    {
        // Empty.
    }

    /// <summary>
    /// Encrypts the given <see cref="Secret"/>.
    /// </summary>
    /// <param name="cryptoStrategy">The cryptographic strategy to use.</param>
    /// <param name="otherSecret">The <see cref="Secret"/> to encrypt.</param>
    /// <param name="optionalAssociatedData">Unencrypted data that can optionally be checked for tampering when using authenticated ciphers.</param>
    /// <returns>The encrypted <see cref="Secret"/>.</returns>
    internal EncryptedPacket WrapSecret(ICryptoStrategy cryptoStrategy, Secret otherSecret, ReadOnlySpan<byte> optionalAssociatedData = default)
        => this.Encrypt(cryptoStrategy, otherSecret.Key, optionalAssociatedData);

    /// <summary>
    /// Encrypts the given data using the secret.
    /// </summary>
    /// <param name="cryptoStrategy">The cryptographic strategy to use.</param>
    /// <param name="plainText">The data to encrypt.</param>
    /// <param name="optionalAssociatedData">Unencrypted data that can optionally be checked for tampering when using authenticated ciphers.</param>
    /// <returns>The encrypted data.</returns>
    internal EncryptedPacket Encrypt(ICryptoStrategy cryptoStrategy, ReadOnlySpan<byte> plainText, ReadOnlySpan<byte> optionalAssociatedData = default)
        => cryptoStrategy.Encrypt(plainText, this.Key, optionalAssociatedData);

    /// <summary>
    /// Decrypts the given ciphertext using the secret.
    /// </summary>
    /// <param name="cryptoStrategy">The cryptographic strategy to use.</param>
    /// <param name="encryptedData">The data to decrypt.</param>
    /// <param name="optionalAssociatedData">Unencrypted data that can optionally be checked for tampering when using authenticated ciphers.</param>
    /// <returns>The decrypted data.</returns>
    internal Span<byte> Decrypt(ICryptoStrategy cryptoStrategy, EncryptedPacket encryptedData, ReadOnlySpan<byte> optionalAssociatedData = default)
        => cryptoStrategy.Decrypt(encryptedData, this.Key, optionalAssociatedData);
}
