using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

internal struct WEBAUTHN_CREDENTIAL()
{
    private uint dwVersion = 1;

    // DWORD cbId
    // PBYTE pbId
    required public byte[] id;

    required public string pwszCredentialType;

    [CustomMarshaller(typeof(WEBAUTHN_CREDENTIAL), MarshalMode.ManagedToUnmanagedIn, typeof(Marshaller))]
    [CustomMarshaller(typeof(WEBAUTHN_CREDENTIAL), MarshalMode.ManagedToUnmanagedOut, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public uint dwVersion;
            public uint cbId;
            public IntPtr pbId;
            public ushort* pwszCredentialType;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_CREDENTIAL managed)
        {
            var marshalledId = SizePrefixedArrayStruct.FromBytes(managed.id);

            return new Unmanaged
            {
                dwVersion = managed.dwVersion,
                cbId = marshalledId.NumElements,
                pbId = marshalledId.Pointer,
                pwszCredentialType = Utf16StringMarshaller.ConvertToUnmanaged(managed.pwszCredentialType),
            };
        }

        public static WEBAUTHN_CREDENTIAL ConvertToManaged(IntPtr unmanaged)
        {
            var reader = new MarshalledMemoryReader(unmanaged);

            uint version = reader.ReadUInt32();

            // This is a bit nasty in the Win32 API.
            //
            // The WEBAUTHN_CREDENTIAL structure is directly embedded inline as a field in the WEBAUTHN_ASSERTION
            // (sandwiched between PBYTE pbSignature and DWORD cbUserId). So, if they ever bump the version and
            // add more data, any attempt by this code to unmarshal will end up corrupt since we'll attempt to read
            // the new extra data into cbUserId..
            //
            // No choice but to throw. We're just not compatible with any version bumps.
            if (version > 1)
            {
                throw new InvalidOperationException(
                    $"Unrecogized {nameof(WEBAUTHN_CREDENTIAL)} version {version}. Max recognized version: 1.");
            }

            var ret = new WEBAUTHN_CREDENTIAL
            {
                dwVersion = version,
                id = reader.ReadSizePrefixedArrayStruct().ToByteArray()!,
                pwszCredentialType = reader.ReadPWSTR()!,
            };
            return ret;
        }

        public static void Free(Unmanaged unmanaged)
        {
            SizePrefixedArrayStruct.Free(unmanaged.pbId);
            Utf16StringMarshaller.Free(unmanaged.pwszCredentialType);
        }
    }
}
