namespace Aegis.Passkeys.Windows.Structures;

internal enum HResult : uint
{
    S_OK = 0,

    NTE_PERM = 0x80090010,
    NTE_NOT_FOUND = 0x80090011,
    NTE_USER_CANCELLED = 0x80090036,
}
