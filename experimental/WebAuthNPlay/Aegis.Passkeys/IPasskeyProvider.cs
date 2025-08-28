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
    /// <returns>The generated HMAC secrets.</returns>
    GetHmacSecretResponse GetHmacSecret(RelyingPartyInfo rpInfo, HmacSecret salt, HmacSecret? secondSalt = null);
}
