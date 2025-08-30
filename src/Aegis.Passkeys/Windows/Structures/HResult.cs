namespace Aegis.Passkeys.Windows.Structures;

internal enum HResult : uint
{
    S_OK = 0,

    E_CANCELLED = 0x800704C7,

    NTE_PERM = 0x80090010,
    NTE_NOT_FOUND = 0x80090011,
    NTE_INVALID_PARAMETER = 0x80090027,
    NTE_USER_CANCELLED = 0x80090036,
}
