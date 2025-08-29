using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

internal struct WEBAUTHN_EXTENSION
{
    public string pwszExtensionIdentifier;

    // DWORD cbExtension
    // PVOID pvExtension;
    public byte[]? extension;

    [CustomMarshaller(typeof(WEBAUTHN_EXTENSION), MarshalMode.ManagedToUnmanagedIn, typeof(Marshaller))]
    [CustomMarshaller(typeof(WEBAUTHN_EXTENSION), MarshalMode.ManagedToUnmanagedOut, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public ushort* pwszExtensionIdentifier;
            public uint cbExtension;
            public IntPtr pvExtension;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_EXTENSION managed)
        {
            var marshalledExtension = SizePrefixedArrayStruct.FromBytes(managed.extension);

            return new Unmanaged
            {
                pwszExtensionIdentifier = Utf16StringMarshaller.ConvertToUnmanaged(managed.pwszExtensionIdentifier),
                cbExtension = marshalledExtension.NumElements,
                pvExtension = marshalledExtension.Pointer,
            };
        }

        public static WEBAUTHN_EXTENSION ConvertToManaged(Unmanaged unmanaged)
        {
            var ret = new WEBAUTHN_EXTENSION
            {
                pwszExtensionIdentifier = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszExtensionIdentifier)!,
                extension = new SizePrefixedArrayStruct(unmanaged.cbExtension, unmanaged.pvExtension).ToByteArray(),
            };
            return ret;
        }

        public static void Free(Unmanaged unmanaged)
        {
            Utf16StringMarshaller.Free(unmanaged.pwszExtensionIdentifier);
            SizePrefixedArrayStruct.Free(unmanaged.pvExtension);
        }
    }
}
