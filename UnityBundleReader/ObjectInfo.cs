namespace UnityBundleReader;

public class ObjectInfo
{
    public long ByteStart;
    public uint ByteSize;
    public int TypeID;
    public int ClassID;
    public ushort IsDestroyed;
    public byte Stripped;

    public long PathId;
    public SerializedType? SerializedType;
}
