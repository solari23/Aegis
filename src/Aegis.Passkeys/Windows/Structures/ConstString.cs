using System.Runtime.InteropServices.Marshalling;

namespace Aegis.Passkeys.Windows.Structures;

[NativeMarshalling(typeof(Marshaller))]
internal struct ConstString
{
    public string Value { get; private init; }

    public static implicit operator string(ConstString cs) => cs.Value;

    [CustomMarshaller(typeof(ConstString), MarshalMode.ManagedToUnmanagedOut, typeof(Marshaller))]
    internal static unsafe class Marshaller
    {
        internal struct Unmanaged
        {
#pragma warning disable 0649
            public ushort* pstr;
#pragma warning restore 0649
        }

        public static ConstString ConvertToManaged(Unmanaged unmanaged)
        {
            var ret = new ConstString
            {
                Value = Utf16StringMarshaller.ConvertToManaged(unmanaged.pstr)!,
            };
            return ret;
        }
    }
}
