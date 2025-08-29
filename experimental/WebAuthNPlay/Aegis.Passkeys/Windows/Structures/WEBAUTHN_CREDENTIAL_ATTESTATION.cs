using Aegis.Passkeys.Marshalling;

using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;

namespace Aegis.Passkeys.Windows.Structures;

[SupportedOSPlatform("windows")]
internal struct WEBAUTHN_CREDENTIAL_ATTESTATION()
{
    public uint dwVersion = 0;

    public string pwszFormatType = string.Empty;

    // DWORD cbAuthenticatorData
    // PBYTE pbAuthenticatorData
    public byte[]? authenticatorData = null;

    // DWORD cbAttestation
    // PBYTE pbAttestation
    public byte[]? attestation = null;

    public uint dwAttestationDecodeType = 0;

    public IntPtr attestationDecode = IntPtr.Zero;

    // DWORD cbAttestationObject
    // PBYTE pbAttestationObject
    public byte[]? attestationObject = null;

    // DWORD cbCredentialId
    // PBYTE pbCredentialId
    public byte[]? credentialId = null;

    public WEBAUTHN_EXTENSION[] Extensions = [];

    public WEBAUTHN_CTAP_TRANSPORT dwUsedTransport = 0;

    public bool bEpAtt = false;

    public bool bLargeBlobSupported = false;

    public bool bResidentKey = false;

    public bool bPrfEnabled = false;

    // DWORD cbUnsignedExtensionOutputs
    // PBYTE pbUnsignedExtensionOutputs
    public byte[]? unsignedExtensionOutputs = null;

    public WEBAUTHN_HMAC_SECRET_SALT? pHmacSecret = null;

    public bool bThirdPartyPayment = false;

    internal unsafe class SafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeHandle() : base(ownsHandle: true)
        {
            // Empty.
        }

        protected override bool ReleaseHandle()
        {
            Win32Interop.WebAuthNFreeCredentialAttestation(this.handle);
            return true;
        }

        public WEBAUTHN_CREDENTIAL_ATTESTATION? ToManaged()
        {
            if (this.IsInvalid)
            {
                return null;
            }

            var reader = new MarshalledMemoryReader(this.handle);

            uint version = reader.ReadUInt32();

            var ret = new WEBAUTHN_CREDENTIAL_ATTESTATION();
            ret.dwVersion = version;
            ret.pwszFormatType = reader.ReadPWSTR() ?? string.Empty;
            ret.authenticatorData = reader.ReadSizePrefixedArrayStruct().ToByteArray();
            ret.attestation = reader.ReadSizePrefixedArrayStruct().ToByteArray();
            ret.dwAttestationDecodeType = reader.ReadUInt32();
            ret.attestationDecode = reader.ReadIntPtr();
            ret.attestationObject = reader.ReadSizePrefixedArrayStruct().ToByteArray();
            ret.credentialId = reader.ReadSizePrefixedArrayStruct().ToByteArray();

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

            if (version < 3)
            {
                // Fields below were added in version 3+.
                return ret;
            }

            ret.dwUsedTransport = (WEBAUTHN_CTAP_TRANSPORT)reader.ReadUInt32();

            if (version < 4)
            {
                // Fields below were added in version 4+.
                return ret;
            }

            ret.bEpAtt = reader.ReadBOOL();
            ret.bLargeBlobSupported = reader.ReadBOOL();
            ret.bResidentKey = reader.ReadBOOL();

            if (version < 5)
            {
                // Fields below were added in version 5+.
                return ret;
            }

            ret.bPrfEnabled = reader.ReadBOOL();

            if (version < 6)
            {
                // Fields below were added in version 6+.
                return ret;
            }

            ret.unsignedExtensionOutputs = reader.ReadSizePrefixedArrayStruct().ToByteArray();

            if (version < 7)
            {
                // Fields below were added in version 7+.
                return ret;
            }

            ret.pHmacSecret = reader.ReadOptionalStruct<WEBAUTHN_HMAC_SECRET_SALT, WEBAUTHN_HMAC_SECRET_SALT.Marshaller.Unmanaged>(
                WEBAUTHN_HMAC_SECRET_SALT.Marshaller.ConvertToManaged);

            ret.bThirdPartyPayment = reader.ReadBOOL();

            return ret;
        }
    }
}
