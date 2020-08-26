namespace Aegis.Models
{
    /// <summary>
    /// Defines the key derivation functions supported by Aegis.
    /// </summary>
    public enum KeyDerivationFunction
    {
        /// <summary>
        /// Invalid value to indicate an uninitialized field.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Password-Based Key Derivation Function 2, using SHA256 as the base hash function.
        /// </summary>
        PBKDF2SHA256 = 1,
    }
}
