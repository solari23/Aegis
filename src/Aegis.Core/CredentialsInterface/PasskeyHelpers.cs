namespace Aegis.Core.CredentialsInterface;

/// <summary>
/// A collection of helpers that standardize working with Passkeys in Aegis.
/// </summary>
public static class PasskeyHelpers
{
    /// <summary>
    /// Gets the WebAuthN passkey Relying Party (RP) Identifier from the given Guid identifier.
    /// </summary>
    public static string GetRelyingPartyIdForIdentifier(Guid identifier) => $"https://{identifier:N}.aegis.local";

    /// <summary>
    /// Converts the given identifier to salt suitable for use with the WebAuthN HMAC (aka PRF) extension.
    /// </summary>
    public static Span<byte> IdentifierToHmacSalt(Guid identifier)
    {
        var salt = new byte[32];
        identifier.TryWriteBytes(salt, bigEndian: true, out _);
        return salt;
    }
}
