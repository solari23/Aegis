using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

internal struct WEBAUTHN_CREDENTIAL_DETAILS
{
    public uint dwVersion;

    // DWORD cbCredentialID
    // PBYTE pbCredentialID
    public byte[]? credentialID;

    public WEBAUTHN_RP_ENTITY_INFORMATION pRpInformation;

    public WEBAUTHN_USER_ENTITY_INFORMATION pUserInformation;

    public bool bRemovable;

    public bool bBackedUp;

    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
#pragma warning disable 0649
            public uint dwVersion;
            public uint cbCredentialID;
            public IntPtr pbCredentialID;
            public IntPtr pRpInformation;
            public IntPtr pUserInformation;
            public int bRemovable;
            public int bBackedUp;
#pragma warning restore 0649
        }

        public static WEBAUTHN_CREDENTIAL_DETAILS ConvertToManaged(Unmanaged unmanaged)
        {
            return new WEBAUTHN_CREDENTIAL_DETAILS
            {
                dwVersion = unmanaged.dwVersion,
                credentialID = new SizePrefixedByteArray(unmanaged.cbCredentialID, unmanaged.pbCredentialID).ToManagedArray(),

                pRpInformation = unmanaged.pRpInformation == IntPtr.Zero
                    ? default
                    : WEBAUTHN_RP_ENTITY_INFORMATION.Marshaller.ConvertToManaged(
                        MarshalHelper.PtrToStruct<WEBAUTHN_RP_ENTITY_INFORMATION.Marshaller.Unmanaged>(unmanaged.pRpInformation)!.Value),

                pUserInformation = unmanaged.pUserInformation == IntPtr.Zero
                    ? default
                    : WEBAUTHN_USER_ENTITY_INFORMATION.Marshaller.ConvertToManaged(
                        MarshalHelper.PtrToStruct<WEBAUTHN_USER_ENTITY_INFORMATION.Marshaller.Unmanaged>(unmanaged.pUserInformation)!.Value),

                bRemovable = unmanaged.bRemovable != 0,
                bBackedUp = unmanaged.bBackedUp != 0,
            };
        }
    }
}
