namespace Aegis.Core.Crypto
{
    using System;

    /// <summary>
    /// Base class for cryptographic secrets that need to be securely managed.
    /// </summary>
    public abstract class Secret : IDisposable
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Secret"/> class.
        /// </summary>
        /// <param name="keyData">The secret key data. This container should be considered the owner for this data.</param>
        protected Secret(byte[] keyData)
        {
            this.InternalKeyData = keyData;
        }

        /// <summary>
        /// Gets the underlying secret key as a read-only data structure.
        /// </summary>
        protected ReadOnlySpan<byte> Key => this.InternalKeyData;

        /// <summary>
        /// Gets the underlying secret key data.
        /// </summary>
        private byte[] InternalKeyData { get; }

        /// <summary>
        /// Encrypts the given <see cref="Secret"/>.
        /// </summary>
        /// <param name="cryptoStrategy">The cryptographic strategy to use.</param>
        /// <param name="otherSecret">The <see cref="Secret"/> to encrypt.</param>
        /// <returns>The encrypted <see cref="Secret"/>.</returns>
        internal EncryptedPacket EncryptSecret(ICryptoStrategy cryptoStrategy, Secret otherSecret)
            => this.Encrypt(cryptoStrategy, otherSecret.Key);

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
        protected virtual void Dispose(bool disposing)
        {
            if (!this.isDisposed)
            {
                if (disposing)
                {
                    // Clear out the key.
                    ((Span<byte>)this.InternalKeyData).Fill(0);
                }

                this.isDisposed = true;
            }
        }

        #endregion
    }
}
