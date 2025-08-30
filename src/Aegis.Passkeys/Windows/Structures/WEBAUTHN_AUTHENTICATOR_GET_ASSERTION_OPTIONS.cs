using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;
using Aegis.Passkeys.WebAuthNInterop;

namespace Aegis.Passkeys.Windows.Structures;

[NativeMarshalling(typeof(Marshaller))]
internal struct WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS()
{
    private uint dwVersion = 7;

    public uint dwTimeoutMilliseconds = 60_000;

    public WEBAUTHN_CREDENTIAL[] CredentialList = [];

    public WEBAUTHN_EXTENSION[] Extensions = [];

    public AuthenticatorAttachment dwAuthenticatorAttachment = 0;

    public UserVerificationRequirement dwUserVerificationRequirement = 0;

    public WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_FLAGS dwFlags = 0;

    // PCWSTR pwszU2fAppId ==> Not Supported.

    // BOOL *pbU2fAppId ==> Not Supported.

    // GUID *pCancellationId ==> Not Supported.

    public WEBAUTHN_CREDENTIAL_LIST? pAllowCredentialList = null;

    public uint dwCredLargeBlobOperation = 0;

    // DWORD cbCredLargeBlob
    // PBYTE pbCredLargeBlob
    public byte[]? credLargeBlob = null;

    private WEBAUTHN_HMAC_SECRET_SALT_VALUES? _pHmacSecretSaltValues;
    public WEBAUTHN_HMAC_SECRET_SALT_VALUES? pHmacSecretSaltValues
    {
        readonly get => this._pHmacSecretSaltValues;
        set
        {
            if (value is null)
            {
                // Unset flag.
                dwFlags &= ~WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_FLAGS.WEBAUTHN_AUTHENTICATOR_HMAC_SECRET_VALUES_FLAG;
            }
            else
            {
                // Set flag.
                dwFlags |= WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS_FLAGS.WEBAUTHN_AUTHENTICATOR_HMAC_SECRET_VALUES_FLAG;
            }
            _pHmacSecretSaltValues = value;
        }
    }

    public bool bBrowserInPrivateMode = false;

    // PCTAPCBOR_HYBRID_STORAGE_LINKED_DATA pLinkedDevice ==> Not Supported.

    public bool bAutoFill = false;

    // DWORD cbJsonExt
    // PBYTE pbJsonExt
    public byte[]? jsonExt = null;

    [CustomMarshaller(typeof(WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS), MarshalMode.ManagedToUnmanagedRef, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public uint dwVersion;
            public uint dwTimeoutMilliseconds;
            public SizePrefixedContiguousStuctArray CredentialList;
            public SizePrefixedContiguousStuctArray Extensions;
            public uint dwAuthenticatorAttachment;
            public uint dwUserVerificationRequirement;
            public uint dwFlags;
            public ushort* pwszU2fAppId;
            public IntPtr pbU2fAppId;
            public IntPtr pCancellationId;
            public IntPtr pAllowCredentialList;
            public uint dwCredLargeBlobOperation;
            public uint cbCredLargeBlob;
            public IntPtr pbCredLargeBlob;
            public IntPtr pHmacSecretSaltValues;
            public int bBrowserInPrivateMode;
            public IntPtr pLinkedDevice;
            public int bAutoFill;
            public uint cbJsonExt;
            public IntPtr pbJsonExt;
        }

        public static unsafe Unmanaged ConvertToUnmanaged(WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS managed)
        {
            var marshalledCredLargeBlob = SizePrefixedByteArray.From(managed.credLargeBlob);
            var marshalledJsonExt = SizePrefixedByteArray.From(managed.jsonExt);

            IntPtr marshalledAllowCredentialList = IntPtr.Zero;
            if (managed.pAllowCredentialList is not null)
            {
                var marshalledStruct = WEBAUTHN_CREDENTIAL_LIST.Marshaller.ConvertToUnmanaged(managed.pAllowCredentialList.Value);
                marshalledAllowCredentialList = MarshalHelper.StructToPtr(marshalledStruct);
            }

            IntPtr marshalledHmacSecretValues = IntPtr.Zero;
            if (managed.pHmacSecretSaltValues.HasValue)
            {
                var marshalledStruct = WEBAUTHN_HMAC_SECRET_SALT_VALUES.Marshaller.ConvertToUnmanaged(managed.pHmacSecretSaltValues.Value);
                marshalledHmacSecretValues = MarshalHelper.StructToPtr(marshalledStruct);
            }

            return new Unmanaged
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
                dwUserVerificationRequirement = (uint)managed.dwUserVerificationRequirement,
                dwFlags = (uint)managed.dwFlags,
                pwszU2fAppId = (ushort*)IntPtr.Zero,    // Not Supported.
                pbU2fAppId = IntPtr.Zero,               // Not Supported.
                pCancellationId = IntPtr.Zero,          // Not Supported.
                pAllowCredentialList = marshalledAllowCredentialList,
                dwCredLargeBlobOperation = managed.dwCredLargeBlobOperation,
                cbCredLargeBlob = marshalledCredLargeBlob.NumElements,
                pbCredLargeBlob = marshalledCredLargeBlob.Pointer,
                pHmacSecretSaltValues = marshalledHmacSecretValues,
                bBrowserInPrivateMode = managed.bBrowserInPrivateMode ? 1 : 0,
                pLinkedDevice = IntPtr.Zero,            // Not Supported.
                bAutoFill = managed.bAutoFill ? 1 : 0,
                cbJsonExt = marshalledJsonExt.NumElements,
                pbJsonExt = marshalledJsonExt.Pointer,
            };
        }

        // We don't use this method. It's only needed because we pass into managed code by ref.
        public static WEBAUTHN_AUTHENTICATOR_GET_ASSERTION_OPTIONS ConvertToManaged(Unmanaged _) => default;

        public static void Free(Unmanaged unmanaged)
        {
            unmanaged.CredentialList.Free<WEBAUTHN_CREDENTIAL.Marshaller.Unmanaged>(
                WEBAUTHN_CREDENTIAL.Marshaller.Free);

            unmanaged.Extensions.Free<WEBAUTHN_EXTENSION.Marshaller.Unmanaged>(
                WEBAUTHN_EXTENSION.Marshaller.Free);

            SizePrefixedByteArray.Free(unmanaged.pbCredLargeBlob);
            SizePrefixedByteArray.Free(unmanaged.pbJsonExt);

            if (unmanaged.pAllowCredentialList != IntPtr.Zero)
            {
                WEBAUTHN_CREDENTIAL_LIST.Marshaller.Free(
                    *(WEBAUTHN_CREDENTIAL_LIST.Marshaller.Unmanaged*)unmanaged.pAllowCredentialList);
                NativeMemory.Free((void*)unmanaged.pAllowCredentialList);
            }

            if (unmanaged.pHmacSecretSaltValues != IntPtr.Zero)
            {
                WEBAUTHN_HMAC_SECRET_SALT_VALUES.Marshaller.Free(
                    *(WEBAUTHN_HMAC_SECRET_SALT_VALUES.Marshaller.Unmanaged*)unmanaged.pHmacSecretSaltValues);
                NativeMemory.Free((void*)unmanaged.pHmacSecretSaltValues);
            }
        }
    }
}
