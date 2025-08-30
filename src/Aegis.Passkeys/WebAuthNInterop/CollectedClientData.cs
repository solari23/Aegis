using System.Text.Json.Serialization;

namespace Aegis.Passkeys.WebAuthNInterop;

/// <summary>
/// The client data represents the contextual bindings of both the WebAuthn Relying Party and the client.
/// See: https://www.w3.org/TR/webauthn-2/#dictionary-client-data
/// </summary>
public class CollectedClientData
{
    public static CollectedClientData ForGetAssertionCall(byte[] challenge, string origin, bool? crossOrigin = null)
        => new("webauthn.get", challenge, origin, crossOrigin ?? false);

    public static CollectedClientData ForMakeCredentialCall(byte[] challenge, string origin, bool? crossOrigin = null)
        => new("webauthn.create", challenge, origin, crossOrigin ?? false);

    private CollectedClientData(string type, byte[] challenge, string origin, bool crossOrigin)
    {
        if (challenge == null || challenge.Length == 0)
        {
            throw new ArgumentNullException(nameof(challenge));
        }

        if (string.IsNullOrEmpty(origin))
        {
            throw new ArgumentNullException(nameof(origin));
        }

        this.Type = type;
        this.Challenge = Convert.ToBase64String(challenge).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        this.Origin = origin;
        this.CrossOrigin = crossOrigin;
    }

    /// <summary>
    /// This member contains the string "webauthn.create" when creating new credentials, and "webauthn.get" when getting an
    /// assertion from an existing credential. The purpose of this member is to prevent certain types of signature confusion
    /// attacks (where an attacker substitutes one legitimate signature for another).
    /// </summary>
    [JsonPropertyName("type"), JsonPropertyOrder(0), JsonRequired]
    public string Type { get; set; }

    /// <summary>
    /// This member contains the base64url encoding of the challenge provided by the Relying Party.
    /// </summary>
    [JsonPropertyName("challenge"), JsonPropertyOrder(1), JsonRequired]
    public string Challenge { get; set; }

    /// <summary>
    /// This member contains the fully qualified origin of the requester, as provided to the authenticator by the client,
    /// in the syntax defined by [RFC6454].
    /// </summary>
    [JsonPropertyName("origin"), JsonPropertyOrder(2), JsonRequired]
    public string Origin { get; set; }

    /// <summary>
    /// This member contains the inverse of the sameOriginWithAncestors argument value that was passed into the internal method.
    /// </summary>
    [JsonPropertyName("crossOrigin"), JsonPropertyOrder(3)]
    public bool CrossOrigin { get; set; }
}
