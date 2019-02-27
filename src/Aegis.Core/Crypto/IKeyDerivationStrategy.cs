namespace Aegis.Core.Crypto
{
    using System;

    /// <summary>
    /// Standardized interface for key derivation function usage in Aegis.
    /// </summary>
    internal interface IKeyDerivationStrategy
    {
        /// <summary>
        /// Executes the key derivation function to derive the requested number of bytes of key matter.
        /// </summary>
        /// <param name="numBytesToDerive">The number of bytes of key matter to derive.</param>
        /// <param name="secret">The secret to derive the key from.</param>
        /// <param name="salt">The salt to use when deriving the key.</param>
        /// <param name="workFactor">The work factor parameter to the derivation function.</param>
        /// <returns>The requested number of bytes of key matter.</returns>
        Span<byte> DeriveKeyMatter(
            int numBytesToDerive,
            ReadOnlySpan<byte> secret,
            ReadOnlySpan<byte> salt,
            int workFactor);
    }
}
