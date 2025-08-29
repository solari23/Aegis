namespace Aegis.Passkeys;

/// <summary>
/// Large blob storage extension which RPs can query to store and retrieve large blobs of data on the authenticator.
/// THis is used e.g. for storing certificates on the authenticator.
/// See: https://www.w3.org/TR/webauthn-2/#enumdef-largeblobsupport
/// </summary>
/// <remarks>
/// Values are based on values defined for Win32 in webauthn.h:
///     #define WEBAUTHN_LARGE_BLOB_SUPPORT_NONE                                    0
///     #define WEBAUTHN_LARGE_BLOB_SUPPORT_REQUIRED                                1
///     #define WEBAUTHN_LARGE_BLOB_SUPPORT_PREFERRED                               2
/// May need to split off into separate platform-specific enums if more platforms are supported.
/// </remarks>
public enum LargeBlobSupport : uint
{
    None = 0,
    Required = 1,
    Preferred = 2,
}
