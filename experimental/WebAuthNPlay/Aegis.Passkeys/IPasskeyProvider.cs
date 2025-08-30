namespace Aegis.Passkeys;

internal interface IPasskeyProvider
{
    /// <summary>
    /// Checks if the current platform supports HMAC secret generation for passkeys.
    /// </summary>
    bool IsHmacSecretSupported();

    /// <summary>
    /// Generates an HMAC secret from a passkey authenticator.
    /// </summary>
    GetHmacSecretResponse GetHmacSecret(RelyingPartyInfo rpInfo, HmacSecret salt, HmacSecret? secondSalt, IReadOnlyList<Identifier> allowedCredentialIds);

    /// <summary>
    /// Makes a new passkey credential with an HMAC secret.
    /// </summary>
    MakeCredentialResponse MakeCredentialWithHmacSecret(RelyingPartyInfo rpInfo, UserEntityInfo userInfo, UserVerificationRequirement userVerificationRequirement);
}
