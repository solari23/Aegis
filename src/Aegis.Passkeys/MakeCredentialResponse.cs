namespace Aegis.Passkeys;

/// <summary>
/// The response from a call to <see cref="PasskeyManager.MakeCredentialWithHmacSecret"/>.
/// </summary>
public class MakeCredentialResponse
{
    /// <summary>
    /// The identifier of the newly created credential.
    /// </summary>
    public required Identifier NewCredentialId { get; init; }
}
