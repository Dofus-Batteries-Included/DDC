using UnityBundleReader.Extensions;

namespace UnityBundleReader;

public class FileReader : EndianBinaryReader
{
    public readonly string FullPath;
    public readonly string FileName;
    public readonly FileType FileType;

    static readonly byte[] GzipMagic = [0x1f, 0x8b];
    static readonly byte[] BrotliMagic = [0x62, 0x72, 0x6F, 0x74, 0x6C, 0x69];
    static readonly byte[] ZipMagic = [0x50, 0x4B, 0x03, 0x04];
    static readonly byte[] ZipSpannedMagic = [0x50, 0x4B, 0x07, 0x08];

    public FileReader(string path) : this(path, File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) { }

    public FileReader(string path, Stream stream) : base(stream)
    {
        FullPath = Path.GetFullPath(path);
        FileName = Path.GetFileName(path);
        FileType = CheckFileType();
    }

    FileType CheckFileType()
    {
        string signature = this.ReadStringToNull(20);
        Position = 0;
        switch (signature)
        {
            case "UnityWeb":
            case "UnityRaw":
            case "UnityArchive":
            case "UnityFS":
                return FileType.BundleFile;
            case "UnityWebData1.0":
                return FileType.WebFile;
            default:
            {
                byte[] magic = ReadBytes(2);
                Position = 0;
                if (GzipMagic.SequenceEqual(magic))
                {
                    return FileType.GZipFile;
                }
                Position = 0x20;
                magic = ReadBytes(6);
                Position = 0;
                if (BrotliMagic.SequenceEqual(magic))
                {
                    return FileType.BrotliFile;
                }
                if (IsSerializedFile())
                {
                    return FileType.AssetsFile;
                }
                magic = ReadBytes(4);
                Position = 0;
                if (ZipMagic.SequenceEqual(magic) || ZipSpannedMagic.SequenceEqual(magic))
                {
                    return FileType.ZipFile;
                }
                return FileType.ResourceFile;
            }
        }
    }

    bool IsSerializedFile()
    {
        long fileSize = BaseStream.Length;
        if (fileSize < 20)
        {
            return false;
        }
        uint mMetadataSize = ReadUInt32();
        long mFileSize = ReadUInt32();
        uint mVersion = ReadUInt32();
        long mDataOffset = ReadUInt32();
        byte mEndianess = ReadByte();
        byte[] mReserved = ReadBytes(3);
        if (mVersion >= 22)
        {
            if (fileSize < 48)
            {
                Position = 0;
                return false;
            }
            mMetadataSize = ReadUInt32();
            mFileSize = ReadInt64();
            mDataOffset = ReadInt64();
        }
        Position = 0;
        if (mFileSize != fileSize)
        {
            return false;
        }
        if (mDataOffset > fileSize)
        {
            return false;
        }
        return true;
    }
}
