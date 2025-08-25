using System.Runtime.InteropServices.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

[NativeMarshalling(typeof(Marshaller))]
internal struct WEBAUTHN_GET_CREDENTIALS_OPTIONS()
{
    private uint dwVersion = 1;

    required public string pwszRpId;

    required public bool bBrowserInPrivateMode;

    [CustomMarshaller(typeof(WEBAUTHN_GET_CREDENTIALS_OPTIONS), MarshalMode.ManagedToUnmanagedRef, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public uint dwVersion;
            public ushort* pwszRpId;
            public bool bBrowserInPrivateMode;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_GET_CREDENTIALS_OPTIONS managed)
        {
            return new Unmanaged
            {
                dwVersion = managed.dwVersion,
                pwszRpId = Utf16StringMarshaller.ConvertToUnmanaged(managed.pwszRpId),
                bBrowserInPrivateMode = managed.bBrowserInPrivateMode,
            };
        }

        public static WEBAUTHN_GET_CREDENTIALS_OPTIONS ConvertToManaged(Unmanaged unmanaged)
        {
            return new WEBAUTHN_GET_CREDENTIALS_OPTIONS
            {
                dwVersion = unmanaged.dwVersion,
                pwszRpId = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszRpId)!,
                bBrowserInPrivateMode = unmanaged.bBrowserInPrivateMode,
            };
        }

        public static void Free(Unmanaged unmanaged)
        {
            Utf16StringMarshaller.Free(unmanaged.pwszRpId);
        }
    }
}
