using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

[NativeMarshalling(typeof(Marshaller))]
internal struct WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS()
{
    private uint dwVersion = 7;

    public uint dwTimeoutMilliseconds = 60_000;

    public WEBAUTHN_CREDENTIAL[] CredentialList = [];

    public WEBAUTHN_EXTENSION[] Extensions = [];

    public AuthenticatorAttachment dwAuthenticatorAttachment = 0;

    public bool bRequireResidentKey = false;

    public UserVerificationRequirement dwUserVerificationRequirement = 0;

    public AttestationConveyancePreference dwAttestationConveyancePreference = 0;

    public WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_FLAGS dwFlags = 0;

    // GUID *pCancellationId ==> Not Supported.

    // PWEBAUTHN_CREDENTIAL_LIST pExcludeCredentialList ==> Not Supported.

    // DWORD dwEnterpriseAttestation ==> Not Supported.

    public LargeBlobSupport dwLargeBlobSupport = 0;

    public bool bPreferResidentKey = false;

    public bool bBrowserInPrivateMode = false;

    public bool bEnablePrf = false;

    // PCTAPCBOR_HYBRID_STORAGE_LINKED_DATA pLinkedDevice ==> Not Supported.

    // DWORD cbJsonExt;
    // PBYTE pbJsonExt;
    public byte[]? jsonExt = null;

    [CustomMarshaller(typeof(WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS), MarshalMode.ManagedToUnmanagedRef, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public uint dwVersion;
            public uint dwTimeoutMilliseconds;
            public SizePrefixedArrayStruct CredentialList;
            public SizePrefixedArrayStruct Extensions;
            public uint dwAuthenticatorAttachment;
            public int bRequireResidentKey;
            public uint dwUserVerificationRequirement;
            public uint dwAttestationConveyancePreference;
            public uint dwFlags;
            public IntPtr pCancellationId;
            public IntPtr pExcludeCredentialList;
            public uint dwEnterpriseAttestation;
            public uint dwLargeBlobSupport;
            public int bPreferResidentKey;
            public int bBrowserInPrivateMode;
            public int bEnablePrf;
            public IntPtr pLinkedDevice;
            public uint cbJsonExt;
            public IntPtr pbJsonExt;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS managed)
        {
            var marshalledJsonExt = SizePrefixedArrayStruct.FromBytes(managed.jsonExt);

            var ret = new Unmanaged
            {
                dwVersion = managed.dwVersion,
                dwTimeoutMilliseconds = managed.dwTimeoutMilliseconds,

                CredentialList = SizePrefixedArrayStruct.FromArrayToContiguous(
                    managed.CredentialList,
                    WEBAUTHN_CREDENTIAL.Marshaller.ConvertToUnmanaged),

                Extensions = SizePrefixedArrayStruct.FromArrayToContiguous(
                    managed.Extensions,
                    WEBAUTHN_EXTENSION.Marshaller.ConvertToUnmanaged),

                dwAuthenticatorAttachment = (uint)managed.dwAuthenticatorAttachment,
                bRequireResidentKey = managed.bRequireResidentKey ? 1 : 0,
                dwUserVerificationRequirement = (uint)managed.dwUserVerificationRequirement,
                dwAttestationConveyancePreference = (uint)managed.dwAttestationConveyancePreference,
                dwFlags = (uint)managed.dwFlags,
                pCancellationId = IntPtr.Zero, // Not Supported.
                pExcludeCredentialList = IntPtr.Zero, // Not Supported.
                dwEnterpriseAttestation = 0, // Not Supported.
                dwLargeBlobSupport = (uint)managed.dwLargeBlobSupport,
                bPreferResidentKey = managed.bPreferResidentKey ? 1 : 0,
                bBrowserInPrivateMode = managed.bBrowserInPrivateMode ? 1 : 0,
                bEnablePrf = managed.bEnablePrf ? 1 : 0,
                pLinkedDevice = IntPtr.Zero, // Not Supported.
                cbJsonExt = marshalledJsonExt.NumElements,
                pbJsonExt = marshalledJsonExt.Pointer,
            };
            return ret;
        }

        // We don't use this method. It's only needed because we pass into managed code by ref.
        public static WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS ConvertToManaged(Unmanaged _) => default;

        public static void Free(Unmanaged unmanaged)
        {
            unmanaged.CredentialList.Free();
            unmanaged.Extensions.Free();
            SizePrefixedArrayStruct.Free(unmanaged.pbJsonExt);
        }
    }
}
