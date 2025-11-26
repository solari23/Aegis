namespace Aegis.Passkeys.WebAuthNInterop;

/// <summary>
/// Allows relying party to communicate hints to the user agent about how a request may be best completed.
/// See: https://w3c.github.io/webauthn/#enum-hints
/// </summary>
internal class PublicKeyCredentialHint
{
    /// <summary>
    /// Physical security keys such as YubiKey.
    /// </summary>
    /// <remarks>
    /// For compatibility with older user agents, when this hint is is used,
    /// authenticatorAttachment hint should be set to 'cross-platform'.
    /// </remarks>
    public const string SecurityKey = "security-key";

    /// <summary>
    /// Platform authenticators built into the client device.
    /// </summary>
    /// <remarks>
    /// For compatibility with older user agents, when this hint is is used,
    /// authenticatorAttachment hint should be set to 'platform'.
    /// </remarks>
    public const string ClientDevice = "client-device";

    /// <summary>
    /// General-purpose authenticators such as smartphones.
    /// </summary>
    /// <remarks>
    /// For compatibility with older user agents, when this hint is is used,
    /// authenticatorAttachment hint should be set to 'cross-platform'.
    /// </remarks>
    public const string Hybrid = "hybrid";
}
