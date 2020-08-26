using System;
using System.Security.Cryptography;

namespace Aegis.Core.Crypto
{
    /// <summary>
    /// Implements key derivation using the PBKDF2 algorithm descibed in RFC 2898,
    /// with SHA256 used as the base hash function.
    /// </summary>
    internal class Pbkdf2Sha256KeyDerivationStrategy : IKeyDerivationStrategy
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

            using var kdf = new Rfc2898DeriveBytes(secret.ToArray(), salt.ToArray(), workFactor, HashAlgorithmName.SHA256);
            return kdf.GetBytes(numBytesToDerive);
        }
    }
}
