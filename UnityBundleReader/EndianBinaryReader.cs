using System.Buffers.Binary;

namespace UnityBundleReader;

public class EndianBinaryReader : BinaryReader
{
    readonly byte[] _buffer;

    public EndianType Endian;

    public EndianBinaryReader(Stream stream, EndianType endian = EndianType.BigEndian) : base(stream)
    {
        Endian = endian;
        _buffer = new byte[8];
    }

    public long Position {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }

    public override short ReadInt16()
    {
        if (Endian == EndianType.BigEndian)
        {
            Read(_buffer, 0, 2);
            return BinaryPrimitives.ReadInt16BigEndian(_buffer);
        }
        return base.ReadInt16();
    }

    public override int ReadInt32()
    {
        if (Endian == EndianType.BigEndian)
        {
            Read(_buffer, 0, 4);
            return BinaryPrimitives.ReadInt32BigEndian(_buffer);
        }
        return base.ReadInt32();
    }

    public override long ReadInt64()
    {
        if (Endian == EndianType.BigEndian)
        {
            Read(_buffer, 0, 8);
            return BinaryPrimitives.ReadInt64BigEndian(_buffer);
        }
        return base.ReadInt64();
    }

    public override ushort ReadUInt16()
    {
        if (Endian == EndianType.BigEndian)
        {
            Read(_buffer, 0, 2);
            return BinaryPrimitives.ReadUInt16BigEndian(_buffer);
        }
        return base.ReadUInt16();
    }

    public override uint ReadUInt32()
    {
        if (Endian == EndianType.BigEndian)
        {
            Read(_buffer, 0, 4);
            return BinaryPrimitives.ReadUInt32BigEndian(_buffer);
        }
        return base.ReadUInt32();
    }

    public override ulong ReadUInt64()
    {
        if (Endian == EndianType.BigEndian)
        {
            Read(_buffer, 0, 8);
            return BinaryPrimitives.ReadUInt64BigEndian(_buffer);
        }
        return base.ReadUInt64();
    }

    public override float ReadSingle()
    {
        if (Endian == EndianType.BigEndian)
        {
            Read(_buffer, 0, 4);
            Array.Reverse(_buffer, 0, 4);
            return BitConverter.ToSingle(_buffer, 0);
        }
        return base.ReadSingle();
    }

    public override double ReadDouble()
    {
        if (Endian == EndianType.BigEndian)
        {
            Read(_buffer, 0, 8);
            Array.Reverse(_buffer);
            return BitConverter.ToDouble(_buffer, 0);
        }
        return base.ReadDouble();
    }
}
