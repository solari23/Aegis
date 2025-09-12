using Aegis.Passkeys.WebAuthNInterop;

namespace Aegis.Passkeys;

internal interface IPasskeyProvider
{
    /// <summary>
    /// Checks if the current platform supports HMAC secret generation for passkeys.
    /// </summary>
    bool IsHmacSecretSupported();

    /// <summary>
    /// Checks if the current platform supports HMAC secret generation during MakeCredential.
    /// </summary>
    /// <remarks>
    /// If false, then <see cref="MakeCredentialWithHmacSecret"/> won't return HMAC secrets. A second call to
    /// <see cref="GetHmacSecret"/> would be needed.
    ///
    /// Example: Windows only added support for HMAC secret generation during MakeCredential in Windows 11 25H2.
    /// </remarks>
    bool IsHmacGenerationDuringMakeCredentialSupported();

    /// <summary>
    /// Generates an HMAC secret from a passkey authenticator.
    /// </summary>
    GetHmacSecretResponse GetHmacSecret(RelyingPartyInfo rpInfo, HmacSecret salt, HmacSecret? secondSalt, IReadOnlyList<Identifier> allowedCredentialIds);

    /// <summary>
    /// Makes a new passkey credential with an HMAC secret.
    /// </summary>
    MakeCredentialResponse MakeCredentialWithHmacSecret(RelyingPartyInfo rpInfo, UserEntityInfo userInfo, HmacSecret? salt, HmacSecret? secondSalt, UserVerificationRequirement userVerificationRequirement);
}
