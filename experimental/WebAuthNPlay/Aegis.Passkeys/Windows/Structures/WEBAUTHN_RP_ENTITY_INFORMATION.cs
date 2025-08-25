using System.Runtime.InteropServices.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

internal struct WEBAUTHN_RP_ENTITY_INFORMATION
{
    public uint dwVersion;

    public string pwszId;

    public string pwszName;

    public string pwszIcon;

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
            return new WEBAUTHN_RP_ENTITY_INFORMATION
            {
                dwVersion = unmanaged.dwVersion,
                pwszId = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszId)!,
                pwszName = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszName)!,
                pwszIcon = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszIcon)!,
            };
        }
    }
}
