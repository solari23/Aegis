namespace Aegis.Core.Crypto
{
    using System;
    using System.Security.Cryptography;

    /// <summary>
    /// Implements key derivation using the PBKDF2 algorithm descibed in RFC 2898.
    /// </summary>
    internal class Pbkdf2KeyDerivationStrategy : IKeyDerivationStrategy
    {
        /// <inheritdoc />
        public Span<byte> DeriveKeyMatter(
            int numBytesToDerive,
            ReadOnlySpan<byte> secret,
            ReadOnlySpan<byte> salt,
            int workFactor)
        {
            ArgCheck.GreaterThanZero(numBytesToDerive, nameof(numBytesToDerive));
            ArgCheck.NotEmpty(secret, nameof(secret));
            ArgCheck.NotEmpty(salt, nameof(salt));
            ArgCheck.GreaterThanZero(workFactor, nameof(workFactor));

            using var kdf = new Rfc2898DeriveBytes(secret.ToArray(), salt.ToArray(), workFactor);
            return kdf.GetBytes(numBytesToDerive);
        }
    }
}
