using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

internal struct WEBAUTHN_USER_ENTITY_INFORMATION
{
    public uint dwVersion;

    // DWORD cbId
    // PBYTE pbId
    public byte[]? id;

    public string pwszName;

    public string pwszIcon;

    public string pwszDisplayName;

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
    }
}
