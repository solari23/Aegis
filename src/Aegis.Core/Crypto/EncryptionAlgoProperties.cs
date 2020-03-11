using System;
using System.ComponentModel;
using System.Diagnostics;

using Aegis.Models;

namespace Aegis.Core.Crypto
{
    /// <summary>
    /// Encapsulates constant properties about the available encryption algorithms.
    /// </summary>
    [ReadOnly(true)]
    internal sealed class EncryptionAlgoProperties
    {
        /// <summary>
        /// The <see cref="EncryptionAlgoProperties"/> for algorithm <see cref="EncryptionAlgo.Aes256Gcm"/>.
        /// </summary>
        public static EncryptionAlgoProperties Aes256GcmAlgoProperties { get; } = new EncryptionAlgoProperties(EncryptionAlgo.Aes256Gcm, 256 / 8, 12, 16);

        /// <summary>
        /// Gets the <see cref="EncryptionAlgoProperties"/> for a given <see cref="EncryptionAlgo"/>.
        /// </summary>
        /// <param name="algo">The <see cref="EncryptionAlgo"/> to get properties for.</param>
        /// <returns>The <see cref="EncryptionAlgoProperties"/>.</returns>
        public static EncryptionAlgoProperties For(EncryptionAlgo algo) => algo switch
        {
            EncryptionAlgo.Aes256Gcm => Aes256GcmAlgoProperties,
            _ => throw new InvalidOperationException($"No encryption properties defined for algorithm '{algo}'!"),
        };

        /// <summary>
        /// Creates a new instance of the <see cref="EncryptionAlgoProperties"/> class.
        /// </summary>
        /// <param name="keySizeInBytes">The size of the encryption key in bytes.</param>
        /// <param name="ivSizeInBytes">The size of the IV in bytes. Default is 0 for algorithms that do not use an IV.</param>
        /// <param name="authTagSizeInBytes">The size of the authentication tag in bytes. Default is 0 for unauthenticated encryption.</param>
        private EncryptionAlgoProperties(EncryptionAlgo algo, int keySizeInBytes, int ivSizeInBytes = 0, int authTagSizeInBytes = 0)
        {
            Debug.Assert(algo != EncryptionAlgo.Unknown);
            Debug.Assert(keySizeInBytes > 0);
            Debug.Assert(IvSizeInBytes >= 0);
            Debug.Assert(authTagSizeInBytes >= 0);

            this.Algorithm = algo;
            this.KeySizeInBytes = keySizeInBytes;
            this.IvSizeInBytes = ivSizeInBytes;
            this.AuthTagSizeInBytes = authTagSizeInBytes;
        }

        /// <summary>
        /// The algorithm's identifier.
        /// </summary>
        public EncryptionAlgo Algorithm { get; }

        /// <summary>
        /// The size of the encryption key in bytes.
        /// </summary>
        public int KeySizeInBytes { get; }

        /// <summary>
        /// The size of the IV in bytes, or 0 if the algorithm does not use an IV.
        /// </summary>
        public int IvSizeInBytes { get; }

        /// <summary>
        /// The size of the authentication tag in bytes, or 0 if the encryption is unauthenticated.
        /// </summary>
        public int AuthTagSizeInBytes { get; }

        /// <summary>
        /// Gets whether or not the encryption algorithm is authenticated.
        /// </summary>
        public bool IsAuthenticated => this.AuthTagSizeInBytes > 0;
    }
}
