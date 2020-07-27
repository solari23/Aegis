using System;
using System.Security.Cryptography;

namespace Aegis.Core.Crypto
{
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
            ArgCheck.NotEmpty(keyData, nameof(keyData));

            this.InternalKeyData = keyData;
        }

        /// <summary>
        /// Gets the underlying secret key as a read-only data structure.
        /// </summary>
        internal ReadOnlySpan<byte> Key => this.InternalKeyData;

        /// <summary>
        /// Gets the underlying secret key data.
        /// </summary>
        private byte[] InternalKeyData { get; }

        /// <summary>
        /// Computes the HMAC-SHA256 keyed hash of the given data.
        /// </summary>
        /// <param name="dataToHash">The data to hash.</param>
        /// <returns>The HMAC-SHA256 hash of the data.</returns>
        internal Span<byte> ComputeHmacSha256(ReadOnlySpan<byte> dataToHash)
        {
            using var hmacProvider = new HMACSHA256(this.InternalKeyData);
            return hmacProvider.ComputeHash(dataToHash.ToArray());
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
