namespace Aegis.Core
{
    /// <summary>
    /// Metadata that describes security settings (e.g. crypto algorithm choice) for the archive.
    /// </summary>
    /// <remarks>
    /// Data members for this class are defined in a Bond schema (see Aegis.bond).
    /// </remarks>
    public partial class SecuritySettings
    {
        /// <summary>
        /// Gets the default <see cref="SecuritySettings"/>.
        /// </summary>
        public static SecuritySettings Default => new SecuritySettings(
            Constants.DefaultEncryptionAlgo,
            Constants.DefaultKeyDerivationFunction,
            Constants.DefaultKeyDerivationWorkFactor);

        /// <summary>
        /// Initializes a new instance of the <see cref="SecuritySettings"/> class.
        /// </summary>
        /// <param name="encryptionAlgo">The algorithm used to encrypt data in the archive.</param>
        /// <param name="keyDerivationFunction">The key derivation function (KDF) used to generate user keys.</param>
        /// <param name="keyDerivationWorkFactor">The work factor parameter (e.g. iteration count) for the KDF.</param>
        public SecuritySettings(
            EncryptionAlgo encryptionAlgo,
            KeyDerivationFunction keyDerivationFunction,
            int keyDerivationWorkFactor)
        {
            ArgCheck.IsNot(EncryptionAlgo.Unknown, encryptionAlgo, nameof(encryptionAlgo));
            ArgCheck.IsNot(KeyDerivationFunction.Unknown, keyDerivationFunction, nameof(keyDerivationFunction));
            ArgCheck.NotNegative(keyDerivationWorkFactor, nameof(keyDerivationWorkFactor));
            ArgCheck.IsNot(0, keyDerivationWorkFactor, nameof(keyDerivationWorkFactor));

            this.EncryptionAlgo = encryptionAlgo;
            this.KeyDerivationFunction = keyDerivationFunction;
            this.KeyDerivationWorkFactor = keyDerivationWorkFactor;
        }
    }
}
