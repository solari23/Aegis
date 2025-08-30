using System.Runtime.InteropServices;

namespace Aegis.Passkeys.Marshalling;

/// <summary>
/// Suitable for (un)marshalling structures like:
///   DWORD cFoo
///   PTYPE *ppFoo
/// </summary>
internal readonly struct SizePrefixedStructPointerArray(uint numElements, nint pointer)
{
    public static SizePrefixedStructPointerArray Empty => new(0, nint.Zero);

    public static unsafe void Free(nint ptr) => NativeMemory.Free((void*)ptr);

    public static unsafe SizePrefixedStructPointerArray From<TManaged, TUnmanaged>(
        TManaged[] input,
        Func<TManaged, TUnmanaged> elementMarshaller)
        where TManaged : struct
        where TUnmanaged : unmanaged
    {
        if (input is null || input.Length == 0)
        {
            return Empty;
        }

        // TODO: Implement SizePrefixedStructPointerArray.From
        throw new NotImplementedException();
    }

    public readonly uint NumElements = numElements;

    public readonly nint Pointer = pointer;

    public unsafe void Free()
    {
        // TODO: Implement SizePrefixedStructPointerArray.Free
        throw new NotImplementedException();
    }

    public unsafe TManaged[]? ToManagedArray<TManaged, TUnmanaged>(
        Func<TUnmanaged, TManaged> elementMarshaller)
        where TUnmanaged : unmanaged
    {
        if (this.Pointer == nint.Zero)
        {
            return null;
        }

        var data = new TManaged[this.NumElements];

        for (int i = 0; i < this.NumElements; i++)
        {
            nint currentItem = Marshal.ReadIntPtr(this.Pointer + i * sizeof(nint));
            data[i] = elementMarshaller(*(TUnmanaged*)currentItem);
        }

        return data;
    }
}
