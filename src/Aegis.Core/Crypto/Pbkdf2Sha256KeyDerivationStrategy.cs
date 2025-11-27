using System.Security.Cryptography;

namespace Aegis.Core.Crypto;

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
        ArgCheck.GreaterThanZero(numBytesToDerive);
        ArgCheck.NotEmpty(secret);
        ArgCheck.NotEmpty(salt);
        ArgCheck.GreaterThanZero(workFactor);

        return Rfc2898DeriveBytes.Pbkdf2(secret.ToArray(), salt.ToArray(), workFactor, HashAlgorithmName.SHA256, numBytesToDerive);
    }
}
