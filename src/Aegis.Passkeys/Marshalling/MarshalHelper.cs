using System.Runtime.InteropServices;

namespace Aegis.Passkeys.Marshalling;

internal static class MarshalHelper
{
    /// <summary>
    /// Copies the given blittable structure to unmanaged memory and returns the pointer to that memory.
    /// It is the caller's responsibility to free this unmanaged memory.
    /// </summary>
    public static unsafe nint StructToPtr<TStruct>(TStruct str) where TStruct : unmanaged
    {
        var structOnHeap = (nint)NativeMemory.Alloc((nuint)sizeof(TStruct));
        *(TStruct*)structOnHeap = str;
        return structOnHeap;
    }

    /// <summary>
    /// Casts the given pointer to a nullable structure of type TStruct.
    /// </summary>
    public static unsafe TStruct? PtrToStruct<TStruct>(nint ptr) where TStruct : unmanaged
    {
        if (ptr == nint.Zero)
        {
            return null;
        }

        return *(TStruct*)ptr;
    }
}
