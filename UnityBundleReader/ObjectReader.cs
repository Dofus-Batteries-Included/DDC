namespace UnityBundleReader;

public class ObjectReader : EndianBinaryReader
{
    public readonly SerializedFile AssetsFile;
    public readonly long MPathID;
    public readonly long ByteStart;
    public readonly uint ByteSize;
    public readonly ClassIDType Type;
    public readonly SerializedType? SerializedType;
    public readonly BuildTarget Platform;
    public readonly SerializedFileFormatVersion MVersion;

    public int[] Version => AssetsFile.Version;
    public BuildType? BuildType => AssetsFile.BuildType;

    public ObjectReader(EndianBinaryReader reader, SerializedFile assetsFile, ObjectInfo objectInfo) : base(reader.BaseStream, reader.Endian)
    {
        AssetsFile = assetsFile;
        MPathID = objectInfo.PathId;
        ByteStart = objectInfo.ByteStart;
        ByteSize = objectInfo.ByteSize;
        if (Enum.IsDefined(typeof(ClassIDType), objectInfo.ClassID))
        {
            Type = (ClassIDType)objectInfo.ClassID;
        }
        else
        {
            Type = ClassIDType.UnknownType;
        }
        SerializedType = objectInfo.SerializedType;
        Platform = assetsFile.MTargetPlatform;
        MVersion = assetsFile.Header.MVersion;
    }

    public void Reset() => Position = ByteStart;
}
