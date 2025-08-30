namespace Aegis.Passkeys.WebAuthNInterop;

/// <summary>
/// A WebAuthn Relying Party may require user verification for some of its operations but not for others, and may use this type to express its needs.
/// See: https://www.w3.org/TR/webauthn-2/#enumdef-userverificationrequirement
/// </summary>
/// <remarks>
/// Values are based on values defined for Win32 in webauthn.h:
///     #define WEBAUTHN_USER_VERIFICATION_REQUIREMENT_ANY                          0
///     #define WEBAUTHN_USER_VERIFICATION_REQUIREMENT_REQUIRED                     1
///     #define WEBAUTHN_USER_VERIFICATION_REQUIREMENT_PREFERRED                    2
///     #define WEBAUTHN_USER_VERIFICATION_REQUIREMENT_DISCOURAGED                  3
/// May need to split off into separate platform-specific enums if more platforms are supported.
/// </remarks>
public enum UserVerificationRequirement : uint
{
    Any = 0,
    Required = 1,
    Preferred = 2,
    Discouraged = 3,
}
