using System.Runtime.InteropServices;

namespace Aegis.Passkeys.Marshalling;

internal readonly struct SizePrefixedArrayStruct(uint numElements, nint pointer)
{
    public static SizePrefixedArrayStruct Empty => new(0, nint.Zero);

    public static unsafe void Free(nint ptr) => NativeMemory.Free((void*)ptr);

    public static unsafe SizePrefixedArrayStruct FromArrayToContiguous<TManaged, TUnmanaged>(
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

        return new SizePrefixedArrayStruct(numElements, (nint)unmanagedArray);
    }

    public static unsafe SizePrefixedArrayStruct FromBytes(byte[]? data)
    {
        if (data is null)
        {
            return Empty;
        }

        nint ptr = (nint)NativeMemory.Alloc((nuint)data.Length);
        Marshal.Copy(data, 0, ptr, data.Length);

        return new SizePrefixedArrayStruct((uint)data.Length, ptr);
    }

    public readonly uint NumElements = numElements;

    public readonly nint Pointer = pointer;

    public unsafe void Free() => Free(this.Pointer);

    public unsafe TManaged[]? ToManagedFromArrayOfPointers<TManaged, TUnmanaged>(
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

    public unsafe TManaged[]? ToManagedFromContiguousArray<TManaged, TUnmanaged>(
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
            nint currentItemPointer = this.Pointer + i * Marshal.SizeOf<TManaged>();
            data[i] = elementMarshaller(*(TUnmanaged*)currentItemPointer);
        }

        return data;
    }

    public byte[]? ToByteArray()
    {
        if (this.Pointer == nint.Zero)
        {
            return null;
        }

        var data = new byte[this.NumElements];
        Marshal.Copy(this.Pointer, data, 0, (int)this.NumElements);

        return data;
    }
}
