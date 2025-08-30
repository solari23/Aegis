using System.Runtime.InteropServices.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

[NativeMarshalling(typeof(Marshaller))]
internal struct WEBAUTHN_COSE_CREDENTIAL_PARAMETER()
{
    private uint dwVersion = 1;

    private string pwszCredentialType = "public-key";

    public WEBAUTHN_COSE_ALGORITHM lAlg;

    [CustomMarshaller(typeof(WEBAUTHN_COSE_CREDENTIAL_PARAMETER), MarshalMode.ManagedToUnmanagedRef, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public uint dwVersion;
            public ushort* pwszCredentialType;
            public int lAlg;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_COSE_CREDENTIAL_PARAMETER managed)
        {
            var ret = new Unmanaged
            {
                dwVersion = managed.dwVersion,
                pwszCredentialType = Utf16StringMarshaller.ConvertToUnmanaged(managed.pwszCredentialType),
                lAlg = (int)managed.lAlg,
            };
            return ret;
        }

        public static WEBAUTHN_COSE_CREDENTIAL_PARAMETER ConvertToManaged(Unmanaged unmanaged)
        {
            var ret = new WEBAUTHN_COSE_CREDENTIAL_PARAMETER
            {
                dwVersion = unmanaged.dwVersion,
                pwszCredentialType = Utf16StringMarshaller.ConvertToManaged(unmanaged.pwszCredentialType)!,
                lAlg = (WEBAUTHN_COSE_ALGORITHM)unmanaged.lAlg,
            };
            return ret;
        }

        public static void Free(Unmanaged unmanaged)
        {
            Utf16StringMarshaller.Free(unmanaged.pwszCredentialType);
        }
    }
}
