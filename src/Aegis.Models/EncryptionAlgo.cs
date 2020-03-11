namespace Aegis.Models
{
    /// <summary>
    /// Defines the encryption algorithms supported by Aegis.
    /// </summary>
    public enum EncryptionAlgo
    {
        /// <summary>
        /// Invalid value to indicate an uninitialized field.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// AES in GCM mode with a 256-bit key.
        /// </summary>
        Aes256Gcm = 1,
    }
}
