namespace Aegis.Passkeys.Windows.Structures;

[Flags]
internal enum WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_FLAGS : uint
{
    None = 0,

    WEBAUTHN_AUTHENTICATOR_HMAC_SECRET_VALUES_FLAG = 0x0010_0000U,
}
