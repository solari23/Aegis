using System.Runtime.InteropServices.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

[NativeMarshalling(typeof(Marshaller))]
internal struct WEBAUTHN_RP_ENTITY_INFORMATION()
{
    public uint dwVersion = 1;

    public string pwszId = string.Empty;

    public string? pwszName = string.Empty;

    public string? pwszIcon;

    [CustomMarshaller(typeof(WEBAUTHN_RP_ENTITY_INFORMATION), MarshalMode.ManagedToUnmanagedRef, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
#pragma warning disable 0649
            public uint dwVersion;
            public ushort* pwszId;
            public ushort* pwszName;
            public ushort* pwszIcon;
#pragma warning restore 0649
        }

        public static WEBAUTHN_RP_ENTITY_INFORMATION ConvertToManaged(Unmanaged unmanaged)
        {
            var ret = new WEBAUTHN_RP_ENTITY_INFORMATION
            {
                dwVersion = unmanaged.dwVersion,
                pwszId = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszId)!,
                pwszName = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszName)!,
                pwszIcon = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszIcon)!,
            };
            return ret;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_RP_ENTITY_INFORMATION managed)
        {
            var ret = new Unmanaged
            {
                dwVersion = 1,
                pwszId = Utf16StringMarshaller.ConvertToUnmanaged(managed.pwszId),
                pwszName = Utf16StringMarshaller.ConvertToUnmanaged(managed.pwszName),
                pwszIcon = Utf16StringMarshaller.ConvertToUnmanaged(managed.pwszIcon),
            };
            return ret;
        }

        public static void Free(Unmanaged unmanaged)
        {
            Utf16StringMarshaller.Free(unmanaged.pwszId);
            Utf16StringMarshaller.Free(unmanaged.pwszName);
            Utf16StringMarshaller.Free(unmanaged.pwszIcon);
        }
    }
}
