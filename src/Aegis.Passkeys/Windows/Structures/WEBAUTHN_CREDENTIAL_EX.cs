using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

internal struct WEBAUTHN_CREDENTIAL_EX()
{
    private uint dwVersion = 1;

    // DWORD cbId
    // PBYTE pbId
    public byte[]? id = null;

    private string pwszCredentialType = "public-key";

    // DWORD dwTransports ==> Not Supported.

    [CustomMarshaller(typeof(WEBAUTHN_CREDENTIAL_EX), MarshalMode.ManagedToUnmanagedIn, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public uint dwVersion;
            public uint cbId;
            public IntPtr pbId;
            public ushort* pwszCredentialType;
            public uint dwTransports;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_CREDENTIAL_EX managed)
        {
            var marshalledId = SizePrefixedByteArray.From(managed.id);

            var ret = new Unmanaged()
            {
                dwVersion = managed.dwVersion,
                cbId = marshalledId.NumElements,
                pbId = marshalledId.Pointer,
                pwszCredentialType = Utf16StringMarshaller.ConvertToUnmanaged(managed.pwszCredentialType),
                dwTransports = 0, // Not supported.
            };
            return ret;
        }

        public static void Free(Unmanaged unmanaged)
        {
            SizePrefixedByteArray.Free(unmanaged.pbId);
            Utf16StringMarshaller.Free(unmanaged.pwszCredentialType);
        }
    }
}
