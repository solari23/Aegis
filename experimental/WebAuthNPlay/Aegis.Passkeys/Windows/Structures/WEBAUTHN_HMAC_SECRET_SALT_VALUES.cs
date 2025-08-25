using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

internal struct WEBAUTHN_HMAC_SECRET_SALT_VALUES
{
    public WEBAUTHN_HMAC_SECRET_SALT pGlobalHmacSalt;

    // DWORD cCredWithHmacSecretSaltList ==> Not Supported.
    // PWEBAUTHN_CRED_WITH_HMAC_SECRET_SALT pCredWithHmacSecretSaltList  ==> Not Supported.

    [CustomMarshaller(typeof(WEBAUTHN_HMAC_SECRET_SALT_VALUES), MarshalMode.ManagedToUnmanagedIn, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public IntPtr pGlobalHmacSalt;
            public uint cCredWithHmacSecretSaltList;
            public IntPtr pCredWithHmacSecretSaltList;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_HMAC_SECRET_SALT_VALUES managed)
        {
            var marshalledGlobalHmacSalt = WEBAUTHN_HMAC_SECRET_SALT.Marshaller.ConvertToUnmanaged(managed.pGlobalHmacSalt);
            var ret = new Unmanaged
            {
                pGlobalHmacSalt = MarshalHelper.StructToPtr(marshalledGlobalHmacSalt),
                cCredWithHmacSecretSaltList = 0, // Not Supported.
                pCredWithHmacSecretSaltList = IntPtr.Zero, // Not Supported.
            };
            return ret;
        }

        public static void Free(Unmanaged unmanaged)
        {
            if (unmanaged.pGlobalHmacSalt != IntPtr.Zero)
            {
                WEBAUTHN_HMAC_SECRET_SALT.Marshaller.Free(
                    *(WEBAUTHN_HMAC_SECRET_SALT.Marshaller.Unmanaged*)unmanaged.pGlobalHmacSalt);
                NativeMemory.Free((void*)unmanaged.pGlobalHmacSalt);
            }
        }
    }
}
