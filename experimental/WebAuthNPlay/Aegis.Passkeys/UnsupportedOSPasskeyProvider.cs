namespace Aegis.Passkeys;

/// <summary>
/// Implementation of <see cref="IPasskeyProvider"/> for unsupported operating systems.
/// </summary>
internal class UnsupportedOSPasskeyProvider : IPasskeyProvider
{
    /// <inheritdoc />
    public bool IsHmacSecretSupported() => false;

    /// <inheritdoc />
    public GetHmacSecretResponse GetHmacSecret(RelyingPartyInfo rpInfo, HmacSecret salt, HmacSecret? secondSalt, IReadOnlyList<Identifier> allowedCredentialIds)
        => throw new NotImplementedException();

    /// <inheritdoc />
    public MakeCredentialResponse MakeCredentialWithHmacSecret(RelyingPartyInfo rpInfo, UserEntityInfo userInfo, UserVerificationRequirement userVerificationRequirement)
        => throw new NotImplementedException();
}
