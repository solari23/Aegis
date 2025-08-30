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

        uint numElements = (uint)input.Length;
        nint* pointerArray = (nint*)NativeMemory.Alloc((nuint)(sizeof(nint) * numElements));

        // To reduce the number of allocations, we'll marshal the TManaged structs into
        // contiguous memory.
        TUnmanaged* unmanagedArray = (TUnmanaged*)NativeMemory.Alloc((nuint)(sizeof(TUnmanaged) * numElements));

        for (int i = 0; i < numElements; i++)
        {
            unmanagedArray[i] = elementMarshaller(input[i]);
            pointerArray[i] = (nint)(&unmanagedArray[i]);
        }

        return new SizePrefixedStructPointerArray(numElements, (nint)pointerArray);
    }

    public readonly uint NumElements = numElements;

    public readonly nint Pointer = pointer;

    public unsafe void Free<TUnmanaged>(Action<TUnmanaged> elementFreeFunction)
        where TUnmanaged : unmanaged
    {
        if (this.Pointer == nint.Zero)
        {
            return;
        }

        for (int i = 0; i < this.NumElements; i++)
        {
            nint currentItem = this.Pointer + i * sizeof(nint);
            elementFreeFunction(**(TUnmanaged**)currentItem);
        }

        // Free the contiguous struct array.
        NativeMemory.Free((void*)*(nint*)this.Pointer);

        // Free the pointer array.
        NativeMemory.Free((void*)this.Pointer);
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
