using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

[NativeMarshalling(typeof(Marshaller))]
internal struct WEBAUTHN_USER_ENTITY_INFORMATION()
{
    public uint dwVersion = 1;

    // DWORD cbId
    // PBYTE pbId
    public byte[]? id = null;

    public string pwszName = string.Empty;

    public string? pwszIcon = null;

    public string pwszDisplayName = string.Empty;

    [CustomMarshaller(typeof(WEBAUTHN_USER_ENTITY_INFORMATION), MarshalMode.ManagedToUnmanagedRef, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
#pragma warning disable 0649
            public uint dwVersion;
            public uint cbId;
            public IntPtr pbId;
            public ushort* pwszName;
            public ushort* pwszIcon;
            public ushort* pwszDisplayName;
#pragma warning restore 0649
        }

        public static WEBAUTHN_USER_ENTITY_INFORMATION ConvertToManaged(Unmanaged unmanaged)
        {
            return new WEBAUTHN_USER_ENTITY_INFORMATION
            {
                dwVersion = unmanaged.dwVersion,
                id = new SizePrefixedArrayStruct(unmanaged.cbId, unmanaged.pbId).ToByteArray(),
                pwszName = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszName)!,
                pwszIcon = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszIcon)!,
                pwszDisplayName = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszDisplayName)!,
            };
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_USER_ENTITY_INFORMATION managed)
        {
            var marshalledId = SizePrefixedArrayStruct.FromBytes(managed.id);

            var ret = new Unmanaged
            {
                dwVersion = 1,
                cbId = marshalledId.NumElements,
                pbId = marshalledId.Pointer,
                pwszName = Utf16StringMarshaller.ConvertToUnmanaged(managed.pwszName),
                pwszIcon = Utf16StringMarshaller.ConvertToUnmanaged(managed.pwszIcon),
                pwszDisplayName = Utf16StringMarshaller.ConvertToUnmanaged(managed.pwszDisplayName),
            };
            return ret;
        }

        public static void Free(Unmanaged unmanaged)
        {
            SizePrefixedArrayStruct.Free(unmanaged.pbId);
            Utf16StringMarshaller.Free(unmanaged.pwszName);
            Utf16StringMarshaller.Free(unmanaged.pwszIcon);
            Utf16StringMarshaller.Free(unmanaged.pwszDisplayName);
        }
    }
}
