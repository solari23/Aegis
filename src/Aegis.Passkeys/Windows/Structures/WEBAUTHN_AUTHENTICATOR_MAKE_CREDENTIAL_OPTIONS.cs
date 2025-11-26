using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;
using Aegis.Passkeys.WebAuthNInterop;

namespace Aegis.Passkeys.Windows.Structures;

[NativeMarshalling(typeof(Marshaller))]
internal struct WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS()
{
    private uint dwVersion = 8;

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

    // DWORD cbJsonExt
    // PBYTE pbJsonExt
    public byte[]? jsonExt = null;

    private WEBAUTHN_HMAC_SECRET_SALT? _pPRFGlobalEval = null;
    public WEBAUTHN_HMAC_SECRET_SALT? pPRFGlobalEval
    {
        readonly get => this._pPRFGlobalEval;
        set
        {
            if (value is null)
            {
                // Unset flag.
                dwFlags &= ~WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_FLAGS.WEBAUTHN_AUTHENTICATOR_HMAC_SECRET_VALUES_FLAG;
                bEnablePrf = false;
            }
            else
            {
                // Set flag.
                dwFlags |= WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS_FLAGS.WEBAUTHN_AUTHENTICATOR_HMAC_SECRET_VALUES_FLAG;
                bEnablePrf = true;
            }
            _pPRFGlobalEval = value;
        }
    }

    // DWORD cCredentialHints
    // LPCWSTR *ppwszCredentialHints
    public string[]? credentialHints = null;

    public bool bAutoFill = false;

    [CustomMarshaller(typeof(WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS), MarshalMode.ManagedToUnmanagedRef, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public uint dwVersion;
            public uint dwTimeoutMilliseconds;
            public SizePrefixedContiguousStuctArray CredentialList;
            public SizePrefixedContiguousStuctArray Extensions;
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
            public IntPtr pPRFGlobalEval;
            public uint cCredentialHints;
            public IntPtr ppwszCredentialHints;
            public int bAutoFill;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS managed)
        {
            var marshalledJsonExt = SizePrefixedByteArray.From(managed.jsonExt);
            var marshalledCredentialHints = SizePrefixedStringArray.From(managed.credentialHints);

            var marshalledPRFGlobalEval = IntPtr.Zero;
            if (managed.pPRFGlobalEval.HasValue)
            {
                var marshalledStruct = WEBAUTHN_HMAC_SECRET_SALT.Marshaller.ConvertToUnmanaged(managed.pPRFGlobalEval.Value);
                marshalledPRFGlobalEval = MarshalHelper.StructToPtr(marshalledStruct);
            }

            var ret = new Unmanaged
            {
                dwVersion = managed.dwVersion,
                dwTimeoutMilliseconds = managed.dwTimeoutMilliseconds,

                CredentialList = SizePrefixedContiguousStuctArray.From(
                    managed.CredentialList,
                    WEBAUTHN_CREDENTIAL.Marshaller.ConvertToUnmanaged),

                Extensions = SizePrefixedContiguousStuctArray.From(
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
                pPRFGlobalEval = marshalledPRFGlobalEval,
                cCredentialHints = marshalledCredentialHints.NumElements,
                ppwszCredentialHints = marshalledCredentialHints.Pointer,
                bAutoFill = managed.bAutoFill ? 1 : 0,
            };
            return ret;
        }

        // We don't use this method. It's only needed because we pass into managed code by ref.
        public static WEBAUTHN_AUTHENTICATOR_MAKE_CREDENTIAL_OPTIONS ConvertToManaged(Unmanaged _) => default;

        public static void Free(Unmanaged unmanaged)
        {
            unmanaged.CredentialList.Free<WEBAUTHN_CREDENTIAL.Marshaller.Unmanaged>(
                WEBAUTHN_CREDENTIAL.Marshaller.Free);
            unmanaged.Extensions.Free<WEBAUTHN_EXTENSION.Marshaller.Unmanaged>(
                WEBAUTHN_EXTENSION.Marshaller.Free);

            SizePrefixedByteArray.Free(unmanaged.pbJsonExt);

            if (unmanaged.pPRFGlobalEval != IntPtr.Zero)
            {
                WEBAUTHN_HMAC_SECRET_SALT.Marshaller.Free(
                    *(WEBAUTHN_HMAC_SECRET_SALT.Marshaller.Unmanaged*)unmanaged.pPRFGlobalEval);
                NativeMemory.Free((void*)unmanaged.pPRFGlobalEval);
            }

            new SizePrefixedStringArray(unmanaged.cCredentialHints, unmanaged.ppwszCredentialHints)
                .Free();
        }
    }
}
