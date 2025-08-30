using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

internal struct WEBAUTHN_CREDENTIAL_LIST()
{
    public WEBAUTHN_CREDENTIAL_EX[] Credentials = [];

    [CustomMarshaller(typeof(WEBAUTHN_CREDENTIAL_LIST), MarshalMode.ManagedToUnmanagedIn, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public uint cCredentials;
            public IntPtr ppCredentials;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_CREDENTIAL_LIST managed)
        {
            var marshalledCredentialsArray = SizePrefixedStructPointerArray.From(
                managed.Credentials,
                WEBAUTHN_CREDENTIAL_EX.Marshaller.ConvertToUnmanaged);

            var ret = new Unmanaged
            {
                cCredentials = marshalledCredentialsArray.NumElements,
                ppCredentials = marshalledCredentialsArray.Pointer,
            };
            return ret;
        }

        public static void Free(Unmanaged unmanaged)
        {
            new SizePrefixedStructPointerArray(unmanaged.cCredentials, unmanaged.ppCredentials)
                .Free<WEBAUTHN_CREDENTIAL_EX.Marshaller.Unmanaged>(
                    WEBAUTHN_CREDENTIAL_EX.Marshaller.Free);
        }
    }
}
