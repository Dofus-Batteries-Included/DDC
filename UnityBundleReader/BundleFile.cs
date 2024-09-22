using K4os.Compression.LZ4;
using UnityBundleReader.Extensions;

namespace UnityBundleReader;

[Flags]
public enum ArchiveFlags
{
    CompressionTypeMask = 0x3f,
    BlocksAndDirectoryInfoCombined = 0x40,
    BlocksInfoAtTheEnd = 0x80,
    OldWebPluginCompatibility = 0x100,
    BlockInfoNeedPaddingAtStart = 0x200
}

[Flags]
public enum StorageBlockFlags
{
    CompressionTypeMask = 0x3f,
    Streamed = 0x40
}

public enum CompressionType
{
    None,
    Lzma,
    Lz4,
    Lz4Hc,
    Lzham
}

public class BundleFile
{
    public readonly BundleFileHeader Header;
    BundleFileStorageBlock[] _mBlocksInfo = [];
    BundleFileNode[] _mDirectoryInfo = [];

    public StreamFile[] FileList = [];

    public BundleFile(FileReader reader)
    {
        string signature = reader.ReadStringToNull();
        uint version = reader.ReadUInt32();
        string unityVersion = reader.ReadStringToNull();
        string unityRevision = reader.ReadStringToNull();

        Header = new BundleFileHeader
        {
            Signature = signature,
            Version = version,
            UnityVersion = unityVersion,
            UnityRevision = unityRevision
        };

        switch (Header.Signature)
        {
            case "UnityArchive":
                break; //TODO
            case "UnityWeb":
            case "UnityRaw":
                if (Header.Version == 6)
                {
                    goto case "UnityFS";
                }
                ReadHeaderAndBlocksInfo(reader);
                using (Stream blocksStream = CreateBlocksStream(reader.FullPath))
                {
                    ReadBlocksAndDirectory(reader, blocksStream);
                    ReadFiles(blocksStream, reader.FullPath);
                }
                break;
            case "UnityFS":
                ReadHeader(reader);
                ReadBlocksInfoAndDirectory(reader);
                using (Stream blocksStream = CreateBlocksStream(reader.FullPath))
                {
                    ReadBlocks(reader, blocksStream);
                    ReadFiles(blocksStream, reader.FullPath);
                }
                break;
        }
    }

    void ReadHeaderAndBlocksInfo(EndianBinaryReader reader)
    {
        if (Header.Version >= 4)
        {
            byte[] hash = reader.ReadBytes(16);
            uint crc = reader.ReadUInt32();
        }
        uint minimumStreamedBytes = reader.ReadUInt32();
        Header.Size = reader.ReadUInt32();
        uint numberOfLevelsToDownloadBeforeStreaming = reader.ReadUInt32();
        int levelCount = reader.ReadInt32();
        _mBlocksInfo = new BundleFileStorageBlock[1];
        for (int i = 0; i < levelCount; i++)
        {
            BundleFileStorageBlock storageBlock = new()
            {
                CompressedSize = reader.ReadUInt32(),
                UncompressedSize = reader.ReadUInt32()
            };
            if (i == levelCount - 1)
            {
                _mBlocksInfo[0] = storageBlock;
            }
        }
        if (Header.Version >= 2)
        {
            uint completeFileSize = reader.ReadUInt32();
        }
        if (Header.Version >= 3)
        {
            uint fileInfoHeaderSize = reader.ReadUInt32();
        }
        reader.Position = Header.Size;
    }

