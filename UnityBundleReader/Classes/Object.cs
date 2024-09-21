using System.Collections.Specialized;

namespace UnityBundleReader.Classes;

public class Object
{
    public readonly SerializedFile AssetsFile;
    public readonly ObjectReader Reader;
    public readonly long MPathID;
    public readonly int[] Version;
    protected readonly BuildType BuildType;
    public readonly BuildTarget Platform;
    public ClassIDType Type;
    public readonly SerializedType? SerializedType;
    public readonly uint ByteSize;

    public Object(ObjectReader reader)
    {
        Reader = reader;
        reader.Reset();
        AssetsFile = reader.AssetsFile;
        Type = reader.Type;
        MPathID = reader.MPathID;
        Version = reader.Version;
        BuildType = reader.BuildType;
        Platform = reader.Platform;
        SerializedType = reader.SerializedType;
        ByteSize = reader.ByteSize;

        if (Platform == BuildTarget.NoTarget)
        {
            _ = reader.ReadUInt32();
        }
    }

    public string? Dump()
    {
        if (SerializedType?.MType != null)
        {
            return TypeTreeHelper.ReadTypeString(SerializedType.MType, Reader);
        }
        return null;
    }

    public string? Dump(TypeTree? mType)
    {
        if (mType != null)
        {
            return TypeTreeHelper.ReadTypeString(mType, Reader);
        }
        return null;
    }

    public OrderedDictionary? ToType()
    {
        if (SerializedType?.MType != null)
        {
            return TypeTreeHelper.ReadType(SerializedType.MType, Reader);
        }
        return null;
    }

    public OrderedDictionary? ToType(TypeTree? mType)
    {
        if (mType != null)
        {
            return TypeTreeHelper.ReadType(mType, Reader);
        }
        return null;
    }

    public byte[] GetRawData()
    {
        Reader.Reset();
        return Reader.ReadBytes((int)ByteSize);
    }
}
