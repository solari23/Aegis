namespace Aegis.Passkeys;

/// <summary>
/// Information about the Relying Party (RP) for WebAuthn operations.
/// </summary>
public class RelyingPartyInfo
{
    /// <summary>
    /// The Relying Party ID (RP ID), which must be a valid domain string.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The origin (scheme + host + port) of the caller, which must match the RP ID.
    /// In addition:
    ///   - The scheme must be 'https'
    ///   - The port is unrestricted
    /// </summary>
    public required string Origin { get; init; }

    /// <summary>
    /// A human-friendly name for the Relying Party (optional).
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// A URL to an icon image (optional).
    /// </summary>
    public string? IconUrl { get; init; }
}
