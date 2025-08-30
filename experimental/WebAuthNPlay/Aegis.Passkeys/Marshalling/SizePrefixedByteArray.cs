using System.Runtime.InteropServices;

namespace Aegis.Passkeys.Marshalling;

/// <summary>
/// Suitable for (un)marshalling structures like:
///   DWORD cbFoo
///   PBYTE pbFoo
/// </summary>
internal readonly struct SizePrefixedByteArray(uint numBytes, nint pointer)
{
    public static SizePrefixedByteArray Empty => new(0, nint.Zero);

    public static unsafe void Free(nint ptr) => NativeMemory.Free((void*)ptr);

    public static unsafe SizePrefixedByteArray From(byte[]? data)
    {
        if (data is null)
        {
            return Empty;
        }

        nint ptr = (nint)NativeMemory.Alloc((nuint)data.Length);
        Marshal.Copy(data, 0, ptr, data.Length);

        return new SizePrefixedByteArray((uint)data.Length, ptr);
    }

    public readonly uint NumElements = numBytes;

    public readonly nint Pointer = pointer;

    public unsafe void Free() => Free(this.Pointer);

    public byte[]? ToManagedArray()
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
