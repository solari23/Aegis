using System.Runtime.InteropServices;

namespace Aegis.Passkeys.Marshalling;

/// <summary>
/// Suitable for (un)marshalling structures like:
///   DWORD cFoo
///   PTYPE pFoo
/// </summary>
internal readonly struct SizePrefixedContiguousStuctArray(uint numElements, nint pointer)
{
    public static SizePrefixedContiguousStuctArray Empty => new(0, nint.Zero);

    public static unsafe SizePrefixedContiguousStuctArray From<TManaged, TUnmanaged>(
        TManaged[] input,
        Func<TManaged, TUnmanaged> elementMarshaller)
        where TManaged : struct
        where TUnmanaged : unmanaged
    {
        if (input is null || input.Length == 0)
        {
            return Empty;
        }

        int itemSize = sizeof(TUnmanaged);
        uint numElements = (uint)input.Length;
        TUnmanaged* unmanagedArray = (TUnmanaged*)NativeMemory.Alloc((nuint)(itemSize * numElements));

        for (int i = 0; i < numElements; i++)
        {
            unmanagedArray[i] = elementMarshaller(input[i]);
        }

        return new SizePrefixedContiguousStuctArray(numElements, (nint)unmanagedArray);
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

        int itemSize = sizeof(TUnmanaged);

        for (int i = 0; i < this.NumElements; i++)
        {
            nint currentItemPointer = this.Pointer + i * itemSize;
            elementFreeFunction(*(TUnmanaged*)currentItemPointer);
        }
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

        int itemSize = sizeof(TUnmanaged);

        for (int i = 0; i < this.NumElements; i++)
        {
            nint currentItemPointer = this.Pointer + i * itemSize;
            data[i] = elementMarshaller(*(TUnmanaged*)currentItemPointer);
        }

        return data;
    }
}
