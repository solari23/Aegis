namespace Aegis.Core
{
    /// <summary>
    /// A collection of Aegis constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The current version number for Aegis <see cref="SecureArchive"/> files.
        /// </summary>
        public const int CurrentAegisSecureArchiveFileVersion = 1;

        /// <summary>
        /// Specifies the default encryption algorithm that Aegis will use.
        /// </summary>
        public const EncryptionAlgo DefaultEncryptionAlgo = EncryptionAlgo.Aes256Gcm;

        /// <summary>
        /// Specifies the default key derivation function that Aegis will use.
        /// </summary>
        public const KeyDerivationFunction DefaultKeyDerivationFunction = KeyDerivationFunction.PBKDF2;

        /// <summary>
        /// Species the default key derivation work factor that Aegis will use.
        /// </summary>
        public const int DefaultKeyDerivationWorkFactor = 100_000;

        /// <summary>
        /// Specifies the default length (in bytes) of the salt used in key derivations.
        /// </summary>
        public const int DefaultKeyDerivationSaltSizeInBytes = 16;
    }
}
