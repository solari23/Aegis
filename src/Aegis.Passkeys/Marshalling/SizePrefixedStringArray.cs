using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Aegis.Passkeys.Marshalling;

/// <summary>
/// Suitable for (un)marshalling arrays like:
///   DWORD cFoo
///   LPCWSTR *ppwszFoo
/// </summary>
internal readonly struct SizePrefixedStringArray(uint numElements, nint pointer)
{
    public static SizePrefixedStringArray Empty => new(0, nint.Zero);

    public static unsafe SizePrefixedStringArray From(string[]? input)
    {
        if (input is null || input.Length == 0)
        {
            return Empty;
        }

        uint numElements = (uint)input.Length;
        ushort** pointerArray = (ushort**)NativeMemory.Alloc((nuint)(sizeof(nint) * numElements));

        for (int i = 0; i < numElements; i++)
        {
            pointerArray[i] = Utf16StringMarshaller.ConvertToUnmanaged(input[i]);
        }

        return new SizePrefixedStringArray(numElements, (nint)pointerArray);
    }

    public readonly uint NumElements = numElements;

    public readonly nint Pointer = pointer;

    public unsafe void Free()
    {
        if (this.Pointer == nint.Zero)
        {
            return;
        }

        for (int i = 0; i < this.NumElements; i++)
        {
            var currentItem = ((ushort**)this.Pointer)[i];
            Utf16StringMarshaller.Free(currentItem);
        }

        // Free the pointer array.
        NativeMemory.Free((void*)this.Pointer);
    }

    public unsafe string[]? ToManagedArray()
    {
        if (this.Pointer == nint.Zero)
        {
            return null;
        }

        var data = new string[this.NumElements];

        for (int i = 0; i < this.NumElements; i++)
        {
            var currentItem = ((ushort**)this.Pointer)[i];
            data[i] = Utf16StringMarshaller.ConvertToManaged(currentItem)!;
        }

        return data;
    }
}
