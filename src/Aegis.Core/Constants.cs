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
        public const EncryptionAlgo DefaultEncryptionAlgorithm = EncryptionAlgo.Aes256Gcm;
    }
}
