using Aegis.Models;

namespace Aegis.Core.Crypto
{
    /// <summary>
    /// An encryption key used to encrypt data in the <see cref="SecureArchive"/>.
    /// </summary>
    internal class ArchiveKey : Secret
    {
        /// <summary>
        /// Creates a new symmetric <see cref="ArchiveKey"/> that can be used with the
        /// algorithms described in the given <see cref="SecuritySettings"/>.
        /// </summary>
        /// <param name="securitySettings">The <see cref="SecuritySettings"/> for the archive.</param>
        /// <returns>The new <see cref="ArchiveKey"/>.</returns>
        public static ArchiveKey CreateNew(SecuritySettings securitySettings)
        {
            // TODO: Validate input securitySettings

            var numKeyBytes = EncryptionAlgoProperties.For(securitySettings.EncryptionAlgo).KeySizeInBytes;
            return new ArchiveKey(CryptoHelpers.GetRandomBytes(numKeyBytes));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveKey"/> class.
        /// </summary>
        /// <param name="keyData">The raw archive encryption key.</param>
        public ArchiveKey(byte[] keyData)
            : base(keyData)
        {
            // Empty.
        }
    }
}
