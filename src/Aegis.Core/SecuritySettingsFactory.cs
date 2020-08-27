using Aegis.Models;

namespace Aegis.Core
{
    /// <summary>
    /// Metadata that describes security settings (e.g. crypto algorithm choice) for the archive.
    /// </summary>
    /// <remarks>
    /// Data members for this class are defined in a Bond schema (see Aegis.bond).
    /// </remarks>
    public static class SecuritySettingsFactory
    {
        /// <summary>
        /// Gets the default <see cref="SecuritySettings"/>.
        /// </summary>
        public static SecuritySettings Default => Create(
            AegisConstants.DefaultEncryptionAlgo,
            AegisConstants.DefaultKeyDerivationFunction,
            AegisConstants.DefaultKeyDerivationWorkFactor,
            AegisConstants.DefaultKeyIdSizeInBytes);

        /// <summary>
        /// Creates a new instance of the <see cref="SecuritySettings"/> class.
        /// </summary>
        /// <param name="encryptionAlgo">The algorithm used to encrypt data in the archive.</param>
        /// <param name="keyDerivationFunction">The key derivation function (KDF) used to generate user keys.</param>
        /// <param name="keyDerivationWorkFactor">The work factor parameter (e.g. iteration count) for the KDF.</param>
        /// <param name="KeyIdSizeInBytes">The size (in bytes) of KeyIds generated for user keys.</param>
        public static SecuritySettings Create(
            EncryptionAlgo encryptionAlgo,
            KeyDerivationFunction keyDerivationFunction,
            int keyDerivationWorkFactor,
            int keyIdSizeInBytes)
        {
            ArgCheck.IsNot(EncryptionAlgo.Unknown, encryptionAlgo, nameof(encryptionAlgo));
            ArgCheck.IsNot(KeyDerivationFunction.Unknown, keyDerivationFunction, nameof(keyDerivationFunction));
            ArgCheck.GreaterThanZero(keyDerivationWorkFactor, nameof(keyDerivationWorkFactor));
            ArgCheck.GreaterThanZero(keyIdSizeInBytes, nameof(keyIdSizeInBytes));

            return new SecuritySettings
            {
                EncryptionAlgo = encryptionAlgo,
                KeyDerivationFunction = keyDerivationFunction,
                KeyDerivationWorkFactor = keyDerivationWorkFactor,
                KeyIdSizeInBytes = keyIdSizeInBytes,
            };
        }
    }
}
