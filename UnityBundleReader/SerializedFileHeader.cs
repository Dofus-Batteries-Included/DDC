namespace UnityBundleReader;

public class SerializedFileHeader
{
    public uint MMetadataSize;
    public long MFileSize;
    public SerializedFileFormatVersion MVersion;
    public long MDataOffset;
    public byte MEndianess;
    public byte[] Reserved = [];
}
