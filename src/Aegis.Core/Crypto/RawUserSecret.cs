namespace Aegis.Core.Crypto;

/// <summary>
/// Container for a user secret used to derive user keys.
/// </summary>
public sealed class RawUserSecret : Secret
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RawUserSecret"/> class.
    /// </summary>
    /// <param name="keyData">The raw archive encryption key.</param>
    public RawUserSecret(byte[] keyData)
        : base(keyData)
    {
        // Empty.
    }
}
