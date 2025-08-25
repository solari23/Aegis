using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

[NativeMarshalling(typeof(Marshaller))]
internal struct WEBAUTHN_CLIENT_DATA()
{
    private uint dwVersion = 1;

    // DWORD cbClientDataJSON;
    // PBYTE pbClientDataJSON;
    required public byte[] clientDataJSON;

    public string pwszHashAlgId = WEBAUTHN_HASH_ALGORITHM.SHA_256;

    [CustomMarshaller(typeof(WEBAUTHN_CLIENT_DATA), MarshalMode.ManagedToUnmanagedRef, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public uint dwVersion;
            public uint cbClientDataJSON;
            public IntPtr pbClientDataJSON;
            public ushort* pwszHashAlgId;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_CLIENT_DATA managed)
        {
            var marshalledClientDataJson = SizePrefixedArrayStruct.FromBytes(managed.clientDataJSON);

            return new Unmanaged
            {
                dwVersion = managed.dwVersion,
                cbClientDataJSON = marshalledClientDataJson.NumElements,
                pbClientDataJSON = marshalledClientDataJson.Pointer,
                pwszHashAlgId = Utf16StringMarshaller.ConvertToUnmanaged(managed.pwszHashAlgId),
            };
        }

        public static WEBAUTHN_CLIENT_DATA ConvertToManaged(Unmanaged unmanaged)
        {
            return new WEBAUTHN_CLIENT_DATA
            {
                dwVersion = unmanaged.dwVersion,
                clientDataJSON = new SizePrefixedArrayStruct(unmanaged.cbClientDataJSON, unmanaged.pbClientDataJSON).ToByteArray()!,
                pwszHashAlgId = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszHashAlgId)!,
            };
        }

        public static void Free(Unmanaged unmanaged)
        {
            Marshal.FreeHGlobal(unmanaged.pbClientDataJSON);
            Utf16StringMarshaller.Free(unmanaged.pwszHashAlgId);
        }
    }
}
