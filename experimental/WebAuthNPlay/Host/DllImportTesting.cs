using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace WebAuthNPlay;

internal static partial class DllImportTesting
{
    [LibraryImport("kernel32.dll")]
    public static partial void DoThing(out AwesomeStruct awesome);
}

[NativeMarshalling(typeof(Marshaller))]
public ref struct AwesomeStruct
{
    public string Name;

    [CustomMarshaller(typeof(AwesomeStruct), MarshalMode.Default, typeof(Marshaller))]
    //[CustomMarshaller(typeof(AwesomeStruct), MarshalMode.ManagedToUnmanagedIn, typeof(Marshaller))]
    //[CustomMarshaller(typeof(AwesomeStruct), MarshalMode.ManagedToUnmanagedOut, typeof(Marshaller))]
    internal static class Marshaller
    {
        public struct Unmanaged { }

        public static AwesomeStruct ConvertToManaged(nint unmanaged)
        {
            throw new NotImplementedException();
        }

        public static nint ConvertToUnmanaged(AwesomeStruct managed)
        {
            throw new NotImplementedException();
        }
    }
}
