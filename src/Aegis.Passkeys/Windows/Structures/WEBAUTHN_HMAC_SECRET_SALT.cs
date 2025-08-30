using System.Runtime.InteropServices.Marshalling;

using Aegis.Passkeys.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

internal struct WEBAUTHN_HMAC_SECRET_SALT
{
    // DWORD cbFirst
    // PBYTE pbFirst
    public byte[] first;

    // DWORD cbSecond
    // PBYTE pbSecond
    public byte[]? second;

    [CustomMarshaller(typeof(WEBAUTHN_HMAC_SECRET_SALT), MarshalMode.ManagedToUnmanagedIn, typeof(Marshaller))]
    [CustomMarshaller(typeof(WEBAUTHN_HMAC_SECRET_SALT), MarshalMode.ManagedToUnmanagedOut, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
            public uint cbFirst;
            public IntPtr pbFirst;
            public uint cbSecond;
            public IntPtr pbSecond;
        }

        public static Unmanaged ConvertToUnmanaged(WEBAUTHN_HMAC_SECRET_SALT managed)
        {
            var marshalledFirst = SizePrefixedByteArray.From(managed.first);
            var marshalledSecond = SizePrefixedByteArray.From(managed.second);

            var ret = new Unmanaged
            {
                cbFirst = marshalledFirst.NumElements,
                pbFirst = marshalledFirst.Pointer,
                cbSecond = marshalledSecond.NumElements,
                pbSecond = marshalledSecond.Pointer,
            };
            return ret;
        }

        public static WEBAUTHN_HMAC_SECRET_SALT ConvertToManaged(Unmanaged unmanaged)
        {
            var managed = new WEBAUTHN_HMAC_SECRET_SALT();

            if (unmanaged.cbFirst > 0)
            {
                var first = new SizePrefixedByteArray(unmanaged.cbFirst, unmanaged.pbFirst);
                managed.first = first.ToManagedArray()!;
            }

            if (unmanaged.cbSecond > 0)
            {
                var second = new SizePrefixedByteArray(unmanaged.cbSecond, unmanaged.pbSecond);
                managed.second = second.ToManagedArray()!;
            }

            return managed;
        }

        public static void Free(Unmanaged unmanaged)
        {
            SizePrefixedByteArray.Free(unmanaged.pbFirst);
            SizePrefixedByteArray.Free(unmanaged.pbSecond);
        }
    }
}