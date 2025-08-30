using Microsoft.Win32.SafeHandles;
using System.Runtime.Versioning;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

[SupportedOSPlatform("windows")]
internal struct WEBAUTHN_CREDENTIAL_DETAILS_LIST
{
    // DWORD cCredentialDetails
    // PWEBAUTHN_CREDENTIAL_DETAILS *ppCredentialDetails
    public WEBAUTHN_CREDENTIAL_DETAILS[]? credentials;

    internal unsafe class SafeHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeHandle() : base(ownsHandle: true)
        {
            // Empty.
        }

        protected override bool ReleaseHandle()
        {
            Win32Interop.WebAuthNFreePlatformCredentialList(this.handle);
            return true;
        }

        public WEBAUTHN_CREDENTIAL_DETAILS_LIST? ToManaged()
        {
            if (this.IsInvalid)
            {
                return null;
            }

            var reader = new MarshalledMemoryReader(this.handle);

            uint cCredentialDetails = reader.ReadUInt32();
            IntPtr ppCredentialDetails = reader.ReadIntPtr();

            var ret = new WEBAUTHN_CREDENTIAL_DETAILS_LIST
            {
                credentials = new SizePrefixedStructPointerArray(cCredentialDetails, ppCredentialDetails)
                    .ToManagedArray<WEBAUTHN_CREDENTIAL_DETAILS, WEBAUTHN_CREDENTIAL_DETAILS.Marshaller.Unmanaged>(
                        WEBAUTHN_CREDENTIAL_DETAILS.Marshaller.ConvertToManaged),
            };
            return ret;
        }
    }
}
