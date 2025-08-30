namespace Aegis.Passkeys.WebAuthNInterop;

/// <summary>
/// WebAuthn Relying Parties may use AttestationConveyancePreference to specify their preference regarding attestation conveyance during credential generation.
/// See: https://www.w3.org/TR/webauthn-2/#enum-attestation-convey
/// </summary>
/// <remarks>
/// Values are based on values defined for Win32 in webauthn.h:
///     #define WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_ANY                      0
///     #define WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_NONE                     1
///     #define WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_INDIRECT                 2
///     #define WEBAUTHN_ATTESTATION_CONVEYANCE_PREFERENCE_DIRECT                   3
/// May need to split off into separate platform-specific enums if more platforms are supported.
/// </remarks>
public enum AttestationConveyancePreference : uint
{
    Any = 0,
    None = 1,
    Indirect = 2,
    Direct = 3,
}