    Stream CreateBlocksStream(string path)
    {
        Stream blocksStream;
        long uncompressedSizeSum = _mBlocksInfo.Sum(x => x.UncompressedSize);
        if (uncompressedSizeSum >= int.MaxValue)
        {
            /*var memoryMappedFile = MemoryMappedFile.CreateNew(null, uncompressedSizeSum);
            assetsDataStream = memoryMappedFile.CreateViewStream();*/
            blocksStream = new FileStream(path + ".temp", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
        }
        else
        {
            blocksStream = new MemoryStream((int)uncompressedSizeSum);
        }
        return blocksStream;
    }

    void ReadBlocksAndDirectory(EndianBinaryReader reader, Stream blocksStream)
    {
        bool isCompressed = Header.Signature == "UnityWeb";
        foreach (BundleFileStorageBlock? blockInfo in _mBlocksInfo)
        {
            byte[] uncompressedBytes = reader.ReadBytes((int)blockInfo.CompressedSize);
            if (isCompressed)
            {
                using (MemoryStream memoryStream = new(uncompressedBytes))
                {
                    using (MemoryStream decompressStream = SevenZipHelper.StreamDecompress(memoryStream))
                    {
                        uncompressedBytes = decompressStream.ToArray();
                    }
                }
            }
            blocksStream.Write(uncompressedBytes, 0, uncompressedBytes.Length);
        }
        blocksStream.Position = 0;
        EndianBinaryReader blocksReader = new(blocksStream);
        int nodesCount = blocksReader.ReadInt32();
        _mDirectoryInfo = new BundleFileNode[nodesCount];
        for (int i = 0; i < nodesCount; i++)
        {
            _mDirectoryInfo[i] = new BundleFileNode
            {
                Path = blocksReader.ReadStringToNull(),
                Offset = blocksReader.ReadUInt32(),
                Size = blocksReader.ReadUInt32()
            };
        }
    }

    public void ReadFiles(Stream blocksStream, string path)
    {
        FileList = new StreamFile[_mDirectoryInfo.Length];
        for (int i = 0; i < _mDirectoryInfo.Length; i++)
        {
            BundleFileNode node = _mDirectoryInfo[i];
            string nodePath = node.Path;
            string nodeFilename = Path.GetFileName(nodePath);
            Stream stream;
            if (node.Size >= int.MaxValue)
            {
                /*var memoryMappedFile = MemoryMappedFile.CreateNew(null, entryinfo_size);
                file.stream = memoryMappedFile.CreateViewStream();*/
                string extractPath = path + "_unpacked" + Path.DirectorySeparatorChar;
                Directory.CreateDirectory(extractPath);
                stream = new FileStream(extractPath + nodeFilename, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            else
            {
                stream = new MemoryStream((int)node.Size);
            }
            blocksStream.Position = node.Offset;
            blocksStream.CopyTo(stream, node.Size);
            stream.Position = 0;

            FileList[i] = new StreamFile
            {
                Path = nodePath,
                FileName = nodeFilename,
                Stream = stream
            };
        }
    }

    void ReadHeader(EndianBinaryReader reader)
    {
        Header.Size = reader.ReadInt64();
        Header.CompressedBlocksInfoSize = reader.ReadUInt32();
        Header.UncompressedBlocksInfoSize = reader.ReadUInt32();
        Header.Flags = (ArchiveFlags)reader.ReadUInt32();
        if (Header.Signature != "UnityFS")
        {
            reader.ReadByte();
        }
    }

    void ReadBlocksInfoAndDirectory(EndianBinaryReader reader)
    {
        byte[] blocksInfoBytes;
        if (Header.Version >= 7)
        {
            reader.AlignStream(16);
        }
        if ((Header.Flags & ArchiveFlags.BlocksInfoAtTheEnd) != 0)
        {
            long position = reader.Position;
            reader.Position = reader.BaseStream.Length - Header.CompressedBlocksInfoSize;
            blocksInfoBytes = reader.ReadBytes((int)Header.CompressedBlocksInfoSize);
            reader.Position = position;
        }
        else //0x40 BlocksAndDirectoryInfoCombined
        {
            blocksInfoBytes = reader.ReadBytes((int)Header.CompressedBlocksInfoSize);
        }
        MemoryStream blocksInfoUncompresseddStream;
        uint uncompressedSize = Header.UncompressedBlocksInfoSize;
        CompressionType compressionType = (CompressionType)(Header.Flags & ArchiveFlags.CompressionTypeMask);
        switch (compressionType)
        {
            case CompressionType.None:
            {
                blocksInfoUncompresseddStream = new MemoryStream(blocksInfoBytes);
                break;
            }
            case CompressionType.Lzma:
            {
                blocksInfoUncompresseddStream = new MemoryStream((int)uncompressedSize);
                using (MemoryStream blocksInfoCompressedStream = new(blocksInfoBytes))
                {
                    SevenZipHelper.StreamDecompress(blocksInfoCompressedStream, blocksInfoUncompresseddStream, Header.CompressedBlocksInfoSize, Header.UncompressedBlocksInfoSize);
                }
                blocksInfoUncompresseddStream.Position = 0;
                break;
            }
            case CompressionType.Lz4:
            case CompressionType.Lz4Hc:
            {
                byte[] uncompressedBytes = new byte[uncompressedSize];
                int numWrite = LZ4Codec.Decode(blocksInfoBytes, uncompressedBytes);
                if (numWrite != uncompressedSize)
                {
                    throw new IOException($"Lz4 decompression error, write {numWrite} bytes but expected {uncompressedSize} bytes");
                }
                blocksInfoUncompresseddStream = new MemoryStream(uncompressedBytes);
                break;
            }
            default:
                throw new IOException($"Unsupported compression type {compressionType}");
        }
        using (EndianBinaryReader blocksInfoReader = new(blocksInfoUncompresseddStream))
        {
            byte[] uncompressedDataHash = blocksInfoReader.ReadBytes(16);
            int blocksInfoCount = blocksInfoReader.ReadInt32();
            _mBlocksInfo = new BundleFileStorageBlock[blocksInfoCount];
            for (int i = 0; i < blocksInfoCount; i++)
            {
                _mBlocksInfo[i] = new BundleFileStorageBlock
                {
                    UncompressedSize = blocksInfoReader.ReadUInt32(),
                    CompressedSize = blocksInfoReader.ReadUInt32(),
                    Flags = (StorageBlockFlags)blocksInfoReader.ReadUInt16()
                };
            }

            int nodesCount = blocksInfoReader.ReadInt32();
            _mDirectoryInfo = new BundleFileNode[nodesCount];
            for (int i = 0; i < nodesCount; i++)
            {
                _mDirectoryInfo[i] = new BundleFileNode
                {
                    Offset = blocksInfoReader.ReadInt64(),
                    Size = blocksInfoReader.ReadInt64(),
                    Flags = blocksInfoReader.ReadUInt32(),
                    Path = blocksInfoReader.ReadStringToNull()
                };
            }
        }
        if ((Header.Flags & ArchiveFlags.BlockInfoNeedPaddingAtStart) != 0)
        {
            reader.AlignStream(16);
        }
    }

    void ReadBlocks(EndianBinaryReader reader, Stream blocksStream)
    {
        foreach (BundleFileStorageBlock? blockInfo in _mBlocksInfo)
        {
            CompressionType compressionType = (CompressionType)(blockInfo.Flags & StorageBlockFlags.CompressionTypeMask);
            switch (compressionType)
            {
                case CompressionType.None:
                {
                    reader.BaseStream.CopyTo(blocksStream, blockInfo.CompressedSize);
                    break;
                }
                case CompressionType.Lzma:
                {
                    SevenZipHelper.StreamDecompress(reader.BaseStream, blocksStream, blockInfo.CompressedSize, blockInfo.UncompressedSize);
                    break;
                }
                case CompressionType.Lz4:
                case CompressionType.Lz4Hc:
                {
                    int compressedSize = (int)blockInfo.CompressedSize;
                    byte[] compressedBytes = BigArrayPool<byte>.Shared.Rent(compressedSize);
                    reader.Read(compressedBytes, 0, compressedSize);
                    int uncompressedSize = (int)blockInfo.UncompressedSize;
                    byte[] uncompressedBytes = BigArrayPool<byte>.Shared.Rent(uncompressedSize);
                    int numWrite = LZ4Codec.Decode(compressedBytes, 0, compressedSize, uncompressedBytes, 0, uncompressedSize);
                    if (numWrite != uncompressedSize)
                    {
                        throw new IOException($"Lz4 decompression error, write {numWrite} bytes but expected {uncompressedSize} bytes");
                    }
                    blocksStream.Write(uncompressedBytes, 0, uncompressedSize);
                    BigArrayPool<byte>.Shared.Return(compressedBytes);
                    BigArrayPool<byte>.Shared.Return(uncompressedBytes);
                    break;
                }
                default:
                    throw new IOException($"Unsupported compression type {compressionType}");
            }
        }
        blocksStream.Position = 0;
    }
}

public class BundleFileHeader
{
    public required string Signature;
    public required uint Version;
    public required string UnityVersion;
    public required string UnityRevision;
    public long Size;
    public uint CompressedBlocksInfoSize;
    public uint UncompressedBlocksInfoSize;
    public ArchiveFlags Flags;
}

public class BundleFileNode
{
    public required long Offset;
    public required long Size;
    public required string Path;
    public uint Flags;
}

public class BundleFileStorageBlock
{
    public uint CompressedSize;
    public uint UncompressedSize;
    public StorageBlockFlags Flags;
}
