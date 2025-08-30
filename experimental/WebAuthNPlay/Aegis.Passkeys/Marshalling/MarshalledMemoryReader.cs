using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Aegis.Passkeys.Marshalling;

internal class MarshalledMemoryReader
{
    public MarshalledMemoryReader(nint handle)
    {
        this.Handle = handle;
        this.offset = 0;
    }

    public nint Handle { get; }

    private int offset;

    public unsafe nint ReadIntPtr()
    {
        offset += offset % sizeof(nint);
        var ret = Marshal.ReadIntPtr(this.Handle + this.offset);
        this.offset += sizeof(nint);
        return ret;
    }

    public uint ReadUInt32() => (uint)this.ReadInt32();

    public int ReadInt32()
    {
        offset += offset % sizeof(int);
        var ret = Marshal.ReadInt32(this.Handle + this.offset);
        this.offset += sizeof(int);
        return ret;
    }

    //public SizePrefixedArrayStruct ReadSizePrefixedArrayStruct()
    //{
    //    uint numElements = this.ReadUInt32();
    //    nint ptr = this.ReadIntPtr();
    //    return new SizePrefixedArrayStruct(numElements, ptr);
    //}

    public byte[]? ReadSizePrefixedBytes()
    {
        uint numElements = this.ReadUInt32();
        nint ptr = this.ReadIntPtr();
        return new SizePrefixedByteArray(numElements, ptr).ToManagedArray();
    }

    public unsafe TStruct ReadStruct<TStruct, TUnmanaged>(Func<nint, TStruct> marshaller)
        where TUnmanaged : unmanaged
    {
        var ret = marshaller(this.Handle + this.offset);
        this.offset += sizeof(TUnmanaged);

        return ret;
    }

    public unsafe TStruct? ReadOptionalStruct<TStruct, TUnmanaged>(Func<TUnmanaged, TStruct> marshaller)
        where TStruct : struct
        where TUnmanaged : unmanaged
    {
        var ptr = this.ReadIntPtr();

        if (ptr == nint.Zero)
        {
            return null;
        }

        var ret = marshaller(*(TUnmanaged*)ptr);
        return ret;
    }

    public bool ReadBOOL() => this.ReadInt32() != 0;

    public unsafe string? ReadPWSTR()
    {
        nint ptr = this.ReadIntPtr();
        var ret = Utf16StringMarshaller.ConvertToManaged((ushort*)ptr);
        return ret;
    }

    public void IncrementOffset(int amount)
    {
        this.offset += amount;
    }
}
