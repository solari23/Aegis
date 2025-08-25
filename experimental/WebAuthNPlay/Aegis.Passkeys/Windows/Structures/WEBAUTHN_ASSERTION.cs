using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

[SupportedOSPlatform("windows")]
internal struct WEBAUTHN_ASSERTION()
{
    public uint dwVersion = 0;

    // DWORD cbAuthenticatorData
    // PBYTE pbAuthenticatorData
    public byte[]? authenticatorData = null;

    // DWORD cbSignature
    // PBYTE pbSignature
    public byte[]? signature = null;

    public WEBAUTHN_CREDENTIAL Credential;

    // DWORD cbUserId
    // PBYTE pbUserId
    public byte[]? userId = null;

    public WEBAUTHN_EXTENSION[] Extensions = Array.Empty<WEBAUTHN_EXTENSION>();

    // DWORD cbCredLargeBlob
    // PBYTE pbCredLargeBlob
    public byte[]? credLargeBlob = null;

    public uint dwCredLargeBlobStatus = 0;

    public WEBAUTHN_HMAC_SECRET_SALT? pHmacSecret = null;

    public WEBAUTHN_CTAP_TRANSPORT dwUsedTransport = 0;

    // DWORD cbUnsignedExtensionOutputs
    // PBYTE pbUnsignedExtensionOutputs
    public byte[]? unsignedExtensionOutputs = null;

    internal unsafe class SafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeHandle() : base(ownsHandle: true)
        {
            // Empty.
        }

        protected override bool ReleaseHandle()
        {
            Win32Interop.WebAuthNFreeAssertion(this.handle);
            return true;
        }

        public WEBAUTHN_ASSERTION? ToManaged()
        {
            if (this.IsInvalid)
            {
                return null;
            }

            var reader = new MarshalledMemoryReader(this.handle);

            uint version = reader.ReadUInt32();

            var ret = new WEBAUTHN_ASSERTION();
            ret.dwVersion = version;
            ret.authenticatorData = reader.ReadSizePrefixedArrayStruct().ToByteArray();
            ret.signature = reader.ReadSizePrefixedArrayStruct().ToByteArray();

            ret.Credential = reader.ReadStruct<WEBAUTHN_CREDENTIAL, WEBAUTHN_CREDENTIAL.Marshaller.Unmanaged>(
                WEBAUTHN_CREDENTIAL.Marshaller.ConvertToManaged);

            ret.userId = reader.ReadSizePrefixedArrayStruct().ToByteArray();

            if (version < 2)
            {
                // Fields below were added in version 2+.
                return ret;
            }

            ret.Extensions = reader
                .ReadSizePrefixedArrayStruct()
                .ToManagedFromContiguousArray<WEBAUTHN_EXTENSION, WEBAUTHN_EXTENSION.Marshaller.Unmanaged>(
                    WEBAUTHN_EXTENSION.Marshaller.ConvertToManaged)
                ?? [];

            ret.credLargeBlob = reader.ReadSizePrefixedArrayStruct().ToByteArray();

            ret.dwCredLargeBlobStatus = reader.ReadUInt32();

            if (version < 3)
            {
                // Fields below were added in version 3+.
                return ret;
            }

            ret.pHmacSecret = reader.ReadOptionalStruct<WEBAUTHN_HMAC_SECRET_SALT, WEBAUTHN_HMAC_SECRET_SALT.Marshaller.Unmanaged>(
                WEBAUTHN_HMAC_SECRET_SALT.Marshaller.ConvertToManaged);

            if (version < 4)
            {
                // Fields below were added in version 4+.
                return ret;
            }

            ret.dwUsedTransport = (WEBAUTHN_CTAP_TRANSPORT)reader.ReadUInt32();

            if (version < 5)
            {
                // Fields below were added in version 5+.
                return ret;
            }

            ret.unsignedExtensionOutputs = reader.ReadSizePrefixedArrayStruct().ToByteArray();

            return ret;
        }
    }
}
