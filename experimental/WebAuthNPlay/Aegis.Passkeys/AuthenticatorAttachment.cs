namespace Aegis.Passkeys;

/// <summary>
/// This enumeration’s values describe authenticators' attachment modalities.
/// Relying Parties use this to express a preferred authenticator attachment modality.
/// See: https://www.w3.org/TR/webauthn-2/#enum-attachment
/// </summary>
/// <remarks>
/// Values are based on values defined for Win32 in webauthn.h:
///     #define WEBAUTHN_AUTHENTICATOR_ATTACHMENT_ANY                               0
///     #define WEBAUTHN_AUTHENTICATOR_ATTACHMENT_PLATFORM                          1
///     #define WEBAUTHN_AUTHENTICATOR_ATTACHMENT_CROSS_PLATFORM                    2
///     #define WEBAUTHN_AUTHENTICATOR_ATTACHMENT_CROSS_PLATFORM_U2F_V2             3
/// May need to split off into separate platform-specific enums if more platforms are supported.
/// </remarks>
public enum AuthenticatorAttachment : uint
{
    Any = 0,
    Platform = 1,
    CrossPlatform = 2,
    CrossPlatformU2F = 3,
}
